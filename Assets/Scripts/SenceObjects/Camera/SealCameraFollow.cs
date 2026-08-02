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

    public Rigidbody2D TargetRb { get; private set; }
    public CameraStateMachine StateMachine { get; private set; }

    private void Awake()
    {
        StateMachine = new CameraStateMachine(this);
    }

    private void Start()
    {
        if (_target == null)
            _target = InformationPool.Get<Seal>("Seal");

        if (_target != null)
            TargetRb = _target.GetComponent<Rigidbody2D>();

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

        ApplyMovement();
        StateMachine.Update();
    }

    private void ApplyMovement()
    {
        Vector3 targetPos = (Vector3)MoveTarget + Offset;
        targetPos.z = transform.position.z;

        targetPos = ClampWithinBounds(targetPos);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, MoveSpeed * Time.deltaTime);
    }

    private Vector3 ClampWithinBounds(Vector3 pos)
    {
        if (_boundTopLeft == null || _boundBottomRight == null) return pos;

        float minX = _boundTopLeft.position.x;
        float maxX = _boundBottomRight.position.x;
        float maxY = _boundTopLeft.position.y;
        float minY = _boundBottomRight.position.y;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        return pos;
    }

    public void SetTarget(Seal seal)
    {
        _target = seal;
        TargetRb = seal != null ? seal.GetComponent<Rigidbody2D>() : null;
    }
}
