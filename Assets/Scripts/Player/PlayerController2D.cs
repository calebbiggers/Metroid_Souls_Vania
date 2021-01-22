using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{
    [Space]
    [Header("Player Stats")]
    [SerializeField] public float move_speed = 10f;                             // Characters move speed
    [Range(0, .3f)] [SerializeField] public float movement_smoothing = .05f;    // Amount to smooth character movement by
    [SerializeField] public float jump_force = 7f;                              // Characters jump force
    [SerializeField] public bool has_air_control = true;                        // Bool used to tell if character has air controll
    [SerializeField] public bool has_better_jump = true;                        // Bool used to tell if character  has better jump
    [SerializeField] public float fall_multiplier = 2.5f;                       // Amount to change gravity by when falling. Makes jumping/falling feel better
    [SerializeField] public float gravity_modifier = 1.1f;                      // Amount to change gravity for character
    [SerializeField] private float coyote_time_limit = .075f;                    // Time allowed since last grounding to jump

    [Space]
    [Header("Movement Debug")]
    [SerializeField] private float input_horizontal = 0f;       // Float for storing the horizontal input
    [SerializeField] private float last_input_average = 0f;     // Float used to store the last input
    [SerializeField] private bool was_moving_right = true;      // Bool used to tell animator which direction player was last moving

    [Space]
    [Header("Jumping Debug")]
    [SerializeField] private Color debug_gizmo_color = Color.red;   // Color used for drawing gizmos
    [SerializeField] private bool is_grounded = true;               // Bool used to tell if player is grounded
    [SerializeField] private Transform ground_check;                // Transform to check for ground at
    [SerializeField] private LayerMask what_is_ground;              // Layer mask for checking ground
    [SerializeField] private float ground_check_distance = .25f;    // Distance from feet to check if grounded
    [SerializeField] private bool jump = false;                     // Bool used to store if player has pressed jump
    [SerializeField] private float last_grounded_time = 0;          // Time last the player was grounded

    private CharacterController2D controller;   // Character controller 2d for moving the character
    private Rigidbody2D rb2d;                   // Rigid body. Used for the animator
    private Animator animator;                  // Animator

    private Vector2 ground_check_collider_size; // Size of collider to check if grounded or not

    // Start is called before the first frame update
    void Awake()
    {
        // Get character controller and set values
        controller = GetComponent<CharacterController2D>();
        controller.move_speed = move_speed;
        controller.movement_smoothing = movement_smoothing;
        controller.jump_force = jump_force;
        controller.has_air_control = has_air_control;
        controller.has_better_jump = has_better_jump;
        controller.fall_multiplier = fall_multiplier;
        controller.gravity_modifier = gravity_modifier;

        // Get rigid body for animations
        rb2d = GetComponent<Rigidbody2D>();

        // Get Animator
        animator = GetComponent<Animator>();

        // Set ground check size
        BoxCollider2D box_collider = GetComponent<BoxCollider2D>();
        ground_check_collider_size.x = box_collider.size.x * box_collider.transform.localScale.x;
        ground_check_collider_size.y = ground_check_distance;

        // Set player to not collide with pickups
        Physics2D.IgnoreLayerCollision(7, 9);
    }

    // Update is called once per frame
    void Update()
    {
        // Set horizontal speed for animator
        animator.SetFloat("horizontal_speed", rb2d.velocity.x);

        // Set horizontal speed for animator
        animator.SetFloat("vertical_speed", rb2d.velocity.y);

        // Set direction
        animator.SetBool("moving_right", was_moving_right);

        // Set is grounded
        animator.SetBool("is_grounded", is_grounded);

        //  Check if player is grounded using collider
        is_grounded = false;

        // Check for ground collision every frame
        Collider2D[] colliders = Physics2D.OverlapBoxAll(ground_check.position, ground_check_collider_size, 0, what_is_ground);
        for (int i = 0; i < colliders.Length; i++)
        {
            // Make sure collision isnt player itself
            if (colliders[i].gameObject != this.gameObject)
            {
                // Set is grouded to true
                is_grounded = true;
                last_grounded_time = Time.time;
            }
        }
    }

    // Fixed update is frame rate dependant
    public void FixedUpdate()
    {
        // Move the character using Character Controller 2D
        controller.Move(input_horizontal, jump);

        // Set jump to false so that player only jumps once
        jump = false;
    }

    // Player move action
    public void Move(InputAction.CallbackContext context)
    {
        // Store calculate running average (I think thats what its called?) with last input
        last_input_average = (last_input_average + input_horizontal) / 2;

        // Read in and store horizontal input
        if (!context.canceled)
        {
            input_horizontal = context.ReadValue<Vector2>().x;
        }

        // If last input is over 1 set moving right to true
        was_moving_right = last_input_average > 0 ? true : false;

        // Set was moving right based on last input
        if(context.canceled)
        {
            last_input_average = 0f;
            input_horizontal = 0f;
        }
    }

    // Player jumps action
    public void Jump(InputAction.CallbackContext context)
    {
        // If action was just started and player is grounded, jump
        if(context.started && (is_grounded || (Time.time - last_grounded_time) < coyote_time_limit))
        {
            jump = true;
        }
    }

    // Debug gizmos for player
    private void OnDrawGizmos()
    {
        // Set gizmo color
        Gizmos.color = debug_gizmo_color;

        // Gizmo for if the player is grounded
        Gizmos.DrawCube(ground_check.position, ground_check_collider_size);
    }
}