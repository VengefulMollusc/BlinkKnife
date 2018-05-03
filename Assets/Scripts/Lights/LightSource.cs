using System.Collections.Generic;
using UnityEngine;

public class LightSource : MonoBehaviour {

    private float updateFrequency = 0.1f;

    [HideInInspector]
    public Light light;
    
    public LayerMask layerMask;

    public virtual void OnEnable()
    {
        light = GetComponent<Light>();

        InvokeRepeating("LightSensorCheck", 0f, updateFrequency);
    }

    /*
     * Check light range for LightSensors that should be lit
     */
    public virtual void LightSensorCheck()
    {
        Debug.LogError("LightSensorCheck must be overridden");
        CancelInvoke("LightSensorCheck");
    }

    /*
     * Gets the intensity of the light at a given point.
     * 
     * Assumes specific LightSource script has checked that this is within bounds of light
     */
    public virtual float GetIntensity(Vector3 _point)
    {
        float distance = Vector3.Distance(transform.position, _point);
        if (distance > light.range)
            return 0f;

        return light.intensity * distance / light.range;
    }
}
