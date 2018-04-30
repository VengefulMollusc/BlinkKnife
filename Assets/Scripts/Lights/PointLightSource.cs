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

        foreach (Collider col in cols)
        {
            LightSensor sensor = col.gameObject.GetComponent<LightSensor>();
            if (sensor == null)
                continue;

            foreach (Vector3 point in sensor.GetLightCheckPoints(col.transform.position - transform.position))
            {
                RaycastHit hitInfo;
                Ray ray = new Ray(transform.position, point - transform.position);
                // TODO: possibly also light object if raycast doesn't hit anything, as only part of collider may be in range
                // ^ lightcheckpoints should solve this
                if (Physics.Raycast(ray, out hitInfo, light.range, layerMask, QueryTriggerInteraction.Ignore))
                {
                    // only trigger 'lit' status if raycast hits the sensor object
                    if (hitInfo.transform == col.transform)
                    {
                        sensor.LightObject();
                        break;
                    }
                }
            }
        }
    }
}
