using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleController : MonoBehaviour
{
    private Rigidbody2D rb;
    private BubbleBase bubbleBase;
    private float speed;

    // Start is called before the first frame update
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
