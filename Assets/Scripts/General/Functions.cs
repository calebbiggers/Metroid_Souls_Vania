using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Functions
{
    // Used to print general info to debug log in color
    public static void DebugLog(string in_string)
    {
        Debug.Log("<color=white>[DEBUG] - " + in_string + "</color>");
    }

    // Used to print errors to debug log in color
    public static void DebugLogError(string in_string)
    {
        Debug.Log("<color=red>[Error] - " + in_string + "</color>");
    }

    // Converts unity units to perfect pixel units
    public static float RoundToNearestPixel(float unity_units, Camera camera)
    {
        float value_in_pixels = (Screen.height / (camera.orthographicSize * 2)) * unity_units;
        value_in_pixels = Mathf.Round(value_in_pixels);
        float adjusted_unity_units = value_in_pixels / (Screen.height / (camera.orthographicSize * 2));
        return adjusted_unity_units;
    }

}
