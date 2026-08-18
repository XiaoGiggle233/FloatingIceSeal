using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum FlowDirection
{
    [LabelText("不流动")]
    None,
    [LabelText("↑ 上")]
    Up,
    [LabelText("↓ 下")]
    Down,
    [LabelText("← 左")]
    Left,
    [LabelText("→ 右")]
    Right
}

public class FlowingWatter : MonoBehaviour
{
    [FoldoutGroup("水流设置", expanded: true)]
    [LabelText("流动方向")]
    [EnumToggleButtons]
    [SerializeField] private FlowDirection flowDirection = FlowDirection.None;

    [FoldoutGroup("水流设置")]
    [LabelText("推力大小")]
    [MinValue(0)]
    [SuffixLabel("N", Overlay = true)]
    [SerializeField] private float forceAmount = 10f;

    /// <summary>
    /// 推力大小（供水流效果等子物体脚本读取）
    /// </summary>
    public float ForceAmount => forceAmount;

    /// <summary>
    /// 流动方向（供水流效果脚本读取）
    /// </summary>
    public FlowDirection FlowDirection => flowDirection;

    #region 碰撞事件

    private void OnEnable()
    {
        GameEvents.Listen(EventType.COLLISION_EVENT_ON_ENTER, OnCollisionEvent);
        GameEvents.Listen(EventType.COLLISION_EVENT_ON_TRIGGER, OnCollisionEvent);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.COLLISION_EVENT_ON_ENTER, OnCollisionEvent);
        GameEvents.Unlisten(EventType.COLLISION_EVENT_ON_TRIGGER, OnCollisionEvent);
    }

    private void OnCollisionEvent(IGameEvent evt)
    {
        var args = evt as CollisionEventArgs;
        if (args == null) return;
        if (args.Source != this.gameObject && args.Target != this.gameObject) return;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (flowDirection == FlowDirection.None) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb == null) return;

        Vector2 direction = GetFlowDirectionVector();
        rb.AddForce(direction * forceAmount);
    }

    #endregion

    private Vector2 GetFlowDirectionVector()
    {
        switch (flowDirection)
        {
            case FlowDirection.Up:    return Vector2.up;
            case FlowDirection.Down:  return Vector2.down;
            case FlowDirection.Left:  return Vector2.left;
            case FlowDirection.Right: return Vector2.right;
            default:                  return Vector2.zero;
        }
    }
}
