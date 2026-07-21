using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleController : MonoBehaviour
{
    private Rigidbody2D rb;
    private BubbleBase bubbleBase;
    private float speed;

    private GameObject wall;
    private GameObject seal;
    private GameObject seagrass;

    private float lastWaterContactTime;

    // Start is called before the first frame update
    void Start()
    {
        bubbleBase = GetComponent<BubbleBase>();
        rb = GetComponent<Rigidbody2D>();
        speed = bubbleBase.speed;
        rb.velocity = Vector2.up * speed;

        wall = ResolveGameObject("Wall");
        seal = ResolveGameObject("Seal");
        seagrass = ResolveGameObject("Seagrass");

        lastWaterContactTime = Time.time;
    }

    private void OnEnable()
    {
        GameEvents.Listen(EventType.COLLISION_EVENT_ON_ENTER, OnCollisionEvent);
        GameEvents.Listen(EventType.COLLISION_EVENT_ON_TRIGGER, OnCollisionEvent);
        GameEvents.Listen(EventType.BUBBLE_EVENT_ON_RELEASE, OnBubbleRelease);
    }

    private void OnDisable()
    {
        GameEvents.Unlisten(EventType.COLLISION_EVENT_ON_ENTER, OnCollisionEvent);
        GameEvents.Unlisten(EventType.COLLISION_EVENT_ON_TRIGGER, OnCollisionEvent);
        GameEvents.Unlisten(EventType.BUBBLE_EVENT_ON_RELEASE, OnBubbleRelease);
    }

    private void OnBubbleRelease(IGameEvent evt)
    {
        var args = evt as BubbleBlowEventArgs;
        if (args == null) return;
        if (args.Bubble != this.gameObject) return;

        lastWaterContactTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - lastWaterContactTime > bubbleBase.outOfWaterBurstDelay)
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

        if (IsWaterObject(other))
        {
            lastWaterContactTime = Time.time;
        }
        else if (other == wall || other == seal || other == seagrass)
        {
            bubbleBase.Burst();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsWaterObject(other.gameObject))
        {
            lastWaterContactTime = Time.time;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (IsWaterObject(collision.gameObject))
        {
            lastWaterContactTime = Time.time;
        }
    }

    /// <summary>检查 GameObject 是否为任一水体对象</summary>
    private static bool IsWaterObject(GameObject go)
    {
        if (go == null) return false;
        if (!InformationPool.TryGet("WatterList", out System.Collections.Generic.List<WatterController> list) || list == null)
            return false;
        foreach (var wc in list)
        {
            if (wc != null && wc.gameObject == go)
                return true;
        }
        return false;
    }

    private static GameObject ResolveGameObject(string key)
    {
        if (InformationPool.TryGet(key, out object obj))
        {
            if (obj is GameObject go) return go;
            if (obj is Component comp) return comp.gameObject;
        }
        return null;
    }

}
