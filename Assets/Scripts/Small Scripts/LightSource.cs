using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSource : MonoBehaviour
{
    private float updateFrequency = 0.1f;

    private Light light;

    [SerializeField]
    private LayerMask layerMask;

    void OnEnable()
    {
        light = GetComponent<Light>();

        InvokeRepeating("LightSensorCheck", 0f, updateFrequency);
    }

    private void LightSensorCheck()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, light.range, layerMask,
            QueryTriggerInteraction.Ignore);

        foreach (Collider col in cols)
        {
            LightSensor sensor = col.gameObject.GetComponent<LightSensor>();
            if (sensor)
            {
                sensor.LightObject();
            }
        }
    }
}
