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
            LightSensor sensor = col.gameObject.GetComponent<LightSensor>();
            if (sensor == null)
                continue;

            int litCount = 0;
            bool useLitPercent = sensor.UseLitPercent();
            List<Vector3> points = sensor.GetLightCheckPoints(col.transform.position - transform.position);
            foreach (Vector3 point in points)
            {
                RaycastHit hitInfo;
                Ray ray = new Ray(transform.position, point - transform.position);
                // TODO: possibly also light object if raycast doesn't hit anything, as only part of collider may be in range
                // ^ lightcheckpoints should solve this
                if (Physics.Raycast(ray, out hitInfo, range, layerMask, QueryTriggerInteraction.Ignore))
                {
                    // only trigger 'lit' status if raycast hits the sensor object
                    if (hitInfo.transform == col.transform)
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
    }
}
