using UnityEngine;

/// <summary>小气泡控制 —— 继承基类，增加上升时间破裂条件</summary>
public class SmallBubbleController : BubbleControllerBase
{
    private SmallBubble smallBubble;
    private float startTime;

    protected override void Start()
    {
        base.Start();
        smallBubble = bubbleBase as SmallBubble;
        startTime = Time.time;
    }

    protected override void Update()
    {
        base.Update();

        if (smallBubble != null && Time.time - startTime > smallBubble.riseBurstTime)
        {
            bubbleBase.Burst();
        }
    }
}
