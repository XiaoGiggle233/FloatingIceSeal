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

    // Start is called before the first frame update
    void Start()
    {
        bubbleBase = GetComponent<BubbleBase>();
        rb = GetComponent<Rigidbody2D>();
        speed = bubbleBase.speed;

        water = ResolveGameObject("Watter");
        wall = ResolveGameObject("Wall");
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

    private void OnCollisionEvent(IGameEvent evt)
    {
        var args = evt as CollisionEventArgs;
        if (args == null) return;
        if (args.Source != this.gameObject && args.Target != this.gameObject) return;

        GameObject other = args.Source == this.gameObject ? args.Target : args.Source;

        if (other == water || other == wall)
        {
            OnHitWaterOrWall(other);
        }
    }

    private void OnHitWaterOrWall(GameObject other)
    {
        Debug.Log($"Bubble hit {other.name}");
        bubbleBase.Burst();
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
