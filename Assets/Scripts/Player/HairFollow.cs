using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HairFollow : MonoBehaviour
{
    [Space]
    [Header("Hair Config")]
    [SerializeField] private GameObject hair_base;  // Reference to base that hair is following
    [SerializeField] private GameObject following;  // Reference to next hair piece in sequence
    [SerializeField] private Vector3 offset;        // Offset from target
    [SerializeField] public float dampening;        // How much to dampen follow by
    [SerializeField] public float scale_factor;     // How much to scale the sprite by (changes by distance from hair bases
    [SerializeField] private float max_distance;    // Max distance to be allowed from next hair piece

    [Space]
    [Header("Hair Stats")]
    [SerializeField] private float distance;        // Distance from hair base

    private Vector3 velocity;      // Used for dampening

    // Update is called once per frame
    void Update()
    {
        // Get distance from base position
        distance = Mathf.Abs(Vector3.Distance(transform.position, hair_base.transform.position));

        // Set the local scale based on distance
        // scale = ((max_scale - min_scale) / .6) distance + max_scale
        transform.localScale = new Vector3(1, 1, 1) * (-.0166f * distance + .07f);

        // Set the max distance this hair section can be from its next one
        max_distance = transform.localScale.x * .3f;

        // Get config from next hair section
        if (following.GetComponent<HairFollow>() != null)
        {
            // Get previous hair section
            HairFollow hair_followed = following.GetComponent<HairFollow>();

            // Config to match
            offset = hair_followed.offset;
            dampening = hair_followed.dampening;
            scale_factor = hair_followed.scale_factor;
        }

        // Calculate offset position
        Vector2 end_position = following.transform.position + offset;

        // Smooth the transition
        transform.position = Vector3.SmoothDamp(transform.position, end_position, ref velocity, dampening);

        // Clamp position
        transform.position = new Vector2(
            Mathf.Clamp(transform.position.x, following.transform.position.x - max_distance, following.transform.position.x + max_distance),
            Mathf.Clamp(transform.position.y, following.transform.position.y - max_distance, following.transform.position.y + max_distance));
    }
}
