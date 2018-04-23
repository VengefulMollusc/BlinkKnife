using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class FibreOpticMeshCreator : MonoBehaviour {

    private static Info<Vector3, Vector3, Vector3, Vector3> bezier;
    private static float radius = 1f;
    private static int lengthSegmentCount = 20;
    private static int radiusSegmentCount = 10;
    private static Mesh mesh;

    public static Mesh CreateMeshForBezier(Info<Vector3, Vector3, Vector3, Vector3> _bezier)
    {
        bezier = _bezier;

        mesh = new Mesh();

        // Do the thing
        GenerateFibreMesh();

        return mesh;
    }

    public static Vector3[] GetBezierMeshVertices(Info<Vector3, Vector3, Vector3, Vector3> _bezier)
    {
        bezier = _bezier;
        
        return GenerateFibreMesh();
    }

    private static Vector3[] GenerateFibreMesh()
    {
        // Get lists of points and tangents for bezier
        Vector3[] points = new Vector3[lengthSegmentCount + 1];
        Vector3[] tangents = new Vector3[lengthSegmentCount + 1];
        for (int i = 0; i <= lengthSegmentCount; i++)
        {
            float t = ((float)i / (float)lengthSegmentCount);
            points[i] = Utilities.LerpBezier(bezier, t);
            tangents[i] = Utilities.BezierDerivative(bezier, t);
        }

        // Create basic circle of points
        Vector3[] circle = new Vector3[radiusSegmentCount];
        Vector3 baseCircleVector = Vector3.up * radius;
        float angle = 360f / radiusSegmentCount;
        for (int i = 0; i < radiusSegmentCount; i++)
        {
            circle[i] = Quaternion.AngleAxis(angle * i, Vector3.forward) * baseCircleVector;
        }

        // Do the thing
        Vector3[] vertices = new Vector3[radiusSegmentCount * (lengthSegmentCount + 1)];
        for (int i = 0; i <= lengthSegmentCount; i++)
        {
            Quaternion rot = Quaternion.LookRotation(tangents[i]);
            for (int j = 0; j < radiusSegmentCount; j++)
            {
                vertices[(i * radiusSegmentCount) + j] = points[i] + (rot * circle[j]);
            }
        }

        return vertices;
    }
}
