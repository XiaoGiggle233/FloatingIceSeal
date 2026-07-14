using UnityEngine;

/// <summary>
/// 碰撞事件发布接口
/// 实现此接口的类具备发布触发器碰撞事件的能力
/// </summary>
public interface ICollisionEventPublisher
{
    /// <summary>发布碰撞事件</summary>
    /// <param name="other">碰撞的对方对象</param>
    void PublishCollisionEvent(GameObject other);
}
