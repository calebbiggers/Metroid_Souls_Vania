using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveMode { PHYSICS, TRANSLATE }


public class DebugMovement : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private Vector2 start;
    public MoveMode mode = MoveMode.PHYSICS;
    public float speed = 5f;
    public float reset_distance = 20f;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        start = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(mode == MoveMode.TRANSLATE)
        {
            transform.position = new Vector2(transform.position.x + speed * Time.deltaTime, transform.position.y);
        }

        if (transform.position.x > start.x + reset_distance)
        {
            transform.position = start;
        }
    }
    private void FixedUpdate()
    {
        if(mode == MoveMode.PHYSICS)
        {
            rb2d.velocity = new Vector2(speed, 0);
        }
    }
}
