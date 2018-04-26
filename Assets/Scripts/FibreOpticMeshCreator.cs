using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using ProBuilder2.Common;
using UnityEngine;

public class FibreOpticMeshCreator : MonoBehaviour
{

    private static Info<Vector3, Vector3, Vector3, Vector3> bezier;
    private static int lengthSegmentCount;
    private static Mesh mesh;
    private static int[] meshTriangles;

    private static Vector3[] vertexList;

    private static Vector3[] innerVertexList;

    // variables that define mesh creation/detail
    private static bool doubleSided = true;
    private static float doubleSidedThickness = 0.05f;
    private static bool stitchEnds = true;
    private static float autoCreateResolution = 0.01f;
    private static float autoCreateTangentAngleThreshold = 10f; // 10f
    private static float autoCreateMaxSegmentLength = 10f;
    private static float radius = 0.5f;
    private static int radiusSegmentCount = 10;

    /*
     * Builds a mesh to fit a given FibreOptic bezier curve
     */
    public static Mesh CreateMeshForBezier(Info<Vector3, Vector3, Vector3, Vector3> _bezier, bool _createVerticesOnly = false)
    {
        bezier = _bezier;

        mesh = new Mesh { name = "FibreOptic Bezier" };
        AutoCreateMeshVertices();
        SetVertices();

        if (_createVerticesOnly)
            return mesh;

        SetTriangles();

        mesh.RecalculateNormals();

        return mesh;
    }

    /*
     * Returns the constructed list of vertices - used to generate wireframe mesh preview
     */
    public static Vector3[] GetBezierMeshVertices(Info<Vector3, Vector3, Vector3, Vector3> _bezier)
    {
        if (bezier == _bezier)
            return vertexList;

        CreateMeshForBezier(_bezier, true);
        return vertexList;
    }

    /*
     * distributes mesh vertices based on angle of change of bezier curve
     * 
     * places more vertices on curved sections and less on straight
     */
    private static void AutoCreateMeshVertices()
    {
        List<Vector3> points = new List<Vector3>();
        List<Vector3> tangents = new List<Vector3>();

        // Create base lists of points and tangents
        CreateBasePoints(points, tangents);

        // Create base circle from radius/radiusSegmentCount
        Vector3[] circle = CreateBaseCircle();
        Vector3[] innerCircle = CreateBaseCircle(doubleSidedThickness);

        // Align circle with bezier tangent at each step and record points
        vertexList = new Vector3[(lengthSegmentCount + 1) * circle.Length];

        if (doubleSided)
            innerVertexList = new Vector3[(lengthSegmentCount + 1) * circle.Length];

        int index = 0;

        for (int i = 0; i <= lengthSegmentCount; i++)
        {
            Quaternion rot = Quaternion.LookRotation(tangents[i]);
            for (int j = 0; j < circle.Length; j++)
            {
                vertexList[index] = points[i] + (rot * circle[j]);

                if (doubleSided)
                    innerVertexList[index] = points[i] + (rot * innerCircle[j]);

                index++;
            }
        }
    }


    /*
     * Populates lists of points and tangents based on bezier curve
     */
    private static void CreateBasePoints(List<Vector3> points, List<Vector3> tangents)
    {
        float u = 0f;
        while (u <= 1f)
        {
            Vector3 currentPoint = Utilities.LerpBezier(bezier, u);
            Vector3 currentTangent = Utilities.BezierDerivative(bezier, u);

            // add points if:
            // we are at each end or
            // the angle is greater than the threshold or 
            // we've reached the max segment length
            if (points.Count == 0 || currentPoint != points[points.Count - 1])
            {
                if (u < autoCreateResolution * 0.5f || u > 1f - (autoCreateResolution * 0.5f)
                    || Vector3.Angle(tangents[tangents.Count - 1], currentTangent) > autoCreateTangentAngleThreshold
                    || Vector3.Distance(points[points.Count - 1], currentPoint) > autoCreateMaxSegmentLength)
                {
                    points.Add(currentPoint);
                    tangents.Add(currentTangent);
                }
            }

            // increment and make sure the value at 1f will be checked
            if (u + autoCreateResolution > 1f && u < 1f - (autoCreateResolution * 0.5f))
                u = 1f;
            else
                u += autoCreateResolution;
        }

        lengthSegmentCount = points.Count - 1;
    }

