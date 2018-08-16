using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWaveCollisionHandler : WaveCollisionHandler
{
    [Header("Physics interactions")]
    public float footDepth;
    public float velocityDampenStrength;

    private Rigidbody rb;
    private JumpCollider jumpCollider;
    private float footDist;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        jumpCollider = GetComponentInChildren<JumpCollider>();
        footDist = GetComponent<MeshFilter>().mesh.bounds.size.y;
    }

    public override void CollideWithWave(WavePositionInfo waveInfo)
    {
        Vector3 position = rb.position;
        Vector3 colFootPosition = position + Vector3.down * footDist;

        float waveOverlap = waveInfo.position.y - colFootPosition.y;
        if (waveOverlap < 0f)
            return;

        jumpCollider.Grounded(true);

        // inside wave volume
        if (waveOverlap > footDepth)
        {
            // move rigidbody to align with surface
            rb.MovePosition(position + Vector3.up * (waveOverlap - footDepth));
        }

        Vector3 velocity = rb.velocity;
        if (Vector3.Dot(velocity.normalized, waveInfo.normal) < 0f)
        {
            Vector3 velocityAlongWaveNormal = Vector3.Project(rb.velocity, waveInfo.normal);
            rb.velocity -= velocityAlongWaveNormal;
        }

        // Dampen velocity while in contact
        if (velocityDampenStrength > 0f)
            rb.velocity *= 1 - (Time.fixedDeltaTime * velocityDampenStrength);
    }
}
