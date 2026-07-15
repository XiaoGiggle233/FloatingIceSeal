using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBubble : BubbleBase
{
    public override void Burst()
    {
        GameEvents.Publish(EventType.BUBBLE_EVENT_ON_BURST,
            new BubbleBurstEventArgs(gameObject));
        Destroy(gameObject);
    }
}