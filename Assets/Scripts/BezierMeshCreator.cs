using System.Collections.Generic;
using UnityEngine;

public class BezierMeshCreator : MonoBehaviour
{
    public static Vector3 start = new Vector3(0, 0, 0);
    public static Vector3 handle1 = new Vector3(0, 1, 0);
    public static Vector3 handle2 = new Vector3(1, 0, 0);
    public static Vector3 end = new Vector3(1, 1, 0);

    public static int resolution = 12;
    public static float thickness = 0.25f;

    public static Mesh CreateMeshForBezier(Info<Vector3, Vector3, Vector3, Vector3> bezier)
    {
        return CreateMeshForBezier(bezier.arg0, bezier.arg1, bezier.arg2, bezier.arg3);
    }

    public static Mesh CreateMeshForBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        start = p0;
        handle1 = p1;
        handle2 = p2;
        end = p3;

        return CreateMesh();
    }

    public static Mesh CreateMesh()
    {
        Mesh mesh;

        mesh = new Mesh();

        float scaling = 1;
        float width = thickness / 2f;
        List<Vector3> vertList = new List<Vector3>();
        List<int> triList = new List<int>();
        List<Vector2> uvList = new List<Vector2>();
        Vector3 upNormal = new Vector3(0, 0, -1);

        triList.AddRange(new int[] {
            2, 1, 0,    //start face
            0, 3, 2
        });

        for (int s = 0; s < resolution; s++)
        {
            float t = ((float)s) / resolution;
            float futureT = ((float)s + 1) / resolution;

            Vector3 segmentStart = PointOnPath(t, start, handle1, handle2, end);
            Vector3 segmentEnd = PointOnPath(futureT, start, handle1, handle2, end);

            Vector3 segmentDirection = segmentEnd - segmentStart;
            if (s == 0 || s == resolution - 1)
                segmentDirection = new Vector3(0, 1, 0);
            segmentDirection.Normalize();
            Vector3 segmentRight = Vector3.Cross(upNormal, segmentDirection);
            segmentRight *= width;
            Vector3 offset = segmentRight.normalized * (width / 2) * scaling;
            Vector3 br = segmentRight + upNormal * width + offset;
            Vector3 tr = segmentRight + upNormal * -width + offset;
            Vector3 bl = -segmentRight + upNormal * width + offset;
            Vector3 tl = -segmentRight + upNormal * -width + offset;

            int curTriIdx = vertList.Count;

            Vector3[] segmentVerts = new Vector3[]
            {
                segmentStart + br,
                segmentStart + bl,
                segmentStart + tl,
                segmentStart + tr,
            };
            vertList.AddRange(segmentVerts);

            Vector2[] uvs = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 1)
            };
            uvList.AddRange(uvs);

            int[] segmentTriangles = new int[]
            {
                curTriIdx + 6, curTriIdx + 5, curTriIdx + 1, //left face
                curTriIdx + 1, curTriIdx + 2, curTriIdx + 6,
                curTriIdx + 7, curTriIdx + 3, curTriIdx + 0, //right face
                curTriIdx + 0, curTriIdx + 4, curTriIdx + 7,
                curTriIdx + 1, curTriIdx + 5, curTriIdx + 4, //top face
                curTriIdx + 4, curTriIdx + 0, curTriIdx + 1,
                curTriIdx + 3, curTriIdx + 7, curTriIdx + 6, //bottom face
                curTriIdx + 6, curTriIdx + 2, curTriIdx + 3
            };
            triList.AddRange(segmentTriangles);

            // final segment fenceposting: finish segment and add end face
            if (s == resolution - 1)
            {
                curTriIdx = vertList.Count;

                vertList.AddRange(new Vector3[] {
                    segmentEnd + br,
                    segmentEnd + bl,
                    segmentEnd + tl,
                    segmentEnd + tr
                });

                uvList.AddRange(new Vector2[] {
                        new Vector2(0, 0),
                        new Vector2(0, 1),
                        new Vector2(1, 1),
                        new Vector2(1, 1)
                    }
                );
                triList.AddRange(new int[] {
                    curTriIdx + 0, curTriIdx + 1, curTriIdx + 2, //end face
                    curTriIdx + 2, curTriIdx + 3, curTriIdx + 0
                });
            }
        }

        mesh.vertices = vertList.ToArray();
        mesh.triangles = triList.ToArray();
        mesh.uv = uvList.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        //mesh.Optimize();

        return mesh;
    }

    //cacluates point coordinates on a quadratic curve
    // TODO: may be replaceable with Utilities bezier method
    public static Vector3 PointOnPath(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u, uu, uuu, tt, ttt;
        Vector3 p;

        u = 1 - t;
        uu = u * u;
        uuu = uu * u;

        tt = t * t;
        ttt = tt * t;

        p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;

        return p;
    }
}
