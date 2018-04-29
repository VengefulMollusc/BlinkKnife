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
            if (sensor == null)
                continue;

            RaycastHit hitInfo;
            Ray ray = new Ray(transform.position, col.transform.position - transform.position);

            // TODO: possibly also light object if raycast doesn't hit anything, as only part of collider may be in range
            if (Physics.Raycast(ray, out hitInfo, light.range, layerMask, QueryTriggerInteraction.Ignore))
            {
                // only trigger 'lit' status if raycast hits the sensor object
                if (hitInfo.transform == col.transform)
                    sensor.LightObject();
            }
        }
    }
}
