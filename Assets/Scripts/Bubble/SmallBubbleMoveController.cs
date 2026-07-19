using UnityEngine;

/// <summary>小泡泡移动控制 —— 限制上浮速度</summary>
public class SmallBubbleMoveController : BubbleMoveBase
{
    protected override void Move()
    {
        Vector2 velocity = rb.velocity;
        velocity.y = Mathf.Clamp(velocity.y, -speed, speed);
        rb.velocity = velocity;
    }
}
