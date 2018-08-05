using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveShaderRider : MonoBehaviour
{
    public WaveShaderPositionTracker wave;
    [Range(0, 1)] public float waveMovementStrength = 1f;
    [Range(0, 1)] public float tiltStrength = 1f;

    private Transform childTransform;

    void Start()
    {
        if (transform.childCount < 1)
        {
            Debug.LogError("No child object");
        }

        childTransform = transform.GetChild(0);
    }

    void Update()
    {
        WavePositionInfo info = wave.CalculateDepthAndNormalAtPoint(transform.position);

        // match child to wave position, taking into account movementStrength for horizontal axes
        childTransform.position = new Vector3(info.position.x * waveMovementStrength, info.position.y, info.position.z * waveMovementStrength);
        
        // apply wave normal tilt to child transform
        if (tiltStrength > 0f)
        {
            Vector3 tilt = new Vector3(info.normal.x * tiltStrength, info.normal.y, info.normal.z * tiltStrength).normalized;
            childTransform.rotation = Quaternion.FromToRotation(transform.up, tilt);
        }
    }
}
