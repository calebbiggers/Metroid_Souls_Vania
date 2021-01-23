using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
    [Space]
    [Header("Character Config")]
    [SerializeField] public float move_speed;                           // Characters move speed
    [Range(0, .3f)] [SerializeField] public float movement_smoothing;   // Amount to smooth character movement by
    [SerializeField] public float jump_force = 7f;                      // Characters jump force
    [SerializeField] public bool has_air_control;                       // Bool used to tell if character has air controll
    [SerializeField] public bool has_better_jump;                       // Bool used to tell if character  has better jump
    [SerializeField] public float fall_multiplier;                      // Amount to change gravity by when falling. Makes jumping/falling feel better
    [SerializeField] public float gravity_modifier;                     // Amount to change gravity for character

    private Rigidbody2D rb2d;
    private Vector2 velocity = Vector3.zero;    // Temp var for movement smoothing

    // Start is called before the first frame update
    void Awake()
    {
        // Get rigid body 2d
        rb2d = GetComponent<Rigidbody2D>();

        // Set gravity modifier
        rb2d.gravityScale = gravity_modifier;
    }

    public void Move(float direction, bool jump)
    {
        // Get target velocity for movement
        Vector2 target_move_velocity = new Vector2(direction * move_speed, rb2d.velocity.y);

        // Apply velocity to character
        rb2d.velocity = Vector2.SmoothDamp(rb2d.velocity, target_move_velocity, ref velocity, movement_smoothing);
        //rb2d.velocity = target_move_velocity;
        //rb2d.AddForce(new Vector2(direction * move_speed, 0f), ForceMode2D.Force);

        // Check if jump has been inputed
        if(jump)
        {
            // Apply velocity to character
            rb2d.velocity *= new Vector2(1f, 0f);
            rb2d.AddForce(Vector2.up * jump_force, ForceMode2D.Impulse);
        }

        // If character has better jumping, change gravity on fall
        if (has_better_jump)
        {
            if(rb2d.velocity.y < 0)
            {
                // Change gravity based on fall multiplier
                rb2d.gravityScale = fall_multiplier;
            }
            else
            {
                // Reset gravity to default
                rb2d.gravityScale = gravity_modifier;
            }
        }
    }
}

