using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpotLightSource))]
public class SpotLightSourceInspector : Editor {

    void OnSceneGUI()
    {
        SpotLightSource source = target as SpotLightSource;

        // Draw test raycasts
        Info<List<Vector3>, List<Vector3>, List<bool>> testInfo = source.GetTestRaycasts();
        if (testInfo != null)
        {
            for (int i = 0; i < testInfo.arg1.Count; i++)
            {
                Handles.color = (testInfo.arg2[i]) ? Color.green : Color.red;
                Handles.DrawLine(testInfo.arg0[i], testInfo.arg0[i] + testInfo.arg1[i]);
            }
        }
    }
}

[CustomEditor(typeof(PointLightSource))]
public class PointLightSourceInspector : Editor
{

    void OnSceneGUI()
    {
        PointLightSource source = target as PointLightSource;

        // Draw test raycasts
        Info<List<Vector3>, List<Vector3>, List<bool>> testInfo = source.GetTestRaycasts();
        if (testInfo != null)
        {
            for (int i = 0; i < testInfo.arg1.Count; i++)
            {
                Handles.color = (testInfo.arg2[i]) ? Color.green : Color.red;
                Handles.DrawLine(testInfo.arg0[i], testInfo.arg0[i] + testInfo.arg1[i]);
            }
        }
    }
}
