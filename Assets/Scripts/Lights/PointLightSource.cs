using System.Collections.Generic;
using UnityEngine;

public class PointLightSource : LightSource
{
    /*
     * Check light range for LightSensors that should be lit
     */
    public override void LightSensorCheck()
    {
        litObjects = new List<GameObject>();

        if (!light.enabled)
            return;

        Vector3 position = transform.position;
        float range = light.range;
        Collider[] cols = Physics.OverlapSphere(position, range, layerMask,
            QueryTriggerInteraction.Ignore);

        foreach (Collider col in cols)
        {
            // check object has a LightSensor
            LightSensor sensor = col.gameObject.GetComponent<LightSensor>();
            if (sensor == null)
                continue;

            Vector3 colPos = col.transform.position;
            RaycastHit hitInfo;
            if (Physics.Raycast(position, colPos - position, out hitInfo, range, layerMask))
            {
                // perform initial position check
                if (hitInfo.transform == col.transform)
                {
                    litObjects.Add(sensor.gameObject);
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
            List<Vector3> points = sensor.GetLightCheckPoints(colPos - position);
            foreach (Vector3 point in points)
            {
                Ray ray = new Ray(position, point - position);
                if (Physics.Raycast(ray, out hitInfo, range, layerMask, QueryTriggerInteraction.Ignore))
                {
                    // only trigger 'lit' status if raycast hits the sensor object
                    if (hitInfo.transform == col.transform)
                    {
                        litObjects.Add(sensor.gameObject);
                        sensor.LightObject(GetIntensity(hitInfo.point));
                        break;
                    }
                }
            }
        }
    }

    // TODO: remove - collects raycast info for testing
    //public override void LightSensorCheck()
    //{
    //    List<Vector3> testPoints = new List<Vector3>();
    //    List<Vector3> rays = new List<Vector3>();
    //    List<bool> hits = new List<bool>();

    //    Vector3 position = transform.position;
    //    float range = light.range;
    //    Collider[] cols = Physics.OverlapSphere(position, range, layerMask,
    //        QueryTriggerInteraction.Ignore);

    //    foreach (Collider col in cols)
    //    {
    //        // check object has a LightSensor
    //        LightSensor sensor = col.gameObject.GetComponent<LightSensor>();
    //        if (sensor == null)
    //            continue;

    //        Vector3 colPos = col.transform.position;
    //        RaycastHit hitInfo;
    //        testPoints.Add(position);
    //        if (Physics.Raycast(position, colPos - position, out hitInfo, range, layerMask))
    //        {
    //            rays.Add((colPos - position).normalized * hitInfo.distance);
    //            // perform initial position check
    //            if (hitInfo.transform == col.transform)
    //            {
    //                hits.Add(true);
    //                sensor.LightObject(GetIntensity(hitInfo.point));
    //                continue;
    //            }
    //            else
    //            {
    //                hits.Add(false);
    //            }
    //        }
    //        else
    //        {
    //            rays.Add((colPos - position).normalized * range);
    //            hits.Add(false);
    //        }
    //        // if no custom points defined, can just skip next bit of logic
    //        if (!sensor.UseCustomPoints())
    //            continue;

    //        // TODO: This section will finish when the first lit point is found. 
    //        // Thus the intensity returned will be the first one found, rather then the highest of all points hit.
    //        // Unsure what ways around this there might be without constantly needing to check every point.
    //        // TODO: Somehow order points by distance from light????
    //        List<Vector3> points = sensor.GetLightCheckPoints(colPos - position);
    //        foreach (Vector3 point in points)
    //        {
    //            testPoints.Add(position);
    //            Ray ray = new Ray(position, point - position);
    //            if (Physics.Raycast(ray, out hitInfo, range, layerMask, QueryTriggerInteraction.Ignore))
    //            {
    //                rays.Add((point - position).normalized * hitInfo.distance);
    //                // only trigger 'lit' status if raycast hits the sensor object
    //                if (hitInfo.transform == col.transform)
    //                {
    //                    hits.Add(true);
    //                    sensor.LightObject(GetIntensity(hitInfo.point));
    //                    break;
    //                }
    //                hits.Add(false);
    //            }
    //            else
    //            {
    //                rays.Add((point - position).normalized * range);
    //                hits.Add(false);
    //            }
    //        }
    //    }

    //    testRaycasts = new Info<List<Vector3>, List<Vector3>, List<bool>>(testPoints, rays, hits);
    //}
}
