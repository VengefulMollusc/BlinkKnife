using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LightSensor))]
public class LightSensorInspector : Editor
{
    private bool drawLightCheckPoints = false;
    private float pointSize = 1f;

    void OnSceneGUI()
    {
        LightSensor sensor = target as LightSensor;

        Info<Vector3, Vector3, bool> info = sensor.GetRaycastInfo();
        if (info != null)
        {
            Handles.color = (info.arg2) ? Color.green : Color.red;
            Handles.DrawLine(info.arg0, info.arg1);
        }

        // Draw light check points
        if (drawLightCheckPoints)
        {
            Handles.color = Color.yellow;
            if (sensor.useCustomLightCheckPoints && sensor.customLightCheckPoints.Count > 0)
            {
                // draw custom points
                foreach (Vector3 point in sensor.customLightCheckPoints)
                    Handles.SphereHandleCap(0, sensor.transform.position + point, Quaternion.identity, pointSize, EventType.Repaint);
            }
            else
            {
                // draw default 5 point config
                foreach (Vector3 point in sensor.GetLightCheckPoints(-sensor.transform.forward))
                    Handles.SphereHandleCap(0, point, Quaternion.identity, pointSize, EventType.Repaint);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Toggle LightCheckPoints"))
        {
            drawLightCheckPoints = !drawLightCheckPoints;
            Debug.Log("Draw Light Check Points: " + drawLightCheckPoints);
        }
    }
}
