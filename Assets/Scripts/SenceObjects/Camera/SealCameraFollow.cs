using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using Sirenix.OdinInspector;

/// <summary>
/// 摄像机系统入口，管理摄像机状态机与移动逻辑。
/// 优先从 InformationPool 获取 Seal 引用，也支持直接拖拽赋值。
/// 在 Camera 下创建两个子物体作为移动边界：BoundTopLeft、BoundBottomRight。
/// </summary>
public class SealCameraFollow : MonoBehaviour
{
    [FoldoutGroup("目标")]
    [SerializeField, LabelText("跟随目标")]
    private Seal _target;

    [FoldoutGroup("移动设置")]
    [SerializeField, LabelText("移动速度"), SuffixLabel("m/s", Overlay = true)]
    public float MoveSpeed = 5f;

    [FoldoutGroup("移动设置")]
    [SerializeField, LabelText("移动目标")]
    public Vector2 MoveTarget;

    [FoldoutGroup("移动设置")]
    [SerializeField, LabelText("偏移")]
    public Vector3 Offset = new Vector3(0, 0, -10);

    [FoldoutGroup("移动边界")]
    [SerializeField, LabelText("边界左上角")]
    private Transform _boundTopLeft;

    [FoldoutGroup("移动边界")]
    [SerializeField, LabelText("边界右下角")]
    private Transform _boundBottomRight;

    [FoldoutGroup("跟随缓冲")]
    [SerializeField, LabelText("缓冲宽度"), MinValue(0)]
    public float FollowBufferWidth = 3f;

    [FoldoutGroup("跟随缓冲")]
    [SerializeField, LabelText("缓冲高度"), MinValue(0)]
    public float FollowBufferHeight = 2f;

    [FoldoutGroup("跟随缓冲")]
    [SerializeField, LabelText("等比系数"), MinValue(1)]
    public float RatioMultiplier = 2f;

    [FoldoutGroup("跟随缓冲")]
    [SerializeField, LabelText("速度等级距离"), SuffixLabel("m", Overlay = true), MinValue(0.01f)]
    public float SpeedStepDistance = 2f;

    [FoldoutGroup("纵向缓冲")]
    [SerializeField, LabelText("缓冲宽度"), MinValue(0)]
    public float VerticalBufferWidth = 3f;

    [FoldoutGroup("纵向缓冲")]
    [SerializeField, LabelText("缓冲高度"), MinValue(0)]
    public float VerticalBufferHeight = 2f;

    [FoldoutGroup("纵向缓冲")]
    [SerializeField, LabelText("等比系数"), MinValue(1)]
    public float VerticalRatioMultiplier = 2f;

    [FoldoutGroup("纵向缓冲")]
    [SerializeField, LabelText("速度等级距离"), SuffixLabel("m", Overlay = true), MinValue(0.01f)]
    public float VerticalSpeedStepDistance = 2f;

    public Rigidbody2D TargetRb { get; private set; }
    public CameraStateMachine StateMachine { get; private set; }
    private Camera _cam;
    private PixelPerfectCamera _pixelPerfect;

    private Vector3 _cachedTopLeft;
    private Vector3 _cachedBottomRight;
    private bool _boundsCached;

    // 锁定状态数据
    public CameraLockData ActiveLockData { get; private set; }
    public float OriginalViewSize { get; private set; }
    public bool RestoreView { get; private set; }
    public float ViewRestoreSpeed { get; private set; }
    private bool _exitPending;
    private float _lockExitTimer;

    // 全览状态数据
    public CameraOverviewData ActiveOverviewData { get; private set; }

    [FoldoutGroup("全览设置")]
    [SerializeField, LabelText("返回角色速度"), MinValue(0.01f), SuffixLabel("unit/s", Overlay = true)]
    public float OverviewReturnSpeed = 5f;

    [FoldoutGroup("状态设置")]
    [SerializeField, LabelText("默认状态"), EnumToggleButtons, Tooltip("摄像机退出锁定/全览等状态后进入的状态")]
    private CameraState _defaultState = CameraState.Follow;

    public CameraState DefaultState => _defaultState;

    [FoldoutGroup("调试", expanded: true)]
    [ShowInInspector, ReadOnly, LabelText("当前摄像机状态")]
    public CameraState CurrentCameraState => StateMachine?.CurrentState ?? DefaultState;

    [FoldoutGroup("调试")]
    [ShowInInspector, ReadOnly, LabelText("当前状态对象")]
    public string CurrentLeafState => StateMachine?.CurrentLeafStateName ?? "Null";

    private void OnEnable()
    {
        InformationPool.Set("SealCameraFollow", this);
    }

