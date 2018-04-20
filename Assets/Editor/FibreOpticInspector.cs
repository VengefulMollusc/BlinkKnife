using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FibreOpticController))]
public class FibreOpticInspector : Editor {

    private void OnSceneGUI()
    {
        FibreOpticController fibre = target as FibreOpticController;

        Info<Vector3, Vector3, Vector3, Vector3> bezierPoints = fibre.GetBezierPoints();

        if (bezierPoints == null)
            return;

        Transform fibreTransform = fibre.transform;

        Vector3 point1 = bezierPoints.arg0;
        Vector3 point2 = bezierPoints.arg3;
        Vector3 tangent1 = bezierPoints.arg1;
        Vector3 tangent2 = bezierPoints.arg2;

        Handles.DrawBezier(point1, point2, tangent1, tangent2, Color.red, null, 1f);

        Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            fibreTransform.rotation : Quaternion.identity;

        EditorGUI.BeginChangeCheck();
        tangent1 = Handles.DoPositionHandle(tangent1, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(fibre, "Move Point");
            EditorUtility.SetDirty(fibre);
            fibre.bezierTargetPosition = fibreTransform.InverseTransformPoint(tangent1);
        }
    }
}
