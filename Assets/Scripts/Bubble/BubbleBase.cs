using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class BubbleBase : MonoBehaviour, ICollisionEventPublisher
{
    //储存的氧气
    [SerializeField]public int oxygen;
    //上浮的速度
    [SerializeField]public float speed;

    //离开水面超过此距离（单位）即破裂
    [SerializeField]public float outOfWaterBurstDistance = 0.5f;

    //泡泡破裂
    [Button("Burst", ButtonSizes.Large)]
    public abstract void Burst();

    #region 碰撞检测

    public void PublishCollisionEvent(GameObject other)
    {
        GameEvents.Publish(EventType.COLLISION_EVENT_ON_ENTER,
            new CollisionEventArgs(this.gameObject, other));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PublishCollisionEvent(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameEvents.Publish(EventType.COLLISION_EVENT_ON_TRIGGER,
            new CollisionEventArgs(this.gameObject, other.gameObject));
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
