using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LightSensor))]
public class LightSensorInspector : Editor
{
    private bool drawLightCheckPoints = false;
    private float pointSize = 0.1f;

    void OnSceneGUI()
    {
        LightSensor sensor = target as LightSensor;

        Info<List<Vector3>, List<Vector3>, List<bool>> testInfo = sensor.GetRaycastInfo();
        if (testInfo != null)
        {
            for (int i = 0; i < testInfo.arg1.Count; i++)
            {
                Handles.color = (testInfo.arg2[i]) ? Color.green : Color.red;
                Handles.DrawLine(testInfo.arg0[i], testInfo.arg0[i] + testInfo.arg1[i]);
            }
        }

        // Draw light check points
        if (drawLightCheckPoints)
        {
            Handles.color = Color.yellow;
            // draw custom points
            foreach (Vector3 point in sensor.GetUnmodifiedPoints())
                Handles.SphereHandleCap(0, point, Quaternion.identity, pointSize, EventType.Repaint);
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
