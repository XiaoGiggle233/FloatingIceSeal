using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>气泡参数快照 —— 关卡重置从 prefab 重建后还原场景 Inspector 覆盖值</summary>
public struct BubbleSettings
{
    public float oxygen;
    public float speed;
    public float outOfWaterBurstDistance;

    public BubbleSettings(float oxygen, float speed, float outOfWaterBurstDistance)
    {
        this.oxygen = oxygen;
        this.speed = speed;
        this.outOfWaterBurstDistance = outOfWaterBurstDistance;
    }
}

public abstract class BubbleBase : MonoBehaviour, ICollisionEventPublisher
{
    //储存的氧气
    [SerializeField]public float oxygen;
    //上浮的速度
    [SerializeField]public float speed;

    //离开水面超过此距离（单位）即破裂
    [SerializeField]public float outOfWaterBurstDistance = 0.5f;

    //泡泡破裂
    [Button("Burst", ButtonSizes.Large)]
    public abstract void Burst();

    /// <summary>捕获当前气泡参数（关卡重置重建后由 LevelResetSystem 还原）</summary>
    public BubbleSettings CaptureSettings() =>
        new BubbleSettings(oxygen, speed, outOfWaterBurstDistance);

    /// <summary>还原气泡参数（关卡重置重建后由 LevelResetSystem 调用）</summary>
    public void RestoreSettings(BubbleSettings settings)
    {
        oxygen = settings.oxygen;
        speed = settings.speed;
        outOfWaterBurstDistance = settings.outOfWaterBurstDistance;
    }

    #region 碰撞检测

    public void PublishCollisionEvent(GameObject other, Vector2 normal)
    {
        GameEvents.Publish(EventType.COLLISION_EVENT_ON_ENTER,
            new CollisionEventArgs(this.gameObject, other, normal));
    }

    public void PublishTriggerEvent(GameObject other)
    {
        GameEvents.Publish(EventType.COLLISION_EVENT_ON_TRIGGER,
            new CollisionEventArgs(this.gameObject, other, Vector2.zero));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 normal = collision.contactCount > 0 ? collision.contacts[0].normal : Vector2.zero;
        PublishCollisionEvent(collision.gameObject, normal);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PublishTriggerEvent(other.gameObject);
    }

    private void OnCollisionEvent(IGameEvent evt)
    {
        var args = evt as CollisionEventArgs;
        if (args == null) return;
        if (args.Source != this.gameObject && args.Target != this.gameObject) return;
    }

    #endregion

    #region Unity 生命周期

    private void Start()
    {
        GameEvents.Publish(EventType.BUBBLE_EVENT_ON_SPAWN,
            new BubbleBurstEventArgs(this.gameObject));
    }

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

    #endregion
}
