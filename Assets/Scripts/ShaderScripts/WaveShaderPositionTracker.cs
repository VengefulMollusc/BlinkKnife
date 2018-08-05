using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveShaderPositionTracker : MonoBehaviour
{
    public Material waveMaterial;
    public Transform targetObjectTransform;
    public float heightOffset;

    private Vector3 trackedPosition;
    private MeshFilter[] childMeshes;

    // Wave settings
    private float seaDepth;
    private float speedGravity;
    private Vector4 waveA;
    private Vector4 waveB;
    private Vector4 waveC;
    private float timeFactor;

    // Displacement settings
    private float dispRange;
    private float flatRangeExt;
    private float dispMaxDepth;

    private void Start()
    {
        seaDepth = waveMaterial.GetFloat("_SeaDepth");
        speedGravity = waveMaterial.GetFloat("_SpeedGravity");
        waveA = waveMaterial.GetVector("_WaveA");
        waveB = waveMaterial.GetVector("_WaveB");
        waveC = waveMaterial.GetVector("_WaveC");
        dispRange = waveMaterial.GetFloat("_DispRange");
        flatRangeExt = waveMaterial.GetFloat("_FlatRangeExt");
        dispMaxDepth = waveMaterial.GetFloat("_DispMaxDepth");

        trackedPosition = targetObjectTransform.position;

        // Set bounds to avoid early frustum culling
        float halfDepth = seaDepth * 0.5f;
        childMeshes = GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter filter in childMeshes)
        {
            Mesh mesh = filter.mesh;
            Bounds bounds = mesh.bounds;
            Vector3 bCenter = bounds.center;
            Vector3 bSize = bounds.size;

            // TODO: extend size to account for wave height etc.
            Vector3 boundsCenter = new Vector3(bCenter.x, bCenter.y - halfDepth, bCenter.z);
            Vector3 boundsSize = new Vector3(bSize.x, seaDepth, bSize.z);
            mesh.bounds = new Bounds(boundsCenter, boundsSize);
        }
    }

    void Update()
    {
        Vector3 newPosition = targetObjectTransform.position;

        timeFactor = Time.timeSinceLevelLoad;
        waveMaterial.SetFloat("unityTime", timeFactor);

        // accelerate movement of y position
        float newY = trackedPosition.y + (newPosition.y - trackedPosition.y) * Time.deltaTime * 10f;
        trackedPosition = new Vector3(trackedPosition.x, newY, trackedPosition.z);

        // smooth movement
        float distanceMoved = Vector3.SqrMagnitude(newPosition - trackedPosition);
        trackedPosition = Vector3.MoveTowards(trackedPosition, newPosition, distanceMoved * Time.deltaTime);

        waveMaterial.SetVector("_PlayerPosition", new Vector4(trackedPosition.x, trackedPosition.y + heightOffset, trackedPosition.z, 0));
    }
    
    /*
     * Performs shader calculation and returns wave position and normal given a world pos
     */
    public WavePositionInfo CalculateDepthAndNormalAtPoint(Vector3 pos)
    {
        Vector3 tangent = new Vector3(1, 0, 0);
        Vector3 binormal = new Vector3(0, 0, 1);

        float pointDepth = 1;
        
        pos = new Vector3(pos.x, transform.position.y, pos.z); // flatten to base of sea

        Vector3 wavePosition = pos;
        wavePosition.y -= Displacement(pos, ref tangent, ref binormal, ref pointDepth);
        wavePosition += GerstnerWave(waveA, pos, ref tangent, ref binormal, pointDepth);
        wavePosition += GerstnerWave(waveB, pos, ref tangent, ref binormal, pointDepth);
        wavePosition += GerstnerWave(waveC, pos, ref tangent, ref binormal, pointDepth);
        Vector3 waveNormal = Vector3.Cross(binormal, tangent);

        return new WavePositionInfo(wavePosition, waveNormal, wavePosition - pos);
    }

    private Vector3 GerstnerWave(Vector4 wave, Vector3 p, ref Vector3 tangent, ref Vector3 binormal, float pointDepth)
    {
        float waveStrength = 0.1f + 0.9f * pointDepth; // alter this to change strength of pointDepth affecting wave
        float steepness = wave.z * waveStrength;
        float wavelength = wave.w;
        float k = 2 * Mathf.PI / wavelength;
        float c = Mathf.Sqrt(speedGravity / k);
        Vector2 d = new Vector2(wave.x, wave.y).normalized;
        float f = k * (Vector2.Dot(d, new Vector2(p.x, p.z)) - c * timeFactor);
        float a = steepness / k;

        float sinF = Mathf.Sin(f);
        float cosF = Mathf.Cos(f);
        float steepSinF = steepness * sinF;
        float steepCosF = steepness * cosF;

        tangent += new Vector3(
            -d.x * d.x * steepSinF,
            d.x * steepCosF,
            -d.x * d.y * steepSinF
        );
        binormal += new Vector3(
            -d.x * d.y * steepSinF,
            d.y * steepCosF,
            -d.y * d.y * steepSinF
        );
        return new Vector3(
            d.x * (a * cosF),
            a * sinF,
            d.y * (a * cosF)
        );
    }

    private float Displacement(Vector3 worldPoint, ref Vector3 tangent, ref Vector3 binormal, ref float pointDepth)
    {
        Vector3 toPlayer = trackedPosition - worldPoint;

        // decimals here need to add to 1
        float depthFactor = Mathf.Clamp01((toPlayer.y - worldPoint.y) / (seaDepth * (dispMaxDepth - 1)));

        if (depthFactor > 0)
        {
            float playerDistXZ = Mathf.Sqrt(toPlayer.x * toPlayer.x + toPlayer.z * toPlayer.z);
            if (playerDistXZ < (dispRange + flatRangeExt))
            {

                float distFactor = Mathf.Clamp01(1 - (playerDistXZ - flatRangeExt) / dispRange);

                pointDepth = 1 - depthFactor * distFactor;

                pointDepth = Mathf.SmoothStep(0f, 1, pointDepth);

                // Adjust normals etc to account for displacement slope
                if (pointDepth < 1 && playerDistXZ > flatRangeExt)
                {
                    float slope = (seaDepth * depthFactor) * 0.4f / dispRange;
                    Vector2 d = new Vector2(-toPlayer.x, -toPlayer.z).normalized;
                    tangent += new Vector3(
                        -d.x * d.x * slope,
                        d.x * slope,
                        -d.x * d.y * slope
                    );
                    binormal += new Vector3(
                        -d.x * d.y * slope,
                        d.y * slope,
                        -d.y * d.y * slope
                    );
                }
            }
        }

        return seaDepth * (1 - pointDepth);
    }
}

public class WavePositionInfo
{
    public Vector3 position;
    public Vector3 normal;
    public Vector3 movement;

    public WavePositionInfo(Vector3 pos, Vector3 nor, Vector3 mov)
    {
        position = pos;
        normal = nor;
        movement = mov;
    }
}
