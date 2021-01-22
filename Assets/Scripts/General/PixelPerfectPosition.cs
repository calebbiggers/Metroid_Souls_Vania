using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelPerfectPosition : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {   
        // Update the position to the pixel perfect position
        transform.position = new Vector2(
            Functions.RoundToNearestPixel(transform.position.x, Camera.main),
            Functions.RoundToNearestPixel(transform.position.y, Camera.main));
    }
}
