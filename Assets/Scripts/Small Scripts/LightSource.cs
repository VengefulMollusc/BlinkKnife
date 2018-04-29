using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSource : MonoBehaviour
{
    public static float updateFrequency = 0.1f;

    private Light light;

    void OnEnable()
    {
        light = GetComponent<Light>();

        InvokeRepeating("LightSensorCheck", 0f, updateFrequency);
    }

    private void LightSensorCheck()
    {
        
    }
}
