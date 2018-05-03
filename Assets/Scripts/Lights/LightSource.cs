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

    public virtual float GetIntensity(Vector3 _point)
    {
        return 1f;
    }
}
