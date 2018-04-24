using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class FibreOpticMeshCreator : MonoBehaviour {

    private static Info<Vector3, Vector3, Vector3, Vector3> bezier;
    private static float radius = 0.5f;
    private static int lengthSegmentCount;
    private static float segmentModifier;
    private static int radiusSegmentCount = 10;
    private static Mesh mesh;
    private static Vector3[] meshVertices;
    private static int[] meshTriangles;
    
    private static Vector3[,] vertexArray;

    public static Mesh CreateMeshForBezier(Info<Vector3, Vector3, Vector3, Vector3> _bezier, int _segments)
    {
        bezier = _bezier;
        lengthSegmentCount = _segments;

        mesh = new Mesh();
        mesh.name = "FibreOptic Bezier";

        CreateMeshVertices();
        SetVertices();
        SetTriangles();

        mesh.RecalculateNormals();

        return mesh;
    }

    public static Vector3[] GetBezierMeshVertices(Info<Vector3, Vector3, Vector3, Vector3> _bezier, int _segments)
    {
        CreateMeshForBezier(_bezier, _segments);
        return meshVertices;
    }

    private static void CreateMeshVertices()
    {
        // Get lists of points and tangents for bezier
        Vector3[] points = new Vector3[lengthSegmentCount + 1];
        Vector3[] tangents = new Vector3[lengthSegmentCount + 1];
        for (int i = 0; i <= lengthSegmentCount; i++)
        {
            float t = ((float) i / (float) lengthSegmentCount);
            points[i] = Utilities.LerpBezier(bezier, t);
            tangents[i] = Utilities.BezierDerivative(bezier, t);
        }

        // Create basic circle of points
        Vector3[] circle = new Vector3[radiusSegmentCount + 1];
        Vector3 baseCircleVector = Vector3.up * radius;
        float angle = 360f / radiusSegmentCount;
        for (int i = 0; i <= radiusSegmentCount; i++)
        {
            circle[i] = Quaternion.AngleAxis(angle * i, Vector3.forward) * baseCircleVector;
        }

        // Align circle with bezier tangent at each step and record points
        vertexArray = new Vector3[lengthSegmentCount + 1, radiusSegmentCount + 1];

        for (int i = 0; i <= lengthSegmentCount; i++)
        {
            Quaternion rot = Quaternion.LookRotation(tangents[i]);
            for (int j = 0; j < circle.Length; j++)
            {
                vertexArray[i, j] = points[i] + (rot * circle[j]);
            }
        }
    }

    private static Vector3 GetMeshVertex(int iLength, int iRadius)
    {
        return vertexArray[iLength, iRadius];
    }

    private static void SetVertices()
    {
        meshVertices = new Vector3[lengthSegmentCount * radiusSegmentCount * 4];

        CreateFirstQuadRing(1);

        int iDelta = radiusSegmentCount * 4;
        for (int u = 2, i = iDelta; u <= lengthSegmentCount; u++, i += iDelta)
        {
            CreateQuadRing(u, i);
        }

        mesh.vertices = meshVertices;
    }

    private static void CreateFirstQuadRing(int u)
    {
        Vector3 vertexA = GetMeshVertex(0, 0);
        Vector3 vertexB = GetMeshVertex(u, 0);
        for (int v = 1, i = 0; v <= radiusSegmentCount; v++, i += 4)
        {
            meshVertices[i] = vertexA;
            meshVertices[i + 1] = vertexA = GetMeshVertex(0, v);
            meshVertices[i + 2] = vertexB;
            meshVertices[i + 3] = vertexB = GetMeshVertex(u, v);
        }
    }

    private static void CreateQuadRing(int u, int i)
    {
        int radiusOffset = radiusSegmentCount * 4;
        Vector3 vertex = GetMeshVertex(u, 0);
        for (int v = 1; v <= radiusSegmentCount; v++, i += 4)
        {
            meshVertices[i] = meshVertices[i - radiusOffset + 2];
            meshVertices[i + 1] = meshVertices[i - radiusOffset + 3];
            meshVertices[i + 2] = vertex;
            meshVertices[i + 3] = vertex = GetMeshVertex(u, v);
        }
    }

    private static void SetTriangles()
    {
        meshTriangles = new int[lengthSegmentCount * radiusSegmentCount * 6];
        for (int t = 0, i = 0; t < meshTriangles.Length; t += 6, i += 4)
        {
            meshTriangles[t] = i;
            meshTriangles[t + 1] = meshTriangles[t + 4] = i + 1;
            meshTriangles[t + 2] = meshTriangles[t + 3] = i + 2;
            meshTriangles[t + 5] = i + 3;
        }
        mesh.triangles = meshTriangles;
    }
}