    /*
     * Creates a circle of points defined by radius and radiusSegmentCount
     */
    private static Vector3[] CreateBaseCircle(float _thickness = 0f)
    {
        Vector3[] circle = new Vector3[radiusSegmentCount];
        Vector3 baseCircleVector = Vector3.up * (radius - _thickness);
        float angle = 360f / radiusSegmentCount;
        for (int i = 0; i < radiusSegmentCount; i++)
        {
            circle[i] = Quaternion.AngleAxis(angle * i, Vector3.forward) * baseCircleVector;
        }
        return circle;
    }

    /*
     * Assigns the list of vertices to the mesh object
     */
    private static void SetVertices()
    {
        mesh.vertices = (doubleSided) ? vertexList.Concat(innerVertexList) : vertexList;
    }

    /*
     * Constructs the list of vertices defining mesh triangles
     */
    private static void SetTriangles()
    {
        meshTriangles = new int[lengthSegmentCount * radiusSegmentCount * 6];
        for (int t = 0, i = 0; t < meshTriangles.Length; t += 6, i += 1)
        {
            meshTriangles[t] = i;
            meshTriangles[t + 2] = meshTriangles[t + 3] = i + radiusSegmentCount;

            // Use different indices if this quad is the last of this pipe segment
            bool mod = (i + 1) % radiusSegmentCount == 0;
            meshTriangles[t + 1] = meshTriangles[t + 4] = (mod) ? i + 1 - radiusSegmentCount : i + 1;
            meshTriangles[t + 5] = (mod) ? i + 1 : i + radiusSegmentCount + 1;
        }

        if (doubleSided)
        {
            int[] innerMeshTriangles = new int[lengthSegmentCount * radiusSegmentCount * 6];
            for (int t = 0, i = vertexList.Length; t < innerMeshTriangles.Length; t += 6, i += 1)
            {
                innerMeshTriangles[t] = i;
                innerMeshTriangles[t + 1] = innerMeshTriangles[t + 4] = i + radiusSegmentCount;

                // Use different indices if this quad is the last of this pipe segment
                bool mod = (i + 1) % radiusSegmentCount == 0;
                innerMeshTriangles[t + 2] = innerMeshTriangles[t + 3] = (mod) ? i + 1 - radiusSegmentCount : i + 1;
                innerMeshTriangles[t + 5] = (mod) ? i + 1 : i + radiusSegmentCount + 1;
            }
            meshTriangles = meshTriangles.Concat(innerMeshTriangles);

            if (stitchEnds)
            {
                // create end-caps for mesh
                int[] endTriangles = new int[(radiusSegmentCount - 2) * 6];
                int baseTriIndex = 0;
                int vertexListIndex = 2;
                int halfway = (radiusSegmentCount - 2) * 3;

                // first end
                for (int t = 0; t < halfway; t += 3, vertexListIndex++)
                {
                    endTriangles[t] = baseTriIndex;
                    endTriangles[t + 1] = vertexListIndex;
                    endTriangles[t + 2] = vertexListIndex - 1;
                }

                baseTriIndex = vertexList.Length - radiusSegmentCount;
                vertexListIndex = baseTriIndex + 2;

                // other end (switch tri declaration direction)
                for (int t = halfway; t < endTriangles.Length; t += 3, vertexListIndex++)
                {
                    endTriangles[t] = baseTriIndex;
                    endTriangles[t + 1] = vertexListIndex - 1;
                    endTriangles[t + 2] = vertexListIndex;
                }

                meshTriangles = meshTriangles.Concat(endTriangles);
            }
        }

        mesh.triangles = meshTriangles;
    }

}
