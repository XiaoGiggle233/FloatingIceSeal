using UnityEngine;

/// <summary>大泡泡移动控制 —— 蓄力时固定在生成位置，释放后上浮</summary>
public class BigBubbleMoveController : BubbleMoveBase
{
    private bool isReleased;

    protected override void Start()
    {
        base.Start();

        if (InformationPool.TryGet("BlowBubbleSpawnPos", out Vector3 spawnPos))
        {
            transform.position = spawnPos;
        }
    }

    private void OnEnable()
    {
        GameEvents.Listen(EventType.BUBBLE_EVENT_ON_RELEASE, OnBubbleRelease);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.BUBBLE_EVENT_ON_RELEASE, OnBubbleRelease);
    }

    private void OnBubbleRelease(IGameEvent evt)
    {
        var args = evt as BubbleBlowEventArgs;
        if (args == null) return;
        if (args.Bubble != this.gameObject) return;

        isReleased = true;
        if (bubbleBase is BigBubble bigBubble)
        {
            bigBubble.SetState(BigBubbleState.AfterRelease);
        }
    }

    protected override void Move()
    {
        if (!isReleased) return;

        Vector2 velocity = rb.velocity;
        velocity.y = Mathf.Clamp(velocity.y, -speed, speed);
        rb.velocity = velocity;
    }
}
