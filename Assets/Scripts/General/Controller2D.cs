using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public struct RaycastOrigins
{
    public Vector2 top_left, top_right;
    public Vector2 bottom_left, bottom_right;
}

public struct CollisionInfo
{
    public bool above, below;
    public bool left, right;

    public bool climbing_slope;
    public float slope_angle, slope_angle_old;
    public void Reset()
    {
        above = below = false;
        left = right = false;
        climbing_slope = false;
        slope_angle_old = slope_angle;
        slope_angle = 0;
    }
}

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    public float move_speed = 10f;                  // Objects horizontal move velocity
    public float acceleration_time_airborne = .1f;  // Horizontal acceleration time if object is airborne
    public float acceleration_time_grounded = .05f; // Horizontal acceleration time if object is grounded
    public float jump_height = 4f;                  // Height in game units the object can jump
    public float fall_gravity_scale = 1.5f;         // Gravity scale when object is falling
    public float normal_gravity_scale = 1f;         // Gravity scale when object is not falling
    public LayerMask collision_mask;                // Layer mask of what to collide with
    const float skin_width = .015f;                 // Distance the raycast origins are offset by inside the box collider 2D
    public int horizontal_ray_count = 4;            // Amount of rays to send out on the x-axis
    public int vertical_ray_count = 4;              // Amount of rays to send out on the y-axis
    public float max_climb_angle = 80f;

    private float direction;                        
    private bool jump;
    private float gravity;
    private Vector3 velocity;
    private float temp;

    private float horizontal_ray_spacing;   // Used for the spacing between each horizontal ray
    private float vertical_ray_spacing;     // Used for the spacing between each vertical ray

    private BoxCollider2D _collider;        // Reference to box collider 2D of object
    private RaycastOrigins raycast_origins; // Struct containing the raycast origins
    public CollisionInfo collisions;        // Struct containing the info about the objects collisions

    private void Start()
    {
        // Get the current gravity
        gravity = GameController.instance.gravity;

        // Get the box collider 2D of the object
        _collider = GetComponent<BoxCollider2D>();

        // Calculate the ray spacing for collider
        CalculateRaySpacing();
    }

    private void Update()
    {
        // Reset vertical velocty if there is a vertical collision
        if (collisions.above || collisions.below)
        {
            velocity.y = 0;
        }

        // Add the jump velocity object was told to jump
        if (jump)
        {
            velocity.y = CalcJumpForce();
        }

        // Add gravity to the object velocity
        if (!collisions.below && velocity.y < 0)
        {
            // Add the modified gravity since object is falling
            velocity.y += gravity * fall_gravity_scale * Time.deltaTime;
        }
        else
        {
            // Add gravity to the objects y velocity
            velocity.y += gravity * normal_gravity_scale * Time.deltaTime;
        }

        // Add the input to the objects x velocity
        float target_velocity_x = direction * move_speed;

        // Smooth the x velocity depending on if airborne or not
        velocity.x = Mathf.SmoothDamp(velocity.x, target_velocity_x, ref temp, (collisions.below) ? acceleration_time_grounded : acceleration_time_airborne);
        
        // Move the objects
        Move(velocity * Time.deltaTime);
    }

    private float CalcJumpForce()
    {
        // The jump force = |gravity| * sqrt( (2 * jump height) / |gravity| )
        return (Mathf.Abs(gravity) * Mathf.Sqrt((2f * jump_height) / Mathf.Abs(gravity)));
    }

    public void UpdateInputs(float direction, bool jump)
    {
        this.direction = direction;
        this.jump = jump;
    }

    private void Move(Vector3 velocity)
    {
        // Reset the objects collision list
        collisions.Reset();

        // Update the raycast origins for the box collider
        UpdateRaycastOrigins();

        // Check for horizontal and vertical collisions
        if(velocity.x != 0)
        {
            HorizontalCollisions(ref velocity);
        }
        if(velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }

        // Move the object
        transform.Translate(velocity);
    }

    private void VerticalCollisions(ref Vector3 velocity)
    {
        // Get the sign of the y velocity. A normalized direction vector
        float direction_y = Mathf.Sign(velocity.y);

        // Calculate ray length based on velocity. Includes skin width
        float ray_length = Mathf.Abs(velocity.y) + skin_width;

        for (int i = 0; i < vertical_ray_count; i++)
        {
            // Get the ray origin. This is from bottom left if moving down, and top left if moving up
            Vector2 ray_origin = (direction_y == -1) ? raycast_origins.bottom_left : raycast_origins.top_left;

            // Add the ray spacing to the origin based on which ray we are currently on. Also add the x velocity because we are checking where we will end up
            ray_origin += Vector2.right * (vertical_ray_spacing * i + velocity.x);

            // Perform raycast
            RaycastHit2D hit = Physics2D.Raycast(ray_origin, Vector2.up * direction_y, ray_length, collision_mask);
            
            // Draw rays for debug
            Debug.DrawRay(ray_origin, Vector2.up * direction_y * ray_length, Color.red);

            // Check if we would hit something
            if (hit)
            {
                // Set the velocity to enough to hit the object we collided with
                velocity.y = (hit.distance - skin_width) * direction_y;

                // Set the ray length to this distance to prevent edge cases where the next ray hits something farther away and sets out velocity higher
                ray_length = hit.distance;

                // If we are climbing the slope and hit something vertically. Recalculate x velocity
                if (collisions.climbing_slope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collisions.slope_angle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                // Set the objects collsions info
                collisions.above = (direction_y == 1);
                collisions.below = (direction_y == -1);
            }
        }
    }

    private void HorizontalCollisions(ref Vector3 velocity)
    {
        // Get the sign of the y velocity. A normalized direction vector
        float direction_x = Mathf.Sign(velocity.x);

        // Calculate ray length based on velocity. Includes skin width
        float ray_length = Mathf.Abs(velocity.x) + skin_width;

        for (int i = 0; i < horizontal_ray_count; i++)
        {
            // Get the ray origin. This is from bottom right if moving right, and bottom left if moving left
            Vector2 ray_origin = (direction_x == 1) ? raycast_origins.bottom_right : raycast_origins.bottom_left;

            // Add the ray spacing to the origin based on which ray we are currently on. Also add the y velocity because we are checking where we will end up
            ray_origin += Vector2.up * (horizontal_ray_spacing * i + velocity.y);

            // Perform raycast
            RaycastHit2D hit = Physics2D.Raycast(ray_origin, Vector2.right * direction_x, ray_length, collision_mask);
            
            // Draw rays for debug
            Debug.DrawRay(ray_origin, Vector2.right * direction_x * ray_length, Color.red);

            // Check if we would hit something
            if (hit)
            {
                // Get the angle of the surface the object collided with
                float slope_angle = Vector2.Angle(hit.normal, Vector2.up);
                
                if(i == 0 && slope_angle <= max_climb_angle)
                {
                    float distance_to_slope_start = 0;
                    // If we are starting to climb a new slope
                    if(slope_angle != collisions.slope_angle_old)
                    {
                        distance_to_slope_start = hit.distance - skin_width;
                        velocity.x -= distance_to_slope_start * direction_x;
                    }
                    ClimbSlope(ref velocity, slope_angle);
                    velocity.x += distance_to_slope_start * direction_x;
                }

                // Only 
                if(!collisions.climbing_slope || collisions.slope_angle > max_climb_angle)
                {
                    // If we collide with something while climbing a slope
                    if (collisions.climbing_slope)
                    {
                        // Set the y velocity to what is needed for the slope
                        velocity.y = Mathf.Tan(collisions.slope_angle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    // Set the velocity to enough to hit the object we collided with
                    velocity.x = (hit.distance - skin_width) * direction_x;

                    // Set the ray length to this distance to prevent edge cases where the next ray hits something farther away and sets out velocity higher
                    ray_length = hit.distance;

                    // Set the objects collsions info
                    collisions.right = (direction_x == 1);
                    collisions.left = (direction_x == -1);
                }
            }
        }
    }

    private void ClimbSlope(ref Vector3 velocity, float angle)
    {
        // Calculate the y velocity for climbing using the slope angle
        float move_distance = Mathf.Abs(velocity.x);
        float climb_velocity_y = velocity.y = move_distance * Mathf.Sin(angle * Mathf.Deg2Rad);

        // Check fi the y velocity is already greater than the climb velocity
        if (velocity.y <= climb_velocity_y){
            velocity.y = climb_velocity_y;
            
            // Calculate the x velocity for climbing using the slope angle. Add in the direction since it is removed for the calculations
            velocity.x = move_distance * Mathf.Cos(angle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
            
            // Update the collisions info
            collisions.below = true;
            collisions.climbing_slope = true;
            collisions.slope_angle = angle;
        }
    }

    private void UpdateRaycastOrigins()
    {
        // Create bounds based on the box collider 2d of the object
        Bounds bounds = _collider.bounds;

        // Shrink the bounds to be skin width inside the collider
        bounds.Expand(skin_width * -2f);

        // Update the raycast origin points
        raycast_origins.top_left = new Vector2(bounds.min.x, bounds.max.y);
        raycast_origins.top_right = new Vector2(bounds.max.x, bounds.max.y);
        raycast_origins.bottom_left = new Vector2(bounds.min.x, bounds.min.y);
        raycast_origins.bottom_right = new Vector2(bounds.max.x, bounds.min.y);
    }

    private void CalculateRaySpacing()
    {
        // Create bounds based on the box collider 2d of the object
        Bounds bounds = _collider.bounds;

        // Shrink the bounds to be skin width inside the collider
        bounds.Expand(skin_width * -2f);

        // Ray counts must always be greater than 2
        horizontal_ray_count = Mathf.Clamp(horizontal_ray_count, 2, int.MaxValue);
        vertical_ray_count = Mathf.Clamp(vertical_ray_count, 2, int.MaxValue);

        // Calculate the spacings
        horizontal_ray_spacing = bounds.size.y / (horizontal_ray_count - 1);
        vertical_ray_spacing = bounds.size.x / (vertical_ray_count - 1);
    }
}

