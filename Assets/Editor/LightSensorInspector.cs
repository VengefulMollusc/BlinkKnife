using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LightSensor))]
public class LightSensorInspector : Editor
{

    void OnSceneGUI()
    {
        LightSensor lightSensor = target as LightSensor;

        Info<Vector3, Vector3, bool> info = lightSensor.GetRaycastInfo();

        if (info == null)
            return;

        Handles.color = (info.arg2) ? Color.green : Color.red;
        Handles.DrawLine(info.arg0, info.arg1);
    }
}
