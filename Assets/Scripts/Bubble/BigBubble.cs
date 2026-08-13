using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BigBubbleState { BeforeRelease, AfterRelease }

public class BigBubble : BubbleBase, ILevelResetable
{
    public BigBubbleState State { get; private set; } = BigBubbleState.BeforeRelease;

    public BubbleBreakStateMachine BreakStateMachine { get; private set; } = new BubbleBreakStateMachine();

    public BubbleAbsorbStateMachine AbsorbStateMachine { get; private set; } = new BubbleAbsorbStateMachine();

    /// <summary>是否正在被小鱼破坏</summary>
    public bool IsBeingBroken => BreakStateMachine.IsBreaking();

    /// <summary>是否正在被海豹吸收</summary>
    public bool IsBeingAbsorbed => AbsorbStateMachine.IsAbsorbing();

    /// <summary>关卡恢复完成回调（场景自带的大气泡参与重置；角色吐出的不参与）</summary>
    public void OnLevelRestore()
    {
    }

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

    /// <summary>设置是否正在被海豹吸收</summary>
    public void SetAbsorbing(bool absorbing)
    {
        AbsorbStateMachine.SetState(absorbing ? BubbleAbsorbState.Absorbing : BubbleAbsorbState.NotAbsorbing);
    }
}