using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotLightSource : LightSource {

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

            
            List<Vector3> points = sensor.GetLightCheckPoints(sensor.transform.position - transform.position);
            int litCount = 0;
            bool useLitPercent = sensor.UseLitPercent();
            foreach (Vector3 point in points)
            {
                // TODO: tweak these checks, this is going to be really sensitive
                Vector3 dir = point - transform.position;
                float hitAngle = Vector3.Angle(transform.forward, dir);
                if (hitAngle > lightAngle)
                    continue;

                RaycastHit hitInfo;
                Ray ray = new Ray(transform.position, dir);
                float rayLength = Utilities.MapValues(hitAngle, 0f, lightAngle, lightRange, coneHyp);
                if (Physics.Raycast(ray, out hitInfo, rayLength, layerMask, QueryTriggerInteraction.Ignore))
                {
                    // only trigger 'lit' status if raycast hits the sensor object
                    if (hitInfo.transform == sensor.transform)
                    {
                        if (!useLitPercent)
                        {
                            sensor.LightObject(1f);
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

        //Collider[] cols = Physics.OverlapSphere(transform.position, lightRange, layerMask,
        //    QueryTriggerInteraction.Ignore);

        //foreach (Collider col in cols)
        //{
        //    // Check object has a LightSensor
        //    LightSensor sensor = col.gameObject.GetComponent<LightSensor>();
        //    if (sensor == null)
        //        continue;

        //    Vector3 dir = col.transform.position - transform.position;

        //    // Check object is within the cone's forward angle
        //    if (Vector3.Angle(transform.forward, dir) > lightAngle)
        //        continue;

        //    // TODO: need good solution for partially lit objects
        //    RaycastHit hitInfo;
        //    Ray ray = new Ray(transform.position, dir);
        //    if (Physics.Raycast(ray, out hitInfo, lightRange, layerMask, QueryTriggerInteraction.Ignore))
        //    {
        //        // only trigger 'lit' status if raycast hits the sensor object
        //        if (hitInfo.transform == col.transform)
        //            sensor.LightObject();
        //    }
        //}
    }
}
