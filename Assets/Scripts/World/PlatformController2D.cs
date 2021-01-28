using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PassengerMovement
{
    public Transform transform;
    public Vector3 velocity;
    public bool standing_on_platform;
    public bool move_before_platform;

    public PassengerMovement(Transform transform, Vector3 velocity, bool standing_on_platform, bool move_before_platform)
    {
        this.transform = transform;
        this.velocity = velocity;
        this.standing_on_platform = standing_on_platform;
        this.move_before_platform = move_before_platform;
    }
}

public class PlatformController2D : RaycastController2D
{
    public LayerMask passenger_mask;

    public Vector3[] local_waypoints;
    private Vector3[] global_waypoints;

    public float speed;
    public bool cyclic = false;
    public float wait_time;
    [Range(0, 2)] public float ease_amount;

    private int from_waypoint_index;
    private float percent_between_waypoints;
    private float next_move_time;

    private List<PassengerMovement> passenger_movement;
    private Dictionary<Transform, Controller2D> passenger_dictionary = new Dictionary<Transform, Controller2D>();

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        global_waypoints = new Vector3[local_waypoints.Length];
        for(int i = 0; i < local_waypoints.Length; i++)
        {
            global_waypoints[i] = local_waypoints[i] + transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRaycastOrigins();

        Vector3 velocity = CalculatePlatformMovement();

        CalculatePassengerMove(velocity);

        MovePassengers(true);
        transform.Translate(velocity);
        MovePassengers(false);
    }
    
    public float Ease(float x)
    {
        float a = ease_amount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }
    private Vector3 CalculatePlatformMovement()
    {
        if(local_waypoints.Length <= 1)
        {
            return Vector3.zero;
        }

        // Make sure its time for platform to move
        if(Time.time < next_move_time)
        {
            return Vector3.zero;
        }
        
        // Limit waypoint index to stay below array length
        from_waypoint_index %= global_waypoints.Length;

        // Get the next index. Also limited to scope of array
        int to_waypoint_index = (from_waypoint_index + 1) % global_waypoints.Length;

        // Get the distance between the waypoints
        float distance_between_waypoints = Vector3.Distance(global_waypoints[from_waypoint_index], global_waypoints[to_waypoint_index]);

        // Get the percent between the waypoints. Clamped to be between 0 and 1.00
        percent_between_waypoints += Time.deltaTime * speed / distance_between_waypoints;
        percent_between_waypoints = Mathf.Clamp(percent_between_waypoints, 0, 1);

        // Ease the percent so the movement is smooth
        float eased_percent_between_waypoints = Ease(percent_between_waypoints);
        
        // Get the new position
        Vector3 new_position = Vector3.Lerp(global_waypoints[from_waypoint_index], global_waypoints[to_waypoint_index], eased_percent_between_waypoints);

        // Check if we've reached the next waypoint
        if(percent_between_waypoints >= 1)
        {
            // Reset the percentages and incriment to next waypoint
            percent_between_waypoints = 0;
            from_waypoint_index++;

            // If this platform isnt cyclic, reverse the array to move back
            if (!cyclic)
            {
                if (from_waypoint_index >= global_waypoints.Length - 1)
                {
                    from_waypoint_index = 0;
                    System.Array.Reverse(global_waypoints);
                }
            }

            // Set the next time to move to wait at this waypoint
            next_move_time = Time.time + wait_time;
        }

        // Return the new position
        return new_position - transform.position;
    }

    void MovePassengers(bool before_move_platform)
    {
        if (passenger_movement != null)
        {
            foreach (PassengerMovement passenger in passenger_movement)
            {
                if (!passenger_dictionary.ContainsKey(passenger.transform))
                {
                    passenger_dictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
                }

                if (passenger.move_before_platform == before_move_platform)
                {
                    passenger_dictionary[passenger.transform].Move(passenger.velocity, passenger.standing_on_platform);
                }
            }
        }
    }

