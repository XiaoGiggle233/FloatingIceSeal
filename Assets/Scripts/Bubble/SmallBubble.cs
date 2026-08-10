using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallBubble : BubbleBase
{
    /// <summary>上升存活时间（秒），超过即破裂</summary>
    [SerializeField] public float riseBurstTime = 5f;

    public override void Burst()
    {
        GameEvents.Publish(EventType.BUBBLE_EVENT_ON_BURST,
            new BubbleBurstEventArgs(gameObject));
        Destroy(gameObject);
    }
}
