using UnityEngine;
using UnityEngine.Tilemaps;

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
