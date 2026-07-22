using UnityEngine;

/// <summary>
/// 移动方向状态机控制器
/// 每帧根据 Rigidbody2D 速度向量角度更新 DirectionStateMachine。
/// 8 个方向各占 45°，初始方向为 Right。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class SealDirectionController : MonoBehaviour
{
    [Header("阈值")]
    [SerializeField] private float velocityThreshold = 0.01f;

    private Seal seal;
    private Rigidbody2D rb;

    private void Awake()
    {
        seal = GetComponent<Seal>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        seal.DirectionStateMachine.SetState(DirectionState.Right);
    }

    private void Update()
    {
        Vector2 velocity = rb.velocity;

        if (velocity.sqrMagnitude < velocityThreshold * velocityThreshold)
            return; // 静止时保持当前方向

        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        DirectionState dir = AngleToDirection(angle);

        seal.DirectionStateMachine.SetState(dir);
    }

    #region 角度转换

    private static DirectionState AngleToDirection(float angle)
    {
        if (angle >= -22.5f && angle < 22.5f)
            return DirectionState.Right;
        if (angle >= 22.5f && angle < 67.5f)
            return DirectionState.UpRight;
        if (angle >= 67.5f && angle < 112.5f)
            return DirectionState.Up;
        if (angle >= 112.5f && angle < 157.5f)
            return DirectionState.UpLeft;
        if (angle >= 157.5f || angle < -157.5f)
            return DirectionState.Left;
        if (angle >= -157.5f && angle < -112.5f)
            return DirectionState.DownLeft;
        if (angle >= -112.5f && angle < -67.5f)
            return DirectionState.Down;
        // -67.5f ~ -22.5f
        return DirectionState.DownRight;
    }

    #endregion
}
