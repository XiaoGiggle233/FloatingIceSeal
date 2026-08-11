using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BigBubbleState { BeforeRelease, AfterRelease }

public class BigBubble : BubbleBase
{
    public BigBubbleState State { get; private set; } = BigBubbleState.BeforeRelease;

    public BubbleBreakStateMachine BreakStateMachine { get; private set; } = new BubbleBreakStateMachine();

    /// <summary>是否正在被小鱼破坏</summary>
    public bool IsBeingBroken => BreakStateMachine.IsBreaking();

    public override void Burst()
    {
        if (State == BigBubbleState.BeforeRelease) return;

        GameEvents.Publish(EventType.BUBBLE_EVENT_ON_BURST,
            new BubbleBurstEventArgs(gameObject));
        Destroy(gameObject);
    }

    public void SetState(BigBubbleState newState)
    {
        State = newState;
    }

    /// <summary>设置是否正在被小鱼破坏</summary>
    public void SetBreaking(bool breaking)
    {
        BreakStateMachine.SetState(breaking ? BubbleBreakState.Breaking : BubbleBreakState.NotBreaking);
    }
}