using UnityEngine;

/// <summary>
/// 碰撞事件发布接口
/// 实现此接口的类具备发布物理碰撞和触发器碰撞事件的能力
/// </summary>
public interface ICollisionEventPublisher
{
    /// <summary>发布物理碰撞进入事件</summary>
    /// <param name="other">碰撞的对方对象</param>
    /// <param name="normal">碰撞法线</param>
    void PublishCollisionEvent(GameObject other, Vector2 normal);

    /// <summary>发布触发器碰撞事件</summary>
    /// <param name="other">进入触发器的对方对象</param>
    void PublishTriggerEvent(GameObject other);
}
