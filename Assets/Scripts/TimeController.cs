﻿using System.Collections;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    /*
     * Handles scene time and time-related functions.
     * 
     * TODO: extend to allow pausing and timer functions eg: run method when time has passed
     */
    private static float timeScale;

    void Start()
    {
        timeScale = Time.timeScale;
    }

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

    public static void Pause()
    {
        Time.timeScale = 0f;
    }

    public static void UnPause()
    {
        Time.timeScale = timeScale;
    }
}
