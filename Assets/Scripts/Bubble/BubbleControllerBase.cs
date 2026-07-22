using UnityEngine;

public abstract class BubbleControllerBase : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected BubbleBase bubbleBase;
    protected float speed;

    protected GameObject seal;
    protected GameObject spike;

    protected virtual void Start()
    {
        bubbleBase = GetComponent<BubbleBase>();
        rb = GetComponent<Rigidbody2D>();
        speed = bubbleBase.speed;
        rb.velocity = Vector2.up * speed;

        seal = ResolveGameObject("Seal");
        spike = ResolveGameObject("Spike");
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

    protected virtual void Update()
    {
        if (!WatterUtils.HasWaterBelow(transform.position, bubbleBase.outOfWaterBurstDistance))
        {
            bubbleBase.Burst();
        }
    }

    private void OnCollisionEvent(IGameEvent evt)
    {
        var args = evt as CollisionEventArgs;
        if (args == null) return;
        if (args.Source != this.gameObject && args.Target != this.gameObject) return;

        GameObject other = args.Source == this.gameObject ? args.Target : args.Source;

        if (other == seal || other == spike)
        {
            bubbleBase.Burst();
        }
    }

    protected static GameObject ResolveGameObject(string key)
    {
        if (InformationPool.TryGet(key, out object obj))
        {
            if (obj is GameObject go) return go;
            if (obj is Component comp) return comp.gameObject;
        }
        return null;
    }
}
