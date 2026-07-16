using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleController : MonoBehaviour
{
    private Rigidbody2D rb;
    private BubbleBase bubbleBase;
    private float speed;

    private GameObject water;
    private GameObject wall;
    private GameObject seal;

    private float lastWaterContactTime;

    // Start is called before the first frame update
    void Start()
    {
        bubbleBase = GetComponent<BubbleBase>();
        rb = GetComponent<Rigidbody2D>();
        speed = bubbleBase.speed;
        rb.velocity = Vector2.up * speed;

        water = ResolveGameObject("Watter");
        wall = ResolveGameObject("Wall");
        seal = ResolveGameObject("Seal");

        lastWaterContactTime = Time.time;
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

        if (other == water)
        {
            lastWaterContactTime = Time.time;
        }
        else if (other == wall || other == seal)
        {
            bubbleBase.Burst();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject == water)
        {
            lastWaterContactTime = Time.time;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject == water)
        {
            lastWaterContactTime = Time.time;
        }
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
