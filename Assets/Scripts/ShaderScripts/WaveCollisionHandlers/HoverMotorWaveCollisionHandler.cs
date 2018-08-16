using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverMotorWaveCollisionHandler : WaveCollisionHandler
{
    private HoverMotor hoverMotor;
    private Rigidbody rb;
    private float hoverHeight = 15f;
    private float hoverForce = 30f;

    void Start()
    {
        hoverMotor = GetComponent<HoverMotor>();
        rb = GetComponent<Rigidbody>();
    }

    public override void CollideWithWave(WavePositionInfo waveInfo)
    {
        float yDist = transform.position.y - waveInfo.position.y;

        float hoverStrength = Mathf.Clamp01(1 - (yDist / hoverHeight)) * hoverForce;

        if (hoverStrength > 0)
            rb.AddForce(Vector3.up * hoverStrength * Time.fixedDeltaTime, ForceMode.Impulse);
    }
}
