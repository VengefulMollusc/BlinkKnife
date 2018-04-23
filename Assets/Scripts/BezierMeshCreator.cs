using System.Collections.Generic;
using UnityEngine;

public class BezierMeshCreator : MonoBehaviour
{
    private static Info<Vector3, Vector3, Vector3, Vector3> bezier;
    private static float thickness = 1f;
    private static int segmentCount = 10;

    public static Mesh CreateMeshForBezier(Info<Vector3, Vector3, Vector3, Vector3> _bezier)
    {
        bezier = _bezier;

        Mesh mesh = new Mesh();

        // Do the thing

        return mesh;
    }

    private static void GenerateFibreMesh(Mesh mesh)
    {
        
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    //private static Vector3 point0, point1, point2, point3;

    ////void Start()
    ////{
    ////    mf = GetComponent<MeshFilter>();

    ////    GenerateMesh();
    ////}

    //public static Mesh CreateMeshForBezier(Info<Vector3, Vector3, Vector3, Vector3> bezier)
    //{
    //    return CreateMeshForBezier(bezier.arg0, bezier.arg1, bezier.arg2, bezier.arg3);
    //}

    //public static Mesh CreateMeshForBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    //{
    //    point0 = p0;
    //    point1 = p1;
    //    point2 = p2;
    //    point3 = p3;

    //    return GenerateMesh();
    //}

    //private static Mesh GenerateMesh()
    //{
    //    var mesh = GetMesh();
    //    var shape = GetExtrudeShape();
    //    var path = GetPath();

    //    Extrude(mesh, shape, path);

    //    return mesh;
    //}


    //private static ExtrudeShape GetExtrudeShape()
    //{
    //    var vert2Ds = new Vertex[] {
    //        new Vertex(
    //            new Vector3(0, 0, 0),
    //            new Vector3(0, 1, 0),
    //            0),
    //        new Vertex(
    //            new Vector3(2, 0, 0),
    //            new Vector3(0, 1, 0),
    //            0),
    //        new Vertex(
    //            new Vector3(2, 0, 0),
    //            new Vector3(0, 1, 0),
    //            0),
    //        new Vertex(
    //            new Vector3(4, 0, 0),
    //            new Vector3(0, 1, 0),
    //            0)
    //    };

    //    var lines = new int[] {
    //        0, 1,
    //        1, 2,
    //        2, 3
    //    };

    //    return new ExtrudeShape(vert2Ds, lines);
    //}


    //private static OrientedPoint[] GetPath()
    //{
    //    /*return new OrientedPoint[] {
    //        new OrientedPoint(
    //            new Vector3(0, 0, 0),
    //            Quaternion.identity),
    //        new OrientedPoint(
    //            new Vector3(0, 1, 1),
    //            Quaternion.identity),
    //        new OrientedPoint(
    //            new Vector3(0, 0, 2),
    //            Quaternion.identity)
    //    };*/

    //    var p = new Vector3[] {
    //        point0,
    //        point1,
    //        point2,
    //        point3
    //    };

    //    var path = new List<OrientedPoint>();

    //    for (float t = 0; t <= 1; t += 0.1f)
    //    {
    //        var point = GetPoint(p, t);
    //        var rotation = GetOrientation3D(p, t, Vector3.up);
    //        path.Add(new OrientedPoint(point, rotation));
    //    }

    //    return path.ToArray();
    //}


    //private static Mesh GetMesh()
    //{
    //    //if (mf.sharedMesh == null)
    //    //{
    //    //    mf.sharedMesh = new Mesh();
    //    //}
    //    //return mf.sharedMesh;
    //    return new Mesh();
    //}


    //private static Vector3 GetPoint(Vector3[] p, float t)
    //{
    //    float omt = 1f - t;
    //    float omt2 = omt * omt;
    //    float t2 = t * t;
    //    return
    //        p[0] * (omt2 * omt) +
    //        p[1] * (3f * omt2 * t) +
    //        p[2] * (3f * omt * t2) +
    //        p[3] * (t2 * t);
    //}


    //private static Vector3 GetTangent(Vector3[] p, float t)
    //{
    //    float omt = 1f - t;
    //    float omt2 = omt * omt;
    //    float t2 = t * t;
    //    Vector3 tangent =
    //        p[0] * (-omt2) +
    //        p[1] * (3 * omt2 - 2 * omt) +
    //        p[2] * (-3 * t2 + 2 * t) +
    //        p[3] * (t2);
    //    return tangent.normalized;
    //}


    //private static Vector3 GetNormal3D(Vector3[] p, float t, Vector3 up)
    //{
    //    var tng = GetTangent(p, t);
    //    var binormal = Vector3.Cross(up, tng).normalized;
    //    return Vector3.Cross(tng, binormal);
    //}


    //private static Quaternion GetOrientation3D(Vector3[] p, float t, Vector3 up)
    //{
    //    var tng = GetTangent(p, t);
    //    var nrm = GetNormal3D(p, t, up);
    //    return Quaternion.LookRotation(tng, nrm);
    //}


    //private static void Extrude(Mesh mesh, ExtrudeShape shape, OrientedPoint[] path)
    //{
    //    int vertsInShape = shape.vert2Ds.Length;
    //    int segments = path.Length - 1;
    //    int edgeLoops = path.Length;
    //    int vertCount = vertsInShape * edgeLoops;
    //    int triCount = shape.lines.Length * segments;
    //    int triIndexCount = triCount * 3;

    //    var triangleIndices = new int[triIndexCount];
    //    var vertices = new Vector3[vertCount];
    //    var normals = new Vector3[vertCount];
    //    var uvs = new Vector2[vertCount];

    //    float totalLength = 0;
    //    float distanceCovered = 0;
    //    for (int i = 0; i < path.Length - 1; i++)
    //    {
    //        var d = Vector3.Distance(path[i].position, path[i + 1].position);
    //        totalLength += d;
    //    }

    //    for (int i = 0; i < path.Length; i++)
    //    {
    //        int offset = i * vertsInShape;
    //        if (i > 0)
    //        {
    //            var d = Vector3.Distance(path[i].position, path[i - 1].position);
    //            distanceCovered += d;
    //        }
    //        float v = distanceCovered / totalLength;

    //        for (int j = 0; j < vertsInShape; j++)
    //        {
    //            int id = offset + j;
    //            vertices[id] = path[i].LocalToWorld(shape.vert2Ds[j].point);
    //            normals[id] = path[i].LocalToWorldDirection(shape.vert2Ds[j].normal);
    //            uvs[id] = new Vector2(shape.vert2Ds[j].uCoord, v);
    //        }
    //    }
    //    int ti = 0;
    //    for (int i = 0; i < segments; i++)
    //    {
    //        int offset = i * vertsInShape;
    //        for (int l = 0; l < shape.lines.Length; l += 2)
    //        {
    //            int a = offset + shape.lines[l] + vertsInShape;
    //            int b = offset + shape.lines[l];
    //            int c = offset + shape.lines[l + 1];
    //            int d = offset + shape.lines[l + 1] + vertsInShape;
    //            triangleIndices[ti] = c; ti++;
    //            triangleIndices[ti] = b; ti++;
    //            triangleIndices[ti] = a; ti++;
    //            triangleIndices[ti] = a; ti++;
    //            triangleIndices[ti] = d; ti++;
    //            triangleIndices[ti] = c; ti++;
    //        }
    //    }


    //    mesh.Clear();
    //    mesh.vertices = vertices;
    //    mesh.normals = normals;
    //    mesh.uv = uvs;
    //    mesh.triangles = triangleIndices;
    //}


    //public struct ExtrudeShape
    //{
    //    public Vertex[] vert2Ds;
    //    public int[] lines;

    //    public ExtrudeShape(Vertex[] vert2Ds, int[] lines)
    //    {
    //        this.vert2Ds = vert2Ds;
    //        this.lines = lines;
    //    }
    //}


    //public struct Vertex
    //{
    //    public Vector3 point;
    //    public Vector3 normal;
    //    public float uCoord;


    //    public Vertex(Vector3 point, Vector3 normal, float uCoord)
    //    {
    //        this.point = point;
    //        this.normal = normal;
    //        this.uCoord = uCoord;
    //    }
    //}


    //public struct OrientedPoint
    //{
    //    public Vector3 position;
    //    public Quaternion rotation;


    //    public OrientedPoint(Vector3 position, Quaternion rotation)
    //    {
    //        this.position = position;
    //        this.rotation = rotation;
    //    }


    //    public Vector3 LocalToWorld(Vector3 point)
    //    {
    //        return position + rotation * point;
    //    }


    //    public Vector3 WorldToLocal(Vector3 point)
    //    {
    //        return Quaternion.Inverse(rotation) * (point - position);
    //    }


    //    public Vector3 LocalToWorldDirection(Vector3 dir)
    //    {
    //        return rotation * dir;
    //    }
    //}

    //////////////////////////////////////////////////////////////////////////////////////////////////////////

    //public static Vector3 start = new Vector3(0, 0, 0);
    //public static Vector3 handle1 = new Vector3(0, 1, 0);
    //public static Vector3 handle2 = new Vector3(1, 0, 0);
    //public static Vector3 end = new Vector3(1, 1, 0);

    //public static int resolution = 12;
    //public static float thickness = 1f;

    //public static Mesh CreateMeshForBezier(Info<Vector3, Vector3, Vector3, Vector3> bezier)
    //{
    //    return CreateMeshForBezier(bezier.arg0, bezier.arg1, bezier.arg2, bezier.arg3);
    //}

    //public static Mesh CreateMeshForBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    //{
    //    start = p0;
    //    handle1 = p1;
    //    handle2 = p2;
    //    end = p3;

    //    return CreateMesh();
    //}

    //private static Mesh CreateMesh()
    //{
    //    Mesh mesh;

    //    mesh = new Mesh();

    //    float scaling = 1;
    //    float width = thickness / 2f;
    //    List<Vector3> vertList = new List<Vector3>();
    //    List<int> triList = new List<int>();
    //    List<Vector2> uvList = new List<Vector2>();
    //    Vector3 upNormal = new Vector3(0, 0, -1);

    //    triList.AddRange(new int[] {
    //        2, 1, 0,    //start face
    //        0, 3, 2
    //    });

    //    for (int s = 0; s < resolution; s++)
    //    {
    //        float t = ((float)s) / resolution;
    //        float futureT = ((float)s + 1) / resolution;

    //        Vector3 segmentStart = PointOnPath(t, start, handle1, handle2, end);
    //        Vector3 segmentEnd = PointOnPath(futureT, start, handle1, handle2, end);

    //        Vector3 segmentDirection = segmentEnd - segmentStart;
    //        if (s == 0 || s == resolution - 1)
    //            segmentDirection = new Vector3(0, 1, 0);
    //        segmentDirection.Normalize();
    //        Vector3 segmentRight = Vector3.Cross(upNormal, segmentDirection);
    //        segmentRight *= width;
    //        Vector3 offset = segmentRight.normalized * (width / 2) * scaling;
    //        Vector3 br = segmentRight + upNormal * width + offset;
    //        Vector3 tr = segmentRight + upNormal * -width + offset;
    //        Vector3 bl = -segmentRight + upNormal * width + offset;
    //        Vector3 tl = -segmentRight + upNormal * -width + offset;

    //        int curTriIdx = vertList.Count;

    //        Vector3[] segmentVerts = new Vector3[]
    //        {
    //            segmentStart + br,
    //            segmentStart + bl,
    //            segmentStart + tl,
    //            segmentStart + tr,
    //        };
    //        vertList.AddRange(segmentVerts);

    //        Vector2[] uvs = new Vector2[]
    //        {
    //            new Vector2(0, 0),
    //            new Vector2(0, 1),
    //            new Vector2(1, 1),
    //            new Vector2(1, 1)
    //        };
    //        uvList.AddRange(uvs);

    //        int[] segmentTriangles = new int[]
    //        {
    //            curTriIdx + 6, curTriIdx + 5, curTriIdx + 1, //left face
    //            curTriIdx + 1, curTriIdx + 2, curTriIdx + 6,
    //            curTriIdx + 7, curTriIdx + 3, curTriIdx + 0, //right face
    //            curTriIdx + 0, curTriIdx + 4, curTriIdx + 7,
    //            curTriIdx + 1, curTriIdx + 5, curTriIdx + 4, //top face
    //            curTriIdx + 4, curTriIdx + 0, curTriIdx + 1,
    //            curTriIdx + 3, curTriIdx + 7, curTriIdx + 6, //bottom face
    //            curTriIdx + 6, curTriIdx + 2, curTriIdx + 3
    //        };
    //        triList.AddRange(segmentTriangles);

    //        // final segment fenceposting: finish segment and add end face
    //        if (s == resolution - 1)
    //        {
    //            curTriIdx = vertList.Count;

    //            vertList.AddRange(new Vector3[] {
    //                segmentEnd + br,
    //                segmentEnd + bl,
    //                segmentEnd + tl,
    //                segmentEnd + tr
    //            });

    //            uvList.AddRange(new Vector2[] {
    //                    new Vector2(0, 0),
    //                    new Vector2(0, 1),
    //                    new Vector2(1, 1),
    //                    new Vector2(1, 1)
    //                }
    //            );
    //            triList.AddRange(new int[] {
    //                curTriIdx + 0, curTriIdx + 1, curTriIdx + 2, //end face
    //                curTriIdx + 2, curTriIdx + 3, curTriIdx + 0
    //            });
    //        }
    //    }

    //    mesh.vertices = vertList.ToArray();
    //    mesh.triangles = triList.ToArray();
    //    mesh.uv = uvList.ToArray();
    //    mesh.RecalculateNormals();
    //    mesh.RecalculateBounds();
    //    //mesh.Optimize();

    //    return mesh;
    //}

    ////cacluates point coordinates on a quadratic curve
    //// TODO: may be replaceable with Utilities bezier method
    //public static Vector3 PointOnPath(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    //{
    //    float u, uu, uuu, tt, ttt;
    //    Vector3 p;

    //    u = 1 - t;
    //    uu = u * u;
    //    uuu = uu * u;

    //    tt = t * t;
    //    ttt = tt * t;

    //    p = uuu * p0;
    //    p += 3 * uu * t * p1;
    //    p += 3 * u * tt * p2;
    //    p += ttt * p3;

    //    return p;
    //}
}
