using UnityEngine;

/// <summary>小气泡控制 —— 继承基类，增加上升距离破裂条件</summary>
public class SmallBubbleController : BubbleControllerBase
{
    private SmallBubble smallBubble;
    private float startY;

    protected override void Start()
    {
        base.Start();
        smallBubble = bubbleBase as SmallBubble;
        startY = transform.position.y;
    }

    protected override void Update()
    {
        base.Update();

        if (smallBubble != null && transform.position.y - startY > smallBubble.riseBurstDistance)
        {
            bubbleBase.Burst();
        }
    }
}