    void CalculatePassengerMove(Vector3 velocity)
    {
        HashSet <Transform> moved_passengers = new HashSet<Transform>();

        passenger_movement = new List<PassengerMovement>();

        float direction_x = Mathf.Sign(velocity.x);
        float direction_y = Mathf.Sign(velocity.y);

        // Vertically moving platform
        if (velocity.y != 0)
        {
            // Calculate ray length based on velocity. Includes skin width
            float ray_length = Mathf.Abs(velocity.y) + skin_width;

            for (int i = 0; i < vertical_ray_count; i++)
            {
                // Get the ray origin. This is from bottom left if moving down, and top left if moving up
                Vector2 ray_origin = (direction_y == -1) ? raycast_origins.bottom_left : raycast_origins.top_left;

                // Add the ray spacing to the origin based on which ray we are currently on. Also add the x velocity because we are checking where we will end up
                ray_origin += Vector2.right * (vertical_ray_spacing * i);

                // Perform raycast
                RaycastHit2D hit = Physics2D.Raycast(ray_origin, Vector2.up * direction_y, ray_length, passenger_mask);

                // Draw rays for debug
                Debug.DrawRay(ray_origin, Vector2.up * direction_y * ray_length, Color.red);

                if (hit)
                {
                    // Only move each passenger once. Checked against hash set
                    if (!moved_passengers.Contains(hit.transform))
                    {
                        // Add passenger to hash set
                        moved_passengers.Add(hit.transform);

                        // Move the passenger
                        float push_x = (direction_y == 1) ? velocity.x : 0;
                        float push_y = velocity.y - (hit.distance - skin_width) * direction_y;

                        passenger_movement.Add(new PassengerMovement(hit.transform, new Vector3(push_x, push_y, 0), direction_y == 1, true));
                    }
                }
            }
        }

        // Horizontally moving platform
        if (velocity.x != 0)
        {
            // Calculate ray length based on velocity. Includes skin width
            float ray_length = Mathf.Abs(velocity.x) + skin_width;

            for (int i = 0; i < horizontal_ray_count; i++)
            {
                // Get the ray origin. This is from bottom right if moving right, and bottom left if moving left
                Vector2 ray_origin = (direction_x == -1) ? raycast_origins.bottom_left : raycast_origins.bottom_right;

                // Add the ray spacing to the origin based on which ray we are currently on. Also add the y velocity because we are checking where we will end up
                ray_origin += Vector2.up * (horizontal_ray_spacing * i);

                // Perform raycast
                RaycastHit2D hit = Physics2D.Raycast(ray_origin, Vector2.right * direction_x, ray_length, passenger_mask);

                // Draw rays for debug
                Debug.DrawRay(ray_origin, Vector2.right * direction_x * ray_length, Color.red);

                if (hit)
                {
                    // Only move each passenger once. Checked against hash set
                    if (!moved_passengers.Contains(hit.transform))
                    {
                        // Add passenger to hash set
                        moved_passengers.Add(hit.transform);

                        // Move the passenger
                        float push_x = velocity.x - (hit.distance - skin_width) * direction_x;
                        float push_y = -skin_width;

                        passenger_movement.Add(new PassengerMovement(hit.transform, new Vector3(push_x, push_y, 0), false, true));
                    }
                }
            }
        }

        // Passenger on top of a horizontaly or downward moving platform
        if(direction_y == -1 || velocity.y == 0 && velocity.x != 0)
        {
            // Only check a little above the object
            float ray_length = skin_width * 2;

            for (int i = 0; i < vertical_ray_count; i++)
            {
                // Get the ray origin. 
                Vector2 ray_origin = raycast_origins.top_left + Vector2.right * (vertical_ray_spacing * i);

                // Perform raycast
                RaycastHit2D hit = Physics2D.Raycast(ray_origin, Vector2.up, ray_length, passenger_mask);

                // Draw rays for debug
                Debug.DrawRay(ray_origin, Vector2.up * direction_y * ray_length, Color.red);

                if (hit)
                {
                    // Only move each passenger once. Checked against hash set
                    if (!moved_passengers.Contains(hit.transform))
                    {
                        // Add passenger to hash set
                        moved_passengers.Add(hit.transform);

                        // Move the passenger
                        float push_x = velocity.x;
                        float push_y = velocity.y;

                        passenger_movement.Add(new PassengerMovement(hit.transform, new Vector3(push_x, push_y), true, false));
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (local_waypoints != null)
        {
            Gizmos.color = Color.red;
            float size = .3f;

            for (int i = 0; i < local_waypoints.Length; i++)
            {
                Vector3 global_waypoint_pos = (Application.isPlaying) ? global_waypoints[i] : local_waypoints[i] + transform.position;

                Gizmos.DrawLine(global_waypoint_pos - Vector3.up * size, global_waypoint_pos + Vector3.up* size);
                Gizmos.DrawLine(global_waypoint_pos - Vector3.left * size, global_waypoint_pos + Vector3.left * size);
            }
        }
    }
}
