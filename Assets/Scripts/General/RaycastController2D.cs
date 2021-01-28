using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RaycastOrigins
{
    public Vector2 top_left, top_right;
    public Vector2 bottom_left, bottom_right;
}

[System.Serializable]
public struct CollisionInfo
{
    public bool above, below;   // If object is colliding above and below itself
    public bool left, right;    // If object is colliding to the right and left of itself

    public bool climbing_slope, descending_slope;   // If the object is climbing or descending a slope
    public bool sliding_down_max_slope;             // If the object is falling down a max slope

    public float slope_angle, slope_angle_old;      // Slope angle and previous slope angle
    public Vector2 slope_normal;                    // The normal of the slope we are on
    public Vector2 velocity_old;                    // Old velocity of object
    public int face_direction;                      // Direction object is facing

    public void Reset()
    {
        // Reset all values and store old ones
        above = below = false;
        left = right = false;
        climbing_slope = false;
        descending_slope = false;
        sliding_down_max_slope = false;
        slope_angle_old = slope_angle;
        slope_normal = Vector2.zero;
        slope_angle = 0;
    }
}

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController2D : MonoBehaviour
{
    public LayerMask collision_mask;                            // Layer mask of what to collide with

    public const float skin_width = .015f;                      // Distance the raycast origins are offset by inside the box collider 2D
    public const float distance_between_rays = .25f;            // Distance between collisions check rays
    [HideInInspector] public int horizontal_ray_count;          // Amount of rays to send out on the x-axis
    [HideInInspector] public int vertical_ray_count;            // Amount of rays to send out on the y-axis

    [HideInInspector] public float horizontal_ray_spacing;      // Used for the spacing between each horizontal ray
    [HideInInspector] public float vertical_ray_spacing;        // Used for the spacing between each vertical ray

    [HideInInspector] private BoxCollider2D _collider;           // Reference to box collider 2D of object
    public RaycastOrigins raycast_origins;                      // Struct containing the raycast origins

    public virtual void Start()
    {
        // Get the box collider 2D of the object
        _collider = GetComponent<BoxCollider2D>();

        // Calculate the ray spacing for collider
        CalculateRaySpacing();
    }

    public void UpdateRaycastOrigins()
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

    public void CalculateRaySpacing()
    {
        // Create bounds based on the box collider 2d of the object
        Bounds bounds = _collider.bounds;

        // Shrink the bounds to be skin width inside the collider
        bounds.Expand(skin_width * -2f);

        float bounds_width = bounds.size.x;
        float bounds_height = bounds.size.y;

        // Get the ray count based on the bounds
        horizontal_ray_count = Mathf.RoundToInt(bounds_height / distance_between_rays);
        vertical_ray_count = Mathf.RoundToInt(bounds_width / distance_between_rays);

        // Ray counts must always be greater than 2
        horizontal_ray_count = Mathf.Clamp(horizontal_ray_count, 2, int.MaxValue);
        vertical_ray_count = Mathf.Clamp(vertical_ray_count, 2, int.MaxValue);

        // Calculate the spacings
        horizontal_ray_spacing = bounds.size.y / (horizontal_ray_count - 1);
        vertical_ray_spacing = bounds.size.x / (vertical_ray_count - 1);
    }
}
