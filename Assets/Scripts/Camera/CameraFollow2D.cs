using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Space]
    [Header("Camera Follow")]
    // Setting for camera follow
    [SerializeField] private GameObject player;
    [SerializeField] private float cameraDistance = -10f;
    [SerializeField] [Range(0,1)] private float timeOffset;

    [Space]
    [Header("Boundries")]
    // Bounding box for camera follow
    [SerializeField] [Range(-100, 100)] private float leftLimit;
    [SerializeField] [Range(-100, 100)] private float rightLimit;
    [SerializeField] [Range(-100, 100)] private float topLimit;
    [SerializeField] [Range(-100, 100)] private float bottomLimit;

    // Used for smooth dampening of camera movement
    private Vector3 velocity;

    // Update is called once per frame
    void Update()
    {
        // Current camera position
        Vector3 startPos = transform.position;

        // Player position
        Vector3 endPos = player.transform.position;
        endPos.z = cameraDistance;

        // Smooth camera motion
        transform.position = Vector3.SmoothDamp(startPos, endPos, ref velocity, timeOffset);

        //transform.position = new Vector3(
         //   Mathf.Clamp(transform.position.x, leftLimit, rightLimit),
          //  Mathf.Clamp(transform.position.y, bottomLimit, topLimit),
          //  cameraDistance);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawLine(new Vector2(leftLimit, topLimit), new Vector2(rightLimit, topLimit));
        //Gizmos.DrawLine(new Vector2(rightLimit, topLimit), new Vector2(rightLimit, bottomLimit));
        //Gizmos.DrawLine(new Vector2(rightLimit, bottomLimit), new Vector2(leftLimit, bottomLimit));
        //Gizmos.DrawLine(new Vector2(leftLimit, bottomLimit), new Vector2(leftLimit, topLimit));
    }
}
