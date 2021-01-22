using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedLimit : MonoBehaviour
{
    [Space]
    [Header("Speed Limit Settings")]
    [SerializeField] public float speed_limit_X;    // Speed limit for object in X direction
    [SerializeField] public float speed_limit_Y;    // Speed limit for object in Y direction
    [SerializeField] public bool limitX = true;     // Boolean for if X velocity should be limited
    [SerializeField] public bool limitY = true;     // Boolean for if Y velocity should be limited

    [Space]
    [Header("Velocity")]
    [SerializeField] public Vector2 velocity = Vector2.zero;   

    private Rigidbody2D rb2d;   // Rigid body of object

    // Start is called before the first frame update
    void Awake()
    {
        // Get rigid body
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Fixed update is frame rate dependant
    void FixedUpdate()
    {
        // Limit X velocity if its enabled
        if (limitX)
        {
            if (rb2d.velocity.x > speed_limit_X)
            {
                rb2d.velocity = new Vector2(speed_limit_X, rb2d.velocity.y);
            }

            if (rb2d.velocity.x < -speed_limit_X)
            {
                rb2d.velocity = new Vector2(-speed_limit_X, rb2d.velocity.y);
            }
        }

        // Limit Y velocity if its enabled
        if (limitY)
        {
            if (rb2d.velocity.y > speed_limit_Y)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, speed_limit_Y);
            }

            if (rb2d.velocity.y < -speed_limit_Y)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, -speed_limit_Y);
            }
        }

        // Copy velocity to variable
        velocity = rb2d.velocity;
    }
}
