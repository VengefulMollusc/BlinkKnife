using System.Collections;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    /*
     * Handles scene time and time-related functions.
     * 
     * TODO: extend to allow pausing and timer functions eg: run method when time has passed
     */

    public static float DeltaTime()
    {
        return Time.deltaTime;
    }

    public static float FixedDeltaTime()
    {
        return Time.fixedDeltaTime;
    }

    public static float TimeSinceLevelLoad()
    {
        return Time.timeSinceLevelLoad;
    }

    public static void Pause(bool paused = true)
    {
        Time.timeScale = (paused) ? 0f : 1f;
    }

    public static void UnPause()
    {
        Time.timeScale = 1f;
    }
}
