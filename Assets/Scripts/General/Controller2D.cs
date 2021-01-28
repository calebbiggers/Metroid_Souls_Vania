using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Controller2D : RaycastController2D
{
    public float max_slope_angle = 80f;             // Max angle the object can handle
    public CollisionInfo collisions;                // Struct containing the info about the objects collisions

    public override void Start()
    {
        base.Start();
        collisions.face_direction = 1;
    }

    public void Move( Vector2 velocity, bool standing_on_platform = false)
    {
        // Update the raycast origins for the box collider
        UpdateRaycastOrigins();

        // Reset the objects collision list
        collisions.Reset();

        // Store the old velocity
        collisions.velocity_old = velocity;

        if (velocity.y < 0)
        {
            DescendSlope(ref velocity);
        }

        if (velocity.x != 0)
        {
            collisions.face_direction = (int)Mathf.Sign(velocity.x);
        }

        // Check for horizontal and vertical collisions
        HorizontalCollisions(ref velocity);

        if(velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }

        // Move the object
        transform.Translate(velocity);

        if(standing_on_platform)
        {
            collisions.below = true;
        }
    }

    private void VerticalCollisions(ref  Vector2 velocity)
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
            Debug.DrawRay(ray_origin, Vector2.up * direction_y, Color.red);

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

        if (collisions.climbing_slope)
        {
            // Check for a slope where we are going to be
            float direction_x = Mathf.Sign(velocity.x);
            ray_length = Mathf.Abs(velocity.x) + skin_width;
            Vector2 ray_origin = ((direction_x == -1) ? raycast_origins.bottom_left : raycast_origins.bottom_right) + Vector2.up * velocity.y;

            // Raycast where we will be
            RaycastHit2D hit = Physics2D.Raycast(ray_origin, Vector2.right * direction_x, ray_length, collision_mask);

            // Draw rays for debug
            Debug.DrawRay(ray_origin, Vector2.right * direction_x, Color.red);

            if (hit)
            {
                // Check if what we hit is actually a new slope
                float new_slope_angle = Vector2.Angle(hit.normal, Vector2.up);
                if(new_slope_angle != collisions.slope_angle)
                {
                    // If its a new slope, modify our x velocity to not go though it
                    velocity.x = (hit.distance - skin_width) * direction_x;
                    collisions.slope_angle = new_slope_angle;
                    collisions.slope_normal = hit.normal;
                }
            }
        }
    }

    private void HorizontalCollisions(ref  Vector2 velocity)
    {
        // Get the sign of the y velocity. A normalized direction vector
        float direction_x = collisions.face_direction;

        // Calculate ray length based on velocity. Includes skin width
        float ray_length = Mathf.Abs(velocity.x) + skin_width;

        // Even if we are a little off the wall. We still want to check collisions
        if(Mathf.Abs(velocity.x) < skin_width)
        {
            ray_length = 2 * skin_width;
        }

        for (int i = 0; i < horizontal_ray_count; i++)
        {
            // Get the ray origin. This is from bottom right if moving right, and bottom left if moving left
            Vector2 ray_origin = (direction_x == 1) ? raycast_origins.bottom_right : raycast_origins.bottom_left;

            // Add the ray spacing to the origin based on which ray we are currently on. Also add the y velocity because we are checking where we will end up
            ray_origin += Vector2.up * (horizontal_ray_spacing * i);

            // Perform raycast
            RaycastHit2D hit = Physics2D.Raycast(ray_origin, Vector2.right * direction_x, ray_length, collision_mask);
            
            // Draw rays for debug
            Debug.DrawRay(ray_origin, Vector2.right * direction_x, Color.red);

            // Check if we would hit something
            if (hit)
            {
                // Check if we are inside something
                if(hit.distance == 0)
                {
                    continue;
                }

                // Get the angle of the surface the object collided with
                float slope_angle = Vector2.Angle(hit.normal, Vector2.up);
                
                if(i == 0 && slope_angle <= max_slope_angle)
                {
                    // Check if we are already descending a slope
                    if (collisions.descending_slope)
                    {
                        collisions.descending_slope = false;
                        velocity = collisions.velocity_old;
                    }

                    float distance_to_slope_start = 0;

                    // If we are starting to climb a new slope
                    if (slope_angle != collisions.slope_angle_old)
                    {
                        distance_to_slope_start = hit.distance - skin_width;
                        velocity.x -= distance_to_slope_start * direction_x;
                    }

                    ClimbSlope(ref velocity, slope_angle, hit.normal);
                    velocity.x += distance_to_slope_start * direction_x;
                }

                // Only 
                if(!collisions.climbing_slope || collisions.slope_angle > max_slope_angle)
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

    private void ClimbSlope(ref Vector2 velocity, float slope_angle, Vector2 slope_normal)
    {
        // Calculate the y velocity for climbing using the slope angle
        float move_distance = Mathf.Abs(velocity.x);

        //float climb_velocity_y = velocity.y = move_distance * Mathf.Sin(angle * Mathf.Deg2Rad);
        float climb_velocity_y = move_distance * Mathf.Sin(slope_angle * Mathf.Deg2Rad);

        // Check if the y velocity is already greater than the climb velocity
        if (velocity.y <= climb_velocity_y){
            velocity.y = climb_velocity_y;
            
            // Calculate the x velocity for climbing using the slope angle. Add in the direction since it is removed for the calculations
            velocity.x = move_distance * Mathf.Cos(slope_angle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
            
            // Update the collisions info
            collisions.below = true;
            collisions.climbing_slope = true;
            collisions.slope_angle = slope_angle;
            collisions.slope_normal = slope_normal;
        }
    }

    private void DescendSlope(ref Vector2 velocity)
    {
        // Cast a ray to check for max slope while descending
        RaycastHit2D max_slope_hit_left = Physics2D.Raycast(raycast_origins.bottom_left, Vector2.down, Mathf.Abs(velocity.y) + skin_width, collision_mask);
        RaycastHit2D max_slope_hit_right = Physics2D.Raycast(raycast_origins.bottom_right, Vector2.down, Mathf.Abs(velocity.y) + skin_width, collision_mask);
        if(max_slope_hit_left ^ max_slope_hit_right)
        {
            SlideDownMaxSlope(max_slope_hit_left, ref velocity);
            SlideDownMaxSlope(max_slope_hit_right, ref velocity);
        }

        if (!collisions.sliding_down_max_slope)
        {
            // Get the direction and get the opposite corner from the direction we are descending the slope
            float direction_x = Mathf.Sign(velocity.x);
            Vector2 ray_origin = (direction_x == -1) ? raycast_origins.bottom_right : raycast_origins.bottom_left;

            // Cast a raycast directly below until it hits something
            RaycastHit2D hit = Physics2D.Raycast(ray_origin, -Vector2.up, Mathf.Infinity, collision_mask);

            // Draw rays for debug
            Debug.DrawRay(ray_origin, -Vector2.up * 2, Color.green);

            if (hit)
            {
                // Get the slope angle of the surface we hit
                float slope_angle = Vector2.Angle(hit.normal, Vector2.up);

                // Make sure its not a flat plane
                if (slope_angle != 0 && slope_angle <= max_slope_angle)
                {
                    // Make sure we are moving in the same direction of the slope
                    if (Mathf.Sign(hit.normal.x) == direction_x)
                    {
                        // Make sure we are actually close enough to be affected by the slope
                        if (hit.distance - skin_width <= Mathf.Tan(slope_angle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                        {
                            float move_distance = Mathf.Abs(velocity.x);
                            float descend_velocity_y = move_distance * Mathf.Sin(slope_angle * Mathf.Deg2Rad);
                            velocity.x = move_distance * Mathf.Cos(slope_angle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                            velocity.y -= descend_velocity_y;

                            // Update our collisions
                            collisions.slope_angle = slope_angle;
                            collisions.descending_slope = true;
                            collisions.below = true;
                            collisions.slope_normal = hit.normal;
                        }
                    }
                }
            }
        }
    }

    private void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 velocity)
    {
        if(hit)
        {
            float slope_angle = Vector2.Angle(hit.normal, Vector2.up);
            if(slope_angle > max_slope_angle)
            {
                velocity.x = hit.normal.x * (Mathf.Abs(velocity.y) - hit.distance) / Mathf.Tan(slope_angle * Mathf.Deg2Rad);

                collisions.slope_angle = slope_angle;
                collisions.sliding_down_max_slope = true;
                collisions.slope_normal = hit.normal;
            }
        }
    }
}

