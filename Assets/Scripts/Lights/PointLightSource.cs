using System.Collections.Generic;
using UnityEngine;

public class PointLightSource : LightSource
{
    /*
     * Check light range for LightSensors that should be lit
     */
    public override void LightSensorCheck()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, light.range, layerMask,
            QueryTriggerInteraction.Ignore);

        float range = light.range;

        foreach (Collider col in cols)
        {
            // check object has a LightSensor
            LightSensor sensor = col.gameObject.GetComponent<LightSensor>();
            if (sensor == null)
                continue;

            RaycastHit hitInfo;
            if (Physics.Raycast(col.transform.position, col.transform.position - transform.position, out hitInfo, range, layerMask))
            {
                // perform initial position check
                if (hitInfo.transform == col.transform)
                {
                    sensor.LightObject(GetIntensity(hitInfo.point));
                    continue;
                }
            }
            // if no custom points defined, can just skip next bit of logic
            if (!sensor.UseCustomPoints())
                continue;

            // TODO: This section will finish when the first lit point is found. 
            // Thus the intensity returned will be the first one found, rather then the highest of all points hit.
            // Unsure what ways around this there might be without constantly needing to check every point.
            // TODO: Somehow order points by distance from light????
            List<Vector3> points = sensor.GetLightCheckPoints(col.transform.position - transform.position);
            foreach (Vector3 point in points)
            {
                Ray ray = new Ray(transform.position, point - transform.position);
                if (Physics.Raycast(ray, out hitInfo, range, layerMask, QueryTriggerInteraction.Ignore))
                {
                    // only trigger 'lit' status if raycast hits the sensor object
                    if (hitInfo.transform == col.transform)
                    {
                        sensor.LightObject(GetIntensity(hitInfo.point));
                        break;
                    }
                }
            }
        }
    }
}
