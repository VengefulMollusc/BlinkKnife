using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverBikeMotor : VehicleMotor
{
    [Header("Hover")]
    public LayerMask hoverCastMask;
    public int waveShaderLayer;
    public float hoverheight;
    public float hoverForce;

    [Header("Movement")]
    public float speed;
    public float strafeSpeed;
    public float boostFactor;

    [Header("Turning")]
    public float turnSpeed;
    public float maxPitchAngle;

    private Rigidbody rb;
    private WaveShaderPositionTracker wavePositionTracker;

    private Vector3 position;
    private Vector3 forward;
    private Vector3 right;

    private Vector2 movementInput;
    private bool boosting;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        position = transform.position;
        forward = transform.forward;
        right = transform.right;

        Movement();

        Turning();

        Hover();
    }

    private void Movement()
    {
        Vector3 forwardMovement = forward * movementInput.y;
        if (movementInput.y > 0)
        {
            forwardMovement *= boosting ? speed * boostFactor : speed;
        }
        else
        {
            forwardMovement *= strafeSpeed;
        }

        Vector3 strafeMovement = right * movementInput.x * strafeSpeed;

        Vector3 movementForce = forwardMovement + strafeMovement;

        rb.AddForce(movementForce * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    private void Turning()
    {
        Vector3 cameraFacing = cameraTransform.forward;
        float yRotDiff = Vector3.Magnitude(new Vector3(cameraFacing.x, 0f, cameraFacing.z) -
                                     new Vector3(forward.x, 0f, forward.y));
        //float xRotDiff = Vector2.Angle(new Vector2(cameraFacing.y, cameraFacing.z), new Vector2(forward.y, forward.z));

        Vector3 torque = new Vector3(0f, yRotDiff, 0f);

        rb.AddTorque(torque * turnSpeed * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    private void Hover()
    {
        RaycastHit[] hits = Physics.RaycastAll(position, Vector3.down, hoverheight, hoverCastMask, QueryTriggerInteraction.Collide);

        if (hits.Length <= 0)
            return;

        float closestHoverSurface = hoverheight;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.isTrigger)
            {
                if (hit.collider.gameObject.layer == waveShaderLayer)
                {
                    WavePositionInfo wavePositionInfo = wavePositionTracker.CalculateDepthAndNormalAtPoint(position);
                    float dist = (position - wavePositionInfo.position).magnitude;
                    if (dist < closestHoverSurface)
                        closestHoverSurface = dist;
                }
            }
            else
            {
                if (hit.distance < closestHoverSurface)
                    closestHoverSurface = hit.distance;
            }
        }

        float hoverForceFactor = 1 - closestHoverSurface / hoverheight;

        rb.AddForce(Vector3.up * hoverForceFactor * hoverForce * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    public override void MovementInput(Vector2 input, bool boosting)
    {
        movementInput = input;
        this.boosting = boosting;
    }
}
