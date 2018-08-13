using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoverMotor : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private float verticalDrag = 0.01f;
    [SerializeField] private float gravityForce = 10f;

    [Header("Movement")]
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float leanStrength = 0.5f;

    [Header("Turning")]
    [SerializeField] private float turnSpeed = 4f;
    [SerializeField] private float baseAngularDrag = 4f;
    [SerializeField] private float turningAngularDrag = 2f;

    [Header("Boost")]
    [SerializeField] private float boostForceMultiplier = 2f;
    [SerializeField] private float boostTime = 1.5f;
    [SerializeField] private float boostRechargeTime = 3f;
    [SerializeField] private float boostRechargeDelay = 1.5f;

    [Header("Hover")]
    [SerializeField] private float hoverHeight = 20f;
    [SerializeField] private float hoverForce = 30f;
    [SerializeField] private LayerMask raycastMask;
    public List<Vector3> raycastDirections;
    public float rayCastHeightModifier = 2f;
    public float rayCastHorizontalLengthModifier = 2.5f;

    [Header("Gyro")]
    [SerializeField] private float gyroRotationLimit = 0.7f;
    [SerializeField] private float gyroCorrectionStrength = 6f;
    [SerializeField] private float rotationLimit = 0.6f;
    [SerializeField] private float rotationCorrectionStrength = 4f;

    private Rigidbody rb;
    private Vector2 moveInputVector;
    private Vector2 turnInputVector;
    private Vector3 gravityVector;

    private bool boosting;
    private float boostState;

    private bool useBumperForce;
    private float bumperColliderRadius;

    private float[] raycastBaseLengths;

    private bool controllerActive;

    // Cached variables
    private Vector3 position;
    private Vector3 up;
    private Vector3 forward;
    private Vector3 forwardFlat;
    private Vector3 right;
    private Vector3 rightFlat;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityVector = Vector3.down * gravityForce;

        bumperColliderRadius = GetComponent<SphereCollider>().radius;

        UpdateVariables();
        ProcessHoverRays();
    }

    public void ControllerActiveState(bool active)
    {
        controllerActive = active;

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.mass = controllerActive ? 1f : 5f;
        rb.drag = controllerActive ? 0.4f : 2f;
        rb.angularDrag = controllerActive ? 0.1f : 1f;

        if (!controllerActive)
        {
            moveInputVector = Vector2.zero;
            boosting = false;
        }
    }

    /*
     * Normalise hover raycasts.
     */
    void ProcessHoverRays()
    {
        raycastBaseLengths = new float[raycastDirections.Count];

        for (int i = 0; i < raycastDirections.Count; i++)
        {
            raycastDirections[i].Normalize();
            // extend raycast length depending on angle from vertical
            float verticalDot = (Vector3.Dot(Vector3.up, raycastDirections[i]) + 1) * 0.5f;
            float verticalSpread = verticalDot * hoverHeight * rayCastHorizontalLengthModifier;
            raycastBaseLengths[i] = hoverHeight + verticalSpread;
        }
    }

    /*
     * Caches cardinal and flat direction variables for use by logic functions
     * run at the start of each update
     */
    void UpdateVariables()
    {
        // update cached transform variables
        position = transform.position;
        forward = transform.forward;
        up = transform.up;
        right = transform.right;

        // Get movement axes relative to global axes rather than local vertical look direction
        forwardFlat = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, forwardFlat);
        rightFlat = rot * Vector3.right;
    }

    /*
     * Handle movement input
     */
    public void Move(float x, float y)
    {
        moveInputVector = new Vector2(x, y);
        if (moveInputVector.sqrMagnitude > 1f)
            moveInputVector.Normalize();
    }

    /*
     * Handle turning input
     *
     * TODO: Rebuild turning/aiming to add second level of precise aiming.
     * TODO: Use joystick for basic turning, then floating reticle for precise aiming.
     * TODO: reticle controlled by eg: switch gyro.
     * TODO: act roughly like killzone 3 psmove aiming
     */
    public void Turn(float x, float y)
    {
        turnInputVector = new Vector2(x, y);
        //if (turnInputVector.sqrMagnitude > 1f)
        //    turnInputVector.Normalize();
    }

    /*
     * Handle boost input
     */
    public void Boost(bool isBoosting)
    {
        if (boosting && !isBoosting)
            StartCoroutine(RechargeBoost());
        else
            boosting = isBoosting && boostState < boostTime;
    }

    void FixedUpdate()
    {
        // Update cardinal/flat direction variables etc
        UpdateVariables();

        // Apply Gravity
        rb.AddForce(gravityVector * Time.fixedDeltaTime, ForceMode.VelocityChange);

        // Movement
        ApplyMovementForce();

        // Turning
        if (!controllerActive)
        {
            float angle = Vector3.Angle(forward, forwardFlat);
            if (angle > 2f)
            {
                float dot = Vector3.Dot(forward, Vector3.down);
                turnInputVector = new Vector2(0f, dot * angle * 0.2f);
            }
            else
            {
                turnInputVector = Vector2.zero;
            }
        }
        ApplyTurningForce();
        DampenTurning();

        // Hover force, bumper forces and gyro correction
        ApplyHoverForce();
        ApplyBumperForce();
        GyroCorrection();

        // reset variables
        useBumperForce = false;
    }

    /*
     * Apply movement force based on input
     */
    void ApplyMovementForce()
    {
        if (moveInputVector == Vector2.zero)
            return;

        // calculate strafe velocity dampening
        Vector3 vel = rb.velocity;
        Vector3 rightVel = Vector3.Project(vel, right);
        Vector3 alignmentForce = (forward * rightVel.magnitude * 2f - rightVel * 5f) * Time.fixedDeltaTime;

        // Apply force to move tank
        if (boosting)
        {
            // Apply boosting movement force
            // Uses actual forward/right vectors rather than flat vectors to allow more directional control 
            Vector3 boostForce = (forward * moveInputVector.y) + (rightFlat * moveInputVector.x) + alignmentForce;
            rb.AddForce(boostForce * moveForce * boostForceMultiplier * Time.fixedDeltaTime, ForceMode.Impulse);

            // track boost state and begin recharge if limit is hit
            boostState += Time.fixedDeltaTime;
            if (boostState >= boostTime)
            {
                boostState = boostTime;
                StartCoroutine(RechargeBoost());
            }
        }
        else
        {
            // Not boosting: apply default movement force
            // TODO: look at benefits for using direct vs. flattened directions
            Vector3 movementForce = (forward * moveInputVector.y) + (rightFlat * moveInputVector.x) + alignmentForce;
            rb.AddForce(movementForce * moveForce * Time.fixedDeltaTime, ForceMode.Impulse);
        }

        // lean slightly to match left/right movement
        // TODO: detach visuals slightly from camera, allow leaning forward/back as well
        Vector3 leanTorque = forward * -moveInputVector.x * leanStrength;
        rb.AddTorque(leanTorque * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    /*
     * Coroutine to recharge boost
     *
     * Stops when fully recharged or boost input is activated again
     */
    private IEnumerator RechargeBoost()
    {
        boosting = false;

        // wait for the recharge delay
        yield return new WaitForSeconds(boostRechargeDelay);

        // calculate recharge rate based on time
        float rechargeRate = boostTime / boostRechargeTime;

        // Recharge boost
        while (boostState > 0f && !boosting)
        {
            boostState -= Time.deltaTime * rechargeRate;
            yield return 0;
        }

        if (!boosting)
            boostState = 0f;
    }

    /*
     * Apply turning torque based on input
     */
    void ApplyTurningForce()
    {
        if (turnInputVector == Vector2.zero)
            return;

        // Apply torque to rotate facing direction
        Vector3 torque = (up * turnInputVector.x * turnSpeed) + (right * -turnInputVector.y * turnSpeed);

        rb.AddTorque(torque * Time.fixedDeltaTime, ForceMode.Impulse);

    }

    /*
     * Switch angular drag value based on whether receiving turn input
     * (no input = higher drag for more accurate stopping)
     */
    void DampenTurning()
    {
        // Apply angular drag value based on input state
        rb.angularDrag = (turnInputVector == Vector2.zero) ? baseAngularDrag : turningAngularDrag;
    }

    /*
     * Perform raycasts underneath and apply hover force based on the closest hit
     *
     * TODO: Simpify calculations when controller not active
     */
    void ApplyHoverForce()
    {
        // raycast and apply hover force
        float maxHoverForce = 0f;
        Vector3 origin = position + (Vector3.up * rayCastHeightModifier);
        int raysToCheck = controllerActive ? raycastDirections.Count : 1;
        for (int i = 0; i < raysToCheck; i++)
        {
            RaycastHit hitInfo;
            float rayLength = CalculateHoverRayLengthFromIndex(i);
            if (Physics.Raycast(origin, raycastDirections[i], out hitInfo, rayLength, raycastMask))
            {
                if (hitInfo.distance > rayCastHeightModifier) // this check to make sure raycasts ending above player collider dont trigger hover
                {
                    float force = (1 - (hitInfo.distance - rayCastHeightModifier) / (rayLength - rayCastHeightModifier)) * hoverForce;
                    if (force > maxHoverForce)
                        maxHoverForce = force;
                }
            }
        }

        if (maxHoverForce > 0f)
        {
            if (!controllerActive)
                maxHoverForce *= 4f;
            rb.AddForce(Vector3.up * maxHoverForce * Time.fixedDeltaTime, ForceMode.Impulse);
        }

        // apply vertical momentum drag
        Vector3 velocity = rb.velocity;
        velocity.y *= 1f - verticalDrag;
        rb.velocity = velocity;
    }

    /*
     * Calculates the length of a given hover raycast based on angles to vertical axis and velocity direction
     */
    public float CalculateHoverRayLengthFromIndex(int rayIndex)
    {
        if (rayIndex < 0 || rayIndex >= raycastDirections.Count || rb == null)
            return 1f;

        if (raycastBaseLengths == null)
            ProcessHoverRays();

        Vector3 ray = raycastDirections[rayIndex];

        // extend raycast length depending on angle to movement direction
        float movementSpread = 0f;
        Vector3 movementVector = rb.velocity;
        if (movementVector != Vector3.zero && Vector3.Angle(movementVector, ray) < 45f)
        {
            float movementDot = Vector3.Dot(movementVector * 0.01f, ray);
            if (movementDot > 0f)
                movementSpread = movementDot * hoverHeight * rayCastHorizontalLengthModifier;
        }

        return raycastBaseLengths[rayIndex] + movementSpread;
    }

    /*
     * Shunts tank away from close surfaces in cardinal directions
     */
    void ApplyBumperForce()
    {
        if (!useBumperForce || !controllerActive)
            return;

        // if trigger collider is colliding
        List<Vector3> rays = new List<Vector3>()
        {
            forward,
            -forward,
            right,
            -right,
            up,
            -up,
            Vector3.down,
            rb.velocity.normalized + Vector3.down
        };

        Vector3 bumperForce = Vector3.zero;
        // raycast in relative cardinal directions and average force
        foreach (Vector3 ray in rays)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(position, ray, out hitInfo, bumperColliderRadius, raycastMask))
            {
                float force = (1 - hitInfo.distance / bumperColliderRadius) * hoverForce;
                bumperForce -= ray * force * 2f;
            }
        }
        if (bumperForce != Vector3.zero)
            rb.AddForce(bumperForce * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    /*
     * Apply torque to level tank
     */
    void GyroCorrection()
    {
        // check against vertical rotation limit
        float verticalDot = Vector3.Dot(forward, Vector3.up);
        if (Mathf.Abs(verticalDot) > rotationLimit)
        {
            // Vertical rotation correction
            // map correction strength to 0-1
            float correctionStrength = Utilities.MapValues(Mathf.Abs(verticalDot), rotationLimit, 1f, 0f, 1f);
            if (verticalDot < 0f)
                correctionStrength *= -1;

            Vector3 correctionTorque = Vector3.Cross(Vector3.up, forward).normalized * rotationCorrectionStrength *
                                       correctionStrength;

            rb.AddTorque(correctionTorque * Time.fixedDeltaTime, ForceMode.Impulse);
        }

        // Correct gyro
        if (Mathf.Abs(verticalDot) < gyroRotationLimit)
        {
            // rotate around transform.forward until transform.up is closest to V3.up
            Vector3 projectionPlaneNormal = Vector3.Cross(Vector3.up, forward);

            float gyroDot = Vector3.Dot(up, projectionPlaneNormal);

            if (up.y < 0f)
                gyroDot = gyroDot < 0f ? -1f : 1f;

            Vector3 gyroTorque = forward * Time.fixedDeltaTime * (gyroDot * gyroCorrectionStrength);
            rb.AddTorque(gyroTorque, ForceMode.Impulse);
        }
    }

    /*
     * Trigger collider used to trigger bumper raycasts
     */
    void OnTriggerStay(Collider col)
    {
        if (!col.isTrigger)
        {
            useBumperForce = true;
        }
    }
}