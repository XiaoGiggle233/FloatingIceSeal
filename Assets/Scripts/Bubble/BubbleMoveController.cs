using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleMoveController : MonoBehaviour
{
    //2d刚体
    private Rigidbody2D rb;
    private BubbleBase bubbleBase;
    private float speed;

    void Start()
    {
        bubbleBase = GetComponent<BubbleBase>();
        rb = GetComponent<Rigidbody2D>();
        speed = bubbleBase.speed;
    }

    void FixedUpdate()
    {
        Vector2 velocity = rb.velocity;
        velocity.y = Mathf.Clamp(velocity.y, -speed, speed);
        rb.velocity = velocity;
    }
}
