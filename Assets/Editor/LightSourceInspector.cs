using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LightSource))]
public class LightSourceInspector : Editor {

    void OnSceneGUI()
    {
        LightSource source = target as LightSource;

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
