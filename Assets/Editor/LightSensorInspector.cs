using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LightSensor))]
public class LightSensorInspector : Editor
{
    private bool drawLightCheckPoints = false;
    private float pointSize = 0.1f;
    private string intensity = "Run scene to see intensity";

    void OnEnable()
    {
        this.AddObserver(OnLightStatusNotification, LightSensor.LightStatusNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnLightStatusNotification, LightSensor.LightStatusNotification);
    }

    void OnLightStatusNotification(object sender, object args)
    {
        LightSensor sensor = target as LightSensor;
        Info<GameObject, float> info = (Info<GameObject, float>) args;
        if (info.arg0.GetComponent<LightSensor>() == sensor)
        {
            intensity = info.arg1.ToString();
        }
    }

    void OnSceneGUI()
    {
        LightSensor sensor = target as LightSensor;

        // Draw test sunlight raycasts
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
            // Toggles drawing of the object's lightCheckPoints
            drawLightCheckPoints = !drawLightCheckPoints;
            Debug.Log("Draw Light Check Points: " + drawLightCheckPoints);
        }
        GUILayout.Label("Current light intensity:");
        GUILayout.TextField(intensity);
    }
}
