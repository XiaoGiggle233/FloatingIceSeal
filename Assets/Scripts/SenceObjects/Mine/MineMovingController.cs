using UnityEngine;

/// <summary>
/// 水雷移动控制器 —— 与浮冰相同：偏离后向初始位置回归；被气泡碰撞时被推开
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MineMovingController : MonoBehaviour
{
    [Header("回归速度")]
    [SerializeField] private float returnSpeed = 1f;

    [Header("气泡推力")]
    [SerializeField] private float bubblePushForce = 5f;

    [Header("归位阈值")]
    [SerializeField] private float snapDistance = 0.05f;

    private Vector2 initialPosition;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void FixedUpdate()
    {
        Vector2 currentPos = rb.position;
        Vector2 offset = initialPosition - currentPos;

        // 已归位 → 固定
        if (offset.magnitude < snapDistance)
        {
            rb.MovePosition(initialPosition);
            rb.velocity = Vector2.zero;
            return;
        }

        // 向初始位置移动（回归倾向）
        Vector2 direction = offset.normalized;
        rb.MovePosition(currentPos + direction * returnSpeed * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PushByBubble(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        PushByBubble(collision);
    }

    private void PushByBubble(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<BubbleBase>() == null) return;

        // 沿接触法线推开（法线指向水雷）；无接触点则退回中心差方向
        Vector2 dir = collision.contactCount > 0
            ? collision.contacts[0].normal
            : (rb.position - (Vector2)collision.transform.position).normalized;
        if (dir == Vector2.zero) dir = Vector2.up;
        rb.AddForce(dir * bubblePushForce, ForceMode2D.Impulse);
    }
}
