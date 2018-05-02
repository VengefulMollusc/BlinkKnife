using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotLightSource : LightSource
{

    /*
     * Check light range for LightSensors that should be lit
     */
    public override void LightSensorCheck()
    {
        float lightAngle = light.spotAngle * 0.5f;
        float lightRange = light.range;
        float radius = Mathf.Tan(lightAngle) * lightRange;
        float coneHyp = Mathf.Sqrt(lightRange * lightRange + radius * radius);

        Ray sphereCastRay = new Ray(transform.position, transform.forward);
        RaycastHit[] hits = Physics.SphereCastAll(sphereCastRay, radius, lightRange, layerMask,
            QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            // Check object has a LightSensor
            LightSensor sensor = hit.transform.gameObject.GetComponent<LightSensor>();
            if (sensor == null)
                continue;

            bool useLitPercent = sensor.UseLitPercent();
            RaycastHit hitInfo;

            if (!useLitPercent)
            {
                // perform initial position check
                Vector3 sensorPos = hit.transform.position;
                float hitAngle = Vector3.Angle(transform.forward, sensorPos - transform.position);
                if (hitAngle <= lightAngle && Physics.Raycast(sensorPos, sensorPos - transform.position, out hitInfo, lightRange, layerMask))
                {
                    if (hitInfo.transform == hit.transform)
                    {
                        sensor.LightObject();
                        continue;
                    }
                }
            }

            List<Vector3> points = sensor.GetLightCheckPoints(sensor.transform.position - transform.position);
            int litCount = 0;

            foreach (Vector3 point in points)
            {
                // check if the point is within the spot angle
                Vector3 dir = point - transform.position;
                float hitAngle = Vector3.Angle(transform.forward, dir);
                if (hitAngle > lightAngle)
                    continue;

                Ray ray = new Ray(transform.position, dir);
                float rayLength = Utilities.MapValues(hitAngle, 0f, lightAngle, lightRange, coneHyp);
                if (Physics.Raycast(ray, out hitInfo, rayLength, layerMask, QueryTriggerInteraction.Ignore))
                {
                    // only trigger 'lit' status if raycast hits the sensor object
                    if (hitInfo.transform == sensor.transform)
                    {
                        if (!useLitPercent)
                        {
                            sensor.LightObject();
                            break;
                        }
                        litCount++;
                    }
                }
            }

            if (useLitPercent && litCount > 0) // the useLitPercent check here should be redundant
            {
                sensor.LightObject((float)litCount / points.Count);
            }
        }
    }
}
