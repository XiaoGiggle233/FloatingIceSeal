using UnityEngine;
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

    public Rigidbody2D TargetRb { get; private set; }
    public CameraStateMachine StateMachine { get; private set; }
    private Camera _cam;

    private Vector3 _cachedTopLeft;
    private Vector3 _cachedBottomRight;
    private bool _boundsCached;

    [FoldoutGroup("调试", expanded: true)]
    [ShowInInspector, ReadOnly, LabelText("当前摄像机状态")]
    public CameraState CurrentCameraState => StateMachine?.CurrentState ?? CameraState.Follow;

    [FoldoutGroup("调试")]
    [ShowInInspector, ReadOnly, LabelText("当前状态对象")]
    public string CurrentLeafState => StateMachine?.CurrentLeafStateName ?? "Null";

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        StateMachine = new CameraStateMachine(this);
    }

    private void Start()
    {
        if (_target == null)
            _target = InformationPool.Get<Seal>("Seal");

        if (_target != null)
            TargetRb = _target.GetComponent<Rigidbody2D>();

        CacheBounds();
        StateMachine.EnterLeafState(StateMachine.FollowState);
        StateMachine.SetState(CameraState.Follow);
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

        StateMachine.Update();
    }

    /// <summary>
    /// 以指定速度朝向目标匀速移动（带边界钳制）
    /// </summary>
    public void MoveTo(Vector3 targetPos, float speed)
    {
        targetPos.z = transform.position.z;
        targetPos = ClampWithinBounds(targetPos);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
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

    public void SetTarget(Seal seal)
    {
        _target = seal;
        TargetRb = seal != null ? seal.GetComponent<Rigidbody2D>() : null;
    }
}
