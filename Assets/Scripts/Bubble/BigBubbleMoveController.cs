using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 大泡泡移动控制 —— 释放后由物理驱动上浮（linearDrag 限制终端速度），
/// 不再覆盖速度，因此流动水等外力可推动气泡；被小鱼破坏时上浮力大幅降低
/// </summary>
public class BigBubbleMoveController : BubbleMoveBase
{
    [FoldoutGroup("被小鱼破坏", expanded: true)]
    [LabelText("被破坏时上浮力倍率"), Range(0, 1), SuffixLabel("倍", Overlay = true)]
    [SerializeField] private float breakSpeedFactor = 0.2f;

    [FoldoutGroup("被海豹吸收", expanded: true)]
    [LabelText("被吸收时上浮力倍率"), Range(0, 1), SuffixLabel("倍", Overlay = true)]
    [SerializeField] private float absorbSpeedFactor = 0.2f;

    private bool isReleased;
    private BigBubble bigBubble;
    private float defaultGravityScale;

    protected override void Start()
    {
        base.Start();
        bigBubble = bubbleBase as BigBubble;
        defaultGravityScale = rb.gravityScale;

        if (rb.simulated)
        {
            // 场景直接放置（玩家蓄力生成时刚体 simulated=false）→ 初始即释放，进入 AfterRelease
            isReleased = true;
            bigBubble?.SetState(BigBubbleState.AfterRelease);
        }
        else if (InformationPool.TryGet("BlowBubbleSpawnPos", out Vector3 spawnPos))
        {
            // 玩家蓄力生成 → 定位到生成位置，等待释放事件
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

        // 被小鱼破坏 / 被海豹吸收 → 上浮力大幅降低；否则恢复默认（不覆盖速度，水流可推动气泡）
        float targetGravity = defaultGravityScale;
        if (bigBubble != null)
        {
            if (bigBubble.IsBeingBroken)
                targetGravity = defaultGravityScale * breakSpeedFactor;
            else if (bigBubble.IsBeingAbsorbed)
                targetGravity = defaultGravityScale * absorbSpeedFactor;
        }

        if (Mathf.Abs(rb.gravityScale - targetGravity) > 0.0001f)
            rb.gravityScale = targetGravity;
    }
}
