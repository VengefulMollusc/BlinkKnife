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
        if (!light.enabled)
            return;

        Vector3 position = transform.position;
        Vector3 forward = transform.forward;
        float lightAngle = light.spotAngle * 0.5f;
        float lightRange = light.range;
        float radius = Mathf.Tan(lightAngle * Mathf.Deg2Rad) * lightRange;
        float coneHyp = Mathf.Sqrt(lightRange * lightRange + radius * radius);

        Collider[] cols = Physics.OverlapCapsule(position + (forward * radius), position + (forward * lightRange), radius, layerMask,
            QueryTriggerInteraction.Ignore);

        foreach (Collider col in cols)
        {
            // Check object has a LightSensor
            LightSensor sensor = col.GetComponent<LightSensor>();
            if (sensor == null)
                continue;

            RaycastHit hitInfo;
            // perform initial position check
            Vector3 sensorPos = sensor.transform.position;
            float hitAngle = Vector3.Angle(forward, sensorPos - position);
            float rayLength = Utilities.MapValues(hitAngle, 0f, lightAngle, lightRange, coneHyp);
            if (hitAngle <= lightAngle && Physics.Raycast(position, sensorPos - position, out hitInfo, rayLength,
                layerMask))
            {
                if (hitInfo.transform == sensor.transform)
                {
                    sensor.LightObject(GetIntensity(hitInfo.point));
                    continue;
                }
            }
            // if no custom points defined, can just skip expanded check logic
            if (!sensor.UseCustomPoints())
                continue;

            // TODO: This section will finish when the first lit point is found. 
            // Thus the intensity returned will be the first one found, rather then the highest of all points hit.
            // Unsure what ways around this there might be without constantly needing to check every point.
            // TODO: Somehow order points by distance from light????
            List<Vector3> points = sensor.GetLightCheckPoints(sensorPos - position);
            foreach (Vector3 point in points)
            {
                // check if the point is within the spot angle
                Vector3 dir = point - position;
                hitAngle = Vector3.Angle(forward, dir);
                if (hitAngle > lightAngle)
                    continue;

                Ray ray = new Ray(position, dir);
                rayLength = Utilities.MapValues(hitAngle, 0f, lightAngle, lightRange, coneHyp);
                if (Physics.Raycast(ray, out hitInfo, rayLength, layerMask, QueryTriggerInteraction.Ignore))
                {
                    // only trigger 'lit' status if raycast hits the sensor object
                    if (hitInfo.transform == sensor.transform)
                    {
                        sensor.LightObject(GetIntensity(hitInfo.point));
                        break;
                    }
                }
            }
        }
    }

    // TODO: remove
    //public override void LightSensorCheck()
    //{
    //    List<Vector3> testPoints = new List<Vector3>();
    //    List<Vector3> rays = new List<Vector3>();
    //    List<bool> rayHits = new List<bool>();

    //    Vector3 position = transform.position;
    //    Vector3 forward = transform.forward;
    //    float lightAngle = light.spotAngle * 0.5f;
    //    float lightRange = light.range;
    //    float radius = Mathf.Tan(lightAngle * Mathf.Deg2Rad) * lightRange;
    //    float coneHyp = Mathf.Sqrt(lightRange * lightRange + radius * radius);

    //    Collider[] cols = Physics.OverlapCapsule(position + (forward * radius), position + (forward * lightRange), radius, layerMask,
    //        QueryTriggerInteraction.Ignore);

    //    foreach (Collider col in cols)
    //    {
    //        // Check object has a LightSensor
    //        LightSensor sensor = col.GetComponent<LightSensor>();
    //        if (sensor == null)
    //            continue;

    //        RaycastHit hitInfo;
    //        // perform initial position check
    //        Vector3 sensorPos = sensor.transform.position;
    //        float hitAngle = Vector3.Angle(forward, sensorPos - position);
    //        float rayLength = Utilities.MapValues(hitAngle, 0f, lightAngle, lightRange, coneHyp);
    //        testPoints.Add(position);
    //        if (hitAngle <= lightAngle)
    //        {
    //            if (Physics.Raycast(position, sensorPos - position, out hitInfo, rayLength,
    //                layerMask))
    //            {
    //                rays.Add((sensorPos - position).normalized * hitInfo.distance);
    //                if (hitInfo.transform == sensor.transform)
    //                {
    //                    rayHits.Add(true);
    //                    sensor.LightObject(GetIntensity(hitInfo.point));
    //                    continue;
    //                }
    //                rayHits.Add(false);
    //            }
    //            else
    //            {
    //                rays.Add((sensorPos - position).normalized * rayLength);
    //                rayHits.Add(false);
    //                //continue;
    //            }
    //        }
    //        // if no custom points defined, can just skip expanded check logic
    //        if (!sensor.UseCustomPoints())
    //            continue;

    //        // TODO: This section will finish when the first lit point is found. 
    //        // Thus the intensity returned will be the first one found, rather then the highest of all points hit.
    //        // Unsure what ways around this there might be without constantly needing to check every point.
    //        // TODO: Somehow order points by distance from light????
    //        List<Vector3> points = sensor.GetLightCheckPoints(sensorPos - position);
    //        foreach (Vector3 point in points)
    //        {
    //            // check if the point is within the spot angle
    //            Vector3 dir = point - position;
    //            hitAngle = Vector3.Angle(forward, dir);
    //            if (hitAngle > lightAngle)
    //                continue;

    //            testPoints.Add(position);
    //            Ray ray = new Ray(position, dir);
    //            rayLength = Utilities.MapValues(hitAngle, 0f, lightAngle, lightRange, coneHyp);
    //            if (Physics.Raycast(ray, out hitInfo, rayLength, layerMask, QueryTriggerInteraction.Ignore))
    //            {
    //                rays.Add(dir.normalized * hitInfo.distance);
    //                // only trigger 'lit' status if raycast hits the sensor object
    //                if (hitInfo.transform == sensor.transform)
    //                {
    //                    rayHits.Add(true);
    //                    sensor.LightObject(GetIntensity(hitInfo.point));
    //                    break;
    //                }
    //                rayHits.Add(false);
    //            }
    //            else
    //            {
    //                rays.Add(dir.normalized * rayLength);
    //                rayHits.Add(false);
    //            }
    //        }
    //    }

    //    testRaycasts = new Info<List<Vector3>, List<Vector3>, List<bool>>(testPoints, rays, rayHits);
    //}
}
