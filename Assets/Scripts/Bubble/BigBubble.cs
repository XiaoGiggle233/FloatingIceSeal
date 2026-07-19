using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BigBubbleState { BeforeRelease, AfterRelease }

public class BigBubble : BubbleBase
{
    public BigBubbleState State { get; private set; } = BigBubbleState.BeforeRelease;

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
}