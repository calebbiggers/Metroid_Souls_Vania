using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent (typeof (Controller2D))]
public class PlayerController2D : MonoBehaviour
{
    #region Singleton
    public static PlayerController2D instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of Player Controller found");
            return;
        }
        instance = this;
    }
    #endregion 

    [Space]
    [Header("Player Config")]
    [SerializeField] private float move_speed = 10f;                     // players horizontal move velocity
    [SerializeField] private float wall_slide_speed = 2f;                // Players wall slide speed
    [SerializeField] private float wall_stick_time = .25f;
    [SerializeField] private Vector2 wall_jump_climb;
    [SerializeField] private Vector2 wall_jump_off;
    [SerializeField] private Vector2 wall_leap;
    [SerializeField] private float acceleration_time_airborne = .1f;     // Horizontal acceleration time if player is airborne
    [SerializeField] private float acceleration_time_grounded = .05f;    // Horizontal acceleration time if player is grounded
    [SerializeField] private float jump_height = 4f;                     // Height in game units the player can jump
    [SerializeField] private float fall_gravity_scale = 1.5f;            // Gravity scale when player is falling
    [SerializeField] private float normal_gravity_scale = 1f;            // Gravity scale when player is not falling
    [SerializeField] private float coyote_time_limit = .09f;            // Time allowed since last grounding to jump
    [SerializeField] private float max_jump_buffer = .09f;               // Time since last hit jump for player to jump when grounded

    [Space]
    [Header("Movement")]
    [SerializeField] private float input_horizontal = 0f;       // Float for storing the horizontal input
    [SerializeField] private float input_vertical = 0f;         // Float for storing the vertical input
    [SerializeField] private float last_input_average = 0f;     // Float used to store the last input average
    [SerializeField] private bool moving_right = true;          // Bool used to tell animator which direction player was last moving

    [Space]
    [Header("Jumping")]
    [SerializeField] private Color debug_gizmo_color = Color.red;   // Color used for drawing gizmos
    [SerializeField] private float last_grounded_time = 0;          // Time last the player was grounded
    [SerializeField] private float last_jump_time = -1f;            // Time since the player last hit jump

    [Space]
    [Header("Interactions")]
    [SerializeField] private Interactable focus;                    // Interactable player is currently focused on
    [SerializeField] private Transform interact_transform;          // Center from which interactions will be checked
    [SerializeField] private float interact_distance = 1f;          // Distance from player that player can interact with objects
    [SerializeField] private LayerMask interactables_mask;          // Layers mask of all interactable objects

    [Space]
    [Header("Movement Enable")]
    [SerializeField] public bool control_enabled = true;
       
    private bool jump;                              // If player should be jumping this frame
    private bool wall_sliding;                      // If the player is wall sliding
    private int wall_dir;                           // Direction to wall player is wall sliding on
    private float time_to_wall_unstick;             // Time player sticks to wall after tryign to move off of it
    private float gravity;                          // Gravity on player
    [SerializeField] private Vector3 velocity;      // Vector3 storing the players velocity in the x, y & z axis
    private float temp;                             // Temporary float for the smoothdamp function

    private Controller2D controller;                // Character controller 2d for moving the character
    private Animator animator;                      // Player Animator
    private BoxCollider2D box_collider;             // Players box collider 2D

    private Vector2 interact_collider_size;         // Size of box for interactions

    private const int LEFT = -1;
    private const int RIGHT = 1;

    private bool player_moving_down => velocity.y < 0;
    private bool player_is_grounded => controller.collisions.below;
    private bool player_is_falling => (player_moving_down && !player_is_grounded);

    // Start is called before the first frame update
    void Start()
    {
        // Get the current gravity
        gravity = GameController.instance.gravity;

        // Get character controller and set values
        controller = GetComponent<Controller2D>();

        // Get Animator
        animator = GetComponent<Animator>();;

        // Get box collider
        box_collider = GetComponent<BoxCollider2D>();

        // Set initial interact collider size
        interact_collider_size.x = interact_distance;
        interact_collider_size.y = box_collider.size.y;
    }

    // Update is called once per frame
    void Update()
    {
        // Reset vertical velocty if there is a vertical collision. !THIS MUST BE DONE FIRST IN UPDATE!
        if (controller.collisions.above || controller.collisions.below)
        {
            if (controller.collisions.sliding_down_max_slope)
            {
                velocity.y += controller.collisions.slope_normal.y * gravity * Time.deltaTime;
            }
            else{
                velocity.y = 0;
            }
        }

        // Calculate the velocity of player
        CalculateVelocity();

        // Handle if player is wall sliding
        HandleWallSliding();

        // Handle player jumping
        HandleJumping();

        // Move the player
        controller.Move(velocity * Time.deltaTime);

        #region Animations

        // Set horizontal speed for animator
        animator.SetFloat("horizontal_speed", velocity.x);

        // Set horizontal speed for animator
        animator.SetFloat("vertical_speed", velocity.y);

        // Set direction
        animator.SetBool("moving_right", moving_right);

        // Set is grounded
        animator.SetBool("is_grounded", controller.collisions.below);

        // Set the scale to turn player left and right
        if (moving_right)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        #endregion
    }

    private void CalculateVelocity()
    {
        // Add the input to the objects x velocity
        float target_velocity_x = input_horizontal * move_speed;

        // Smooth the x velocity depending on if airborne or not
        velocity.x = Mathf.SmoothDamp(velocity.x, target_velocity_x, ref temp, ((controller.collisions.below) ? acceleration_time_grounded : acceleration_time_airborne));

        // Add gravity to velocity depending on if object is falling or not
        velocity.y += gravity * (player_is_falling ? fall_gravity_scale : normal_gravity_scale) * Time.deltaTime;
    }

    private void HandleWallSliding()
    {
        if (controller.collisions.sliding_down_max_slope)
        {
            return;
        }

        // Get the direction of the wall we are  wall sliding on (if we are wall sliding)
        wall_dir = (controller.collisions.left) ? LEFT : RIGHT;

        // Check for wall sliding
        wall_sliding = false;
        if ((controller.collisions.left || controller.collisions.right) && player_is_falling)
        {
            wall_sliding = true;

            // Make sure our wall sliding velocity it constant
            if (velocity.y < -wall_slide_speed)
            {
                velocity.y = -wall_slide_speed;
            }

            // Check how long we should be stuck to the wall
            if (time_to_wall_unstick > 0)
            {
                velocity.x = 0;
                temp = 0;

                // Check if we are trying to get off the wall
                if (input_horizontal != wall_dir && input_horizontal != 0)
                {
                    time_to_wall_unstick -= Time.deltaTime;
                }
                else
                {
                    time_to_wall_unstick = wall_stick_time;
                }

            }
            else
            {
                time_to_wall_unstick = wall_stick_time;
            }
        }
    }

    private void HandleJumping()
    {
        // Check if player is currently grounded for coyote time
        if (player_is_grounded)
        {
            last_grounded_time = Time.time;
        }

        // Check if the player hit jump right before touching the ground
        if (player_is_grounded & last_jump_time != -1)
        {
            if (Time.time - last_jump_time < max_jump_buffer)
            {
                // Set the last jump time to invalid
                last_jump_time = -1;

                // Add the jump velocity
                velocity.y = CalcJumpForce(jump_height);

                // Set the jump animation trigger
                animator.SetTrigger("jump");
            }
        }

        // Add the jump velocity object was told to jump
        if (jump)
        {
            // If player is grounded or has coyote time, jump
            if (wall_sliding)
            {
                if (wall_dir == Mathf.Sign(input_horizontal))
                {
                    velocity.x = -wall_dir * wall_jump_climb.x;
                    velocity.y = wall_jump_climb.y;
                }
                else if (Mathf.Abs(input_horizontal) <= .1)
                {
                    velocity.x = -wall_dir * wall_jump_off.x;
                    velocity.y = wall_jump_off.y;
                }
                else
                {
                    velocity.x = -wall_dir * wall_leap.x;
                    velocity.y = wall_leap.y;
                }
            }
            else if (player_is_grounded || (Time.time - last_grounded_time) <= coyote_time_limit)
            {
                if (controller.collisions.sliding_down_max_slope)
                {
                    // Sliding down max slope and tried to jump
                }
                else
                {
                    // Set the last jump time to invalid
                    last_jump_time = -1;

                    // Add the jump velocity
                    velocity.y = CalcJumpForce(jump_height);

                    // Set the jump animation trigger
                    animator.SetTrigger("jump");
                }
            }
            else
            {
                // Save the last jump time if player was not grounded
                last_jump_time = Time.time;
            }

            jump = false;
        }
    }

    // Player move action
    public void Move(InputAction.CallbackContext context)
    {
        if (control_enabled)
        {
            // Store calculate running average (I think thats what its called?) with last input
            last_input_average = (last_input_average + input_horizontal) / 2;

            // Read in and store horizontal input
            if (!context.canceled)
            {
                input_horizontal = context.ReadValue<Vector2>().x;
                input_vertical = context.ReadValue<Vector2>().y;
            }

            // If last input is over 1 set moving right to true
            moving_right = last_input_average > 0;

            // Set was moving right based on last input
            if (context.canceled)
            {
                last_input_average = 0f;
                input_horizontal = 0f;
            }
        }
    }

    // Player jumps action
    public void Jump(InputAction.CallbackContext context)
    {
        if (control_enabled)
        {
            // Only trigger once per button press   
            if (context.started)
            {
                jump = true;
            }
        }
    }

    public void Evade(InputAction.CallbackContext context)
    {
        if (control_enabled)
        {
            if (context.started)
            {
                Functions.DebugLog("Player hit evade");
            }
        }
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (control_enabled)
        {
            if (context.started)
            {
                Functions.DebugLog("Player hit attack");
            }
        }
    }

    public void Special(InputAction.CallbackContext context)
    {
        if (control_enabled)
        {
            if (context.started)
            {
                Functions.DebugLog("Player hit special");
            }
        }
    }

    // Player interacts action
    public void Interact(InputAction.CallbackContext context)
    {
        if (control_enabled)
        {
            // Only do stuff on initial button press
            if (context.started)
            {
                interact_collider_size.x = interact_distance;
                interact_collider_size.y = box_collider.size.y;

                // Check for ground collision every frame
                Collider2D[] colliders = Physics2D.OverlapBoxAll(interact_transform.position, interact_collider_size, interactables_mask);
                for (int i = 0; i < colliders.Length; i++)
                {
                    // Make sure collision isnt player itself
                    if (colliders[i].gameObject != this.gameObject)
                    {
                        // Found an item to interact with
                        Functions.DebugLog("Found an interactable item");
                        Interactable interactable = colliders[i].gameObject.GetComponent<Interactable>();
                        if (interactable != null)
                        {
                            // Set the interactable object as the focus
                            SetFocus(interactable);

                            // Set the animator interaction trigger
                            animator.SetTrigger("interact");
                        }
                    }
                }
            }
        }
    }

    public void SetFocus(Interactable new_focus)
    {
        if (new_focus != focus)
        {
            if(focus != null)
            {
                focus.OnDefocused();
            }
            focus = new_focus;
        }
        new_focus.OnFocused(transform);
    }

    public void RemoveFocus()
    {
        if(focus != null)
        {
            focus.OnDefocused();
        }
        focus = null;
    }

    public float CalcJumpForce(float the_jump_height)
    {
        // The jump force = |gravity| * sqrt( (2 * jump height) / |gravity| )
        return (Mathf.Abs(gravity) * Mathf.Sqrt(2f * the_jump_height / Mathf.Abs(gravity)));
    }

    // Debug gizmos for player
    private void OnDrawGizmos()
    {
        // Set gizmo color
        Gizmos.color = Color.blue;

        // Gizmo for interaction radius
        Gizmos.DrawWireCube(interact_transform.position, interact_collider_size);
    }
}