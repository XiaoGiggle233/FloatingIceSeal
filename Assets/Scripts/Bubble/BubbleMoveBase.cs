using UnityEngine;

public abstract class BubbleMoveBase : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected BubbleBase bubbleBase;
    protected float speed;

    protected virtual void Start()
    {
        bubbleBase = GetComponent<BubbleBase>();
        rb = GetComponent<Rigidbody2D>();
        speed = bubbleBase.speed;
    }

    protected virtual void FixedUpdate()
    {
        if (rb == null) return;
        Move();
    }

    protected abstract void Move();
}
