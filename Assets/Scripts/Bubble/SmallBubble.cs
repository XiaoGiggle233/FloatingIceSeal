using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallBubble : BubbleBase
{
    //气泡的持续时间
    [SerializeField]public float duration;

    public override void Burst()
    {
        GameEvents.Publish(EventType.BUBBLE_EVENT_ON_BURST,
            new BubbleBurstEventArgs(gameObject));
        Destroy(gameObject);
    }
}
