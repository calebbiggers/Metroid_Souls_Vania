using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private static float y_factor = .25f;  // Factor by which to lessen vertical parallax
    private Camera camera;          // Reference to camera  
    private Transform subject;      // Subject to follow. In this case the player   
    private Vector2 start_position; // Vector2 storing the start position of the object 
    private float start_z;          // The start z of the object. Stored seperately to make vector math easier

    // The distance the subject has travelled from its starting position
    Vector2 travel => (Vector2) subject.position - start_position;

    // The distance this object is from the subject on the z axis. AKA depth
    float distance_from_subject => transform.position.z - subject.position.z;
    
    // Distance from the clipping plane. 
    float clipping_plane => (camera.transform.position.z + (distance_from_subject > 0 ? camera.farClipPlane : camera.nearClipPlane));
    
    float parallax_factor => Mathf.Abs(distance_from_subject) / clipping_plane;

    private void Start()
    {
        // Get the main camera
        camera = Camera.main;

        // Get the player
        //subject = GameObject.FindGameObjectWithTag("Player").transform;
        subject = Camera.main.transform;

        // Get the start position
        start_position = transform.position;
        start_z = transform.position.z;
    }

    public void Update()
    {
        // Get new x position and y position. Y position has less parallax
        float new_x_position = start_position.x + travel.x * parallax_factor;
        float new_y_position = start_position.y + travel.y * parallax_factor * y_factor;

        // Set the new position
        transform.position = new Vector3(new_x_position, new_y_position, start_z);
    }


}
