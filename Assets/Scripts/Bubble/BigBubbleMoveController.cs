using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>大泡泡移动控制 —— 蓄力时固定在生成位置，释放后上浮；被小鱼破坏时上浮速度大幅降低</summary>
public class BigBubbleMoveController : BubbleMoveBase
{
    [FoldoutGroup("被小鱼破坏", expanded: true)]
    [LabelText("被破坏时上浮速度倍率"), Range(0, 1), SuffixLabel("倍", Overlay = true)]
    [SerializeField] private float breakSpeedFactor = 0.2f;

    private bool isReleased;
    private BigBubble bigBubble;

    protected override void Start()
    {
        base.Start();
        bigBubble = bubbleBase as BigBubble;

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

        // 被小鱼破坏时上浮速度大幅降低
        float limit = (bigBubble != null && bigBubble.IsBeingBroken)
            ? speed * breakSpeedFactor
            : speed;

        Vector2 velocity = rb.velocity;
        velocity.y = Mathf.Clamp(velocity.y, -limit, limit);
        rb.velocity = velocity;
    }
}
