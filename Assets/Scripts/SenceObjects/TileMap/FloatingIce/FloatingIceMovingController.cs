using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>浮冰移动参数快照 —— 关卡重置从 prefab 重建后还原场景 Inspector 覆盖值</summary>
public struct FloatingIceMovingSettings
{
    public float waterReturnSpeed;
    public float airReturnSpeed;
    public float waterCheckDistance;
    public float snapDistance;

    public FloatingIceMovingSettings(float waterReturnSpeed, float airReturnSpeed,
        float waterCheckDistance, float snapDistance)
    {
        this.waterReturnSpeed = waterReturnSpeed;
        this.airReturnSpeed = airReturnSpeed;
        this.waterCheckDistance = waterCheckDistance;
        this.snapDistance = snapDistance;
    }
}

/// <summary>
/// 浮冰移动控制器
/// 控制 TileMap 在偏离初始位置后以不同速度回归（水中/空中速度不同）
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class FloatingIceMovingController : MonoBehaviour
{
    [Header("回归速度")]
    [SerializeField] private float waterReturnSpeed = 1f;
    [SerializeField] private float airReturnSpeed = 3f;

    [Header("水面检测")]
    [SerializeField] private float waterCheckDistance = 5f;

    [Header("归位阈值")]
    [SerializeField] private float snapDistance = 0.05f;

    private Vector2 initialPosition;
    private Rigidbody2D rb;
    private Tilemap tilemap;
    private CompositeCollider2D compositeCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        tilemap = GetComponent<Tilemap>();
        compositeCollider = GetComponent<CompositeCollider2D>();
    }

    private void Start()
    {
        initialPosition = transform.position;
    }

    /// <summary>捕获当前移动参数（关卡重置重建后由 LevelResetSystem 还原）</summary>
    public FloatingIceMovingSettings CaptureSettings() =>
        new FloatingIceMovingSettings(waterReturnSpeed, airReturnSpeed, waterCheckDistance, snapDistance);

    /// <summary>还原移动参数（关卡重置重建后由 LevelResetSystem 调用）</summary>
    public void RestoreSettings(FloatingIceMovingSettings settings)
    {
        waterReturnSpeed = settings.waterReturnSpeed;
        airReturnSpeed = settings.airReturnSpeed;
        waterCheckDistance = settings.waterCheckDistance;
        snapDistance = settings.snapDistance;
    }

    private void FixedUpdate()
    {
        Vector2 currentPos = rb.position;
        Vector2 offset = initialPosition - currentPos;

        if (offset.magnitude < snapDistance)
        {
            rb.MovePosition(initialPosition);
            rb.velocity = Vector2.zero;
            return;
        }

        bool inWater = WatterUtils.HasWaterBelow(currentPos, waterCheckDistance);
        float speed = inWater ? waterReturnSpeed : airReturnSpeed;

        Vector2 direction = offset.normalized;
        rb.MovePosition(currentPos + direction * speed * Time.fixedDeltaTime);
    }
}
