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
    [SerializeField] public float move_speed = 10f;                             // Characters move speed
    [Range(0, .3f)] [SerializeField] public float movement_smoothing = .05f;    // Amount to smooth character movement by
    [SerializeField] public float jump_force = 7f;                              // Characters jump force
    [SerializeField] public bool has_air_control = true;                        // Bool used to tell if character has air controll
    [SerializeField] public bool has_better_jump = true;                        // Bool used to tell if character  has better jump
    [SerializeField] public float fall_multiplier = 2.5f;                       // Amount to change gravity by when falling. Makes jumping/falling feel better
    [SerializeField] public float gravity_modifier = 1.1f;                      // Amount to change gravity for character
    [SerializeField] private float coyote_time_limit = .075f;                   // Time allowed since last grounding to jump
    [SerializeField] private float max_jump_buffer = .05f;                      // Time since last hit jump for player to jump when grounded
    [SerializeField] public float gravity = -9.81f;
    [SerializeField] private Vector3 velocity;

    [Space]
    [Header("Movement")]
    [SerializeField] private float input_horizontal = 0f;       // Float for storing the horizontal input
    [SerializeField] private float last_input_average = 0f;     // Float used to store the last input
    [SerializeField] private bool moving_right = true;          // Bool used to tell animator which direction player was last moving

    [Space]
    [Header("Jumping")]
    [SerializeField] private Color debug_gizmo_color = Color.red;   // Color used for drawing gizmos
    [SerializeField] private bool is_grounded = true;               // Bool used to tell if player is grounded
    [SerializeField] private Transform ground_check;                // Transform to check for ground at
    [SerializeField] private LayerMask ground_mask;                 // Layer mask for checking ground
    [SerializeField] private float ground_check_distance = .25f;    // Distance from feet to check if grounded
    [SerializeField] private bool jump = false;                     // Bool used to store if player has pressed jump
    [SerializeField] private float last_grounded_time = 10;         // Time last the player was grounded
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

    private Controller2D controller;   // Character controller 2d for moving the character
    private Rigidbody2D rb2d;                   // Rigid body. Used to get stats for the animator
    private Animator animator;                  // Player Animator
    private BoxCollider2D box_collider;         // Players box collider

    private Vector2 ground_check_collider_size; // Size of collider to check if grounded or not
    private Vector2 interact_collider_size;     // Size of box for interactions

    // Start is called before the first frame update
    void Start()
    {
        // Get character controller and set values
        controller = GetComponent<Controller2D>();

        // Get rigid body for animations
        //rb2d = GetComponent<Rigidbody2D>();

        // Get Animator
        animator = GetComponent<Animator>();

        // Set ground check size
        box_collider = GetComponent<BoxCollider2D>();

        // Set initial ground collider size
        ground_check_collider_size.x = box_collider.size.x * box_collider.transform.localScale.x;
        ground_check_collider_size.y = ground_check_distance;

        // Set initial interact collider size
        interact_collider_size.x = interact_distance;
        interact_collider_size.y = box_collider.size.y;
    }

    // Update is called once per frame
    void Update()
    {
        controller.UpdateInputs(input_horizontal, jump);
        jump = false;

        // Set horizontal speed for animator
        animator.SetFloat("horizontal_speed", velocity.x);

        // Set horizontal speed for animator
        animator.SetFloat("vertical_speed", velocity.y);

        // Set direction
        animator.SetBool("moving_right", moving_right);

        // Set is grounded
        animator.SetBool("is_grounded", is_grounded);

        //  Check if player is grounded using collider
        is_grounded = false;

        // Get player size for grounded check
        ground_check_collider_size.x = box_collider.size.x * box_collider.transform.localScale.x * .98f;
        ground_check_collider_size.y = ground_check_distance;

        // Check for ground collision every frame
        Collider2D[] colliders = Physics2D.OverlapBoxAll(ground_check.position, ground_check_collider_size, 0, ground_mask);
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

        // Check if the player hit jump right before touching the ground
        if(is_grounded & last_jump_time != -1)
        {
            if(Time.time - last_jump_time < max_jump_buffer)
            {   
                // Set the jump bool to true so movement knows to jump
                jump = true;

                // Set the jump animation trigger
                animator.SetTrigger("jump");
            }
        }

        // Set the scale to turn player left and right
        if (moving_right)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
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
                input_horizontal = Mathf.Round(context.ReadValue<Vector2>().x);
            }

            // If last input is over 1 set moving right to true
            moving_right = last_input_average > 0 ? true : false;

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
                // If action was just started and player is grounded, jump
                if (is_grounded || (Time.time - last_grounded_time) < coyote_time_limit)
                {
                    // Set the last jump time to invalid
                    last_jump_time = -1;

                    // Set the jump bool to true so movement knows to jump
                    jump = true;

                    // Set the jump animation trigger
                    animator.SetTrigger("jump");
                }
                else
                {
                    // Save the last jump time if player was not grounded
                    last_jump_time = Time.time;
                }
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

    // Debug gizmos for player
    private void OnDrawGizmos()
    {
        // Set gizmo color
        Gizmos.color = debug_gizmo_color;

        // Gizmo for if the player is grounded
        Gizmos.DrawWireCube(ground_check.position, ground_check_collider_size);

        // Gizmo for interaction radius
        Gizmos.DrawWireCube(interact_transform.position, interact_collider_size);
    }
}