    private void OnDisable()
    {
        InformationPool.Remove("SealCameraFollow");
    }

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _pixelPerfect = GetComponent<PixelPerfectCamera>();
        StateMachine = new CameraStateMachine(this);
    }

    private void Start()
    {
        if (_target == null)
            _target = InformationPool.Get<Seal>("Seal");

        if (_target != null)
            TargetRb = _target.GetComponent<Rigidbody2D>();

        CacheBounds();
        StateMachine.EnterState(DefaultState);
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            _target = InformationPool.Get<Seal>("Seal");
            if (_target != null)
                TargetRb = _target.GetComponent<Rigidbody2D>();
            return;
        }

        // 退出锁定缓冲倒计时
        if (_exitPending)
        {
            _lockExitTimer -= Time.deltaTime;
            if (_lockExitTimer <= 0f)
            {
                _exitPending = false;
                DoReleaseLock();
            }
        }

        StateMachine.Update();
    }

    /// <summary>
    /// 以指定速度朝向目标匀速移动（带边界钳制），返回实际移动目标
    /// </summary>
    public Vector3 MoveTo(Vector3 targetPos, float speed)
    {
        targetPos.z = transform.position.z;
        targetPos = ClampWithinBounds(targetPos);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        return targetPos;
    }

    public Vector3 ClampWithinBounds(Vector3 pos)
    {
        if (!_boundsCached || _cam == null) return pos;

        float minX = _cachedTopLeft.x;
        float maxX = _cachedBottomRight.x;
        float maxY = _cachedTopLeft.y;
        float minY = _cachedBottomRight.y;

        // 相机可视范围半宽半高
        float halfHeight = _cam.orthographicSize;
        float halfWidth = halfHeight * _cam.aspect;

        // 可视范围大于边界时居中
        if (maxX - minX < halfWidth * 2f)
            pos.x = (minX + maxX) * 0.5f;
        else
            pos.x = Mathf.Clamp(pos.x, minX + halfWidth, maxX - halfWidth);

        if (maxY - minY < halfHeight * 2f)
            pos.y = (minY + maxY) * 0.5f;
        else
            pos.y = Mathf.Clamp(pos.y, minY + halfHeight, maxY - halfHeight);

        return pos;
    }

    /// <summary>
    /// 缓存边界物体的世界坐标，供移动钳制使用
    /// </summary>
    public void CacheBounds()
    {
        if (_boundTopLeft == null || _boundBottomRight == null) return;

        _cachedTopLeft = _boundTopLeft.position;
        _cachedBottomRight = _boundBottomRight.position;
        _boundsCached = true;
    }

    /// <summary>
    /// 编辑器中绘制缓冲范围矩形，便于配置缓冲宽度/高度
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        DrawBufferRect(FollowBufferWidth, FollowBufferHeight, new Color(1f, 0.85f, 0.2f, 0.6f));
        DrawBufferRect(VerticalBufferWidth, VerticalBufferHeight, new Color(0f, 1f, 1f, 0.6f));
    }

    private void DrawBufferRect(float width, float height, Color color)
    {
        if (width <= 0f || height <= 0f) return;

        Gizmos.color = color;
        Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0.1f));
    }

    /// <summary>
    /// 匀速过渡正交视野大小
    /// </summary>
    public void TransitionViewSize(float target, float speed)
    {
        _cam.orthographicSize = Mathf.MoveTowards(_cam.orthographicSize, target, speed * Time.deltaTime);
    }

    private void SetPixelPerfectEnabled(bool enabled)
    {
        if (_pixelPerfect != null && _pixelPerfect.enabled != enabled)
            _pixelPerfect.enabled = enabled;
    }

    /// <summary>
    /// 视野是否已恢复到原始大小
    /// </summary>
    public bool IsViewRestored()
    {
        return !RestoreView || Mathf.Approximately(_cam.orthographicSize, OriginalViewSize);
    }

    /// <summary>
    /// 重新启用 Pixel Perfect Camera
    /// </summary>
    public void ReenablePixelPerfect()
    {
        SetPixelPerfectEnabled(true);
    }

    /// <summary>
    /// 进入锁定状态：记录原视野，应用锁定数据
    /// </summary>
    public void ApplyLock(CameraLockData data)
    {
        if (data == null) return;

        // 重新进入时取消待执行的退出
        _exitPending = false;

        ActiveLockData = data;
        OriginalViewSize = _cam.orthographicSize;
        RestoreView = data.AdjustViewSize;
        ViewRestoreSpeed = data.ViewTransitionSpeed;

        // PixelPerfectCamera 会强制覆盖位置与视野，锁定期间关闭
        if (data.MoveToPosition || data.AdjustViewSize)
            SetPixelPerfectEnabled(false);

        StateMachine.EnterLeafState(StateMachine.LockState);
        StateMachine.SetState(CameraState.Lock);
    }

    /// <summary>
    /// 退出锁定状态：经缓冲时间后恢复跟随状态
    /// </summary>
    public void ReleaseLock()
    {
        if (!StateMachine.IsLock()) return;

        _exitPending = true;
        _lockExitTimer = ActiveLockData != null ? ActiveLockData.LockExitBufferTime : 0f;
    }

    private void DoReleaseLock()
    {
        StateMachine.EnterState(DefaultState);
        ActiveLockData = null;
    }

    /// <summary>
    /// 进入全览状态：执行全览指令列表
    /// </summary>
    public void ApplyOverview(CameraOverviewData data)
    {
        if (data == null) return;

        // 取消待执行的锁定退出
        _exitPending = false;

        ActiveOverviewData = data;
        // PixelPerfectCamera 会强制覆盖位置，全览期间关闭
        SetPixelPerfectEnabled(false);

        StateMachine.EnterLeafState(StateMachine.OverviewState);
        StateMachine.SetState(CameraState.Overview);
    }

    /// <summary>
    /// 退出全览状态：进入默认状态
    /// </summary>
    public void ReleaseOverview()
    {
        if (!StateMachine.IsOverview()) return;

        StateMachine.EnterState(DefaultState);
        ActiveOverviewData = null;
    }
    public void SetTarget(Seal seal)
    {
        _target = seal;
        TargetRb = seal != null ? seal.GetComponent<Rigidbody2D>() : null;
    }
}
