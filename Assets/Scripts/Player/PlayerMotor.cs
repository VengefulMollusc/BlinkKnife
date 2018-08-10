using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{

    [SerializeField]
    private Camera cam;
    
    private const float cameraRotLimit = 90f;

    private const float velMod = 1.5f;
    private const float airVelMod = 1.2f;

    // Shift gravity if the difference between current gravity and surface normal is
    // above the threshold defined by warpGravShiftAngle
    [SerializeField]
    private const float warpGravShiftAngle = 1f;

    private const float gravShiftTimeLimit = 2f;

    private Coroutine gravShiftTimerCoroutine;

    [SerializeField]
    [Range(0.0f, 50.0f)]
    private const float warpVelocityModifier = 20f;

    public const string WarpNotification = "PlayerMotor.WarpNotification";
    public const string GravityWarpNotification = "PlayerMotor.GravityWarpNotification";

    private bool frozen;
    private Vector3 frozenVel = Vector3.zero;

    private bool sliding;
    private bool colliding;
    private Vector3 slideSurfaceVector;

    private bool crouching;
    private const float crouchVelFactor = 0.5f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private bool sprinting;
    private float cameraRotationX;
    private float currentCamRotX;

    private float jumpTimer;
    private const float jumpTimerDefault = 0.2f;

    private Rigidbody rb;

    private Vector3 cameraRelativePos;

    private Vector3 currentGravVector;
    private float groundSpeedThreshold;
    private float airSpeedThreshold;

    private Vector3 slopeNormal;
    private float slopeAngle;

    private bool vaulting;

    private TransitionCameraController transCamController;

    private PlayerKnifeController playerKnifeController;
    private Renderer playerRenderer;

    private UtiliseGravity utiliseGravity;

    void OnEnable()
    {
        // Initial check of gravity vector
        OnGravityChange(null, null);

        this.AddObserver(OnGravityChange, GlobalGravityControl.GravityChangeNotification);
        this.AddObserver(OnBoostNotification, BoostRing.BoostNotification);
        this.AddObserver(EndWarp, TransitionCameraController.WarpEndNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnGravityChange, GlobalGravityControl.GravityChangeNotification);
        this.RemoveObserver(OnBoostNotification, BoostRing.BoostNotification);
        this.RemoveObserver(EndWarp, TransitionCameraController.WarpEndNotification);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        playerKnifeController = cam.GetComponent<PlayerKnifeController>();
        utiliseGravity = GetComponent<UtiliseGravity>();

        // Find the TransitionCamera object in the scene
        transCamController = GameObject.Find("TransitionCamera").GetComponent<TransitionCameraController>();

        // Gets the position of the camera relative to the main player position
        cameraRelativePos = cam.transform.position - transform.position;

        // Calculate thresholds for momentum 
        groundSpeedThreshold = PlayerController.Speed() * velMod * PlayerController.SprintModifier();
        airSpeedThreshold = PlayerController.Speed() * airVelMod;
    }

    public void ControllerActiveState(bool active)
    {
        //rb.isKinematic = !active;

        if (active)
        {
            // fade gravity back in
            utiliseGravity.TempDisableGravity(0f, 0.5f);
        }
        else
        {
            utiliseGravity.SetUseGravity(false);
        }

        rb.drag = active ? 0f : 1f;
    }

    /*
     * Store movement input variables from PlayerController
     */
    public void Move(Vector3 _velocity, bool _sprinting)
    {
        if (frozen)
            return;

        velocity = _velocity;
        sprinting = _sprinting;
    }

    /*
     * Apply player and camera rotation based on input
     */
    public void LookRotation(Vector3 _rotation, float _cameraRotationX)
    {
        if (frozen)
            return;

        rotation = _rotation;
        cameraRotationX = _cameraRotationX;

        // Rotate player for horizontal camera movement
        transform.rotation *= Quaternion.Euler(rotation);

        // rotation calculation - clamps to limit values
        currentCamRotX -= cameraRotationX;
        currentCamRotX = Mathf.Clamp(currentCamRotX, -cameraRotLimit, cameraRotLimit);

        // apply rotation to transform of camera
        cam.transform.localEulerAngles = new Vector3(currentCamRotX, 0f, 0f);
    }

    // Run every physics iteration
    void FixedUpdate()
    {
        // if frozen, dont perform any movement
        if (frozen)
            return;

        // Check that player orientation aligns with gravity, if not then transition player to gravity direction
        CheckPlayerGravityAlignment();

        // if posessing vehicle
        if (!utiliseGravity.UseGravity())
            return;

        // Handle player movement
        PerformMovement();

        if (jumpTimer > 0)
            jumpTimer -= Time.fixedDeltaTime;
    }

    /*
     * TODO: move or remove
     * 
     * Try locking skybox and scene lighting to XZ orientation of the player
     * 
     *  - Could take this one step further
     *  Rotate skybox and lighting at the same time as gravity, but in 
     *  a different (opposite?) direction.
     *  
     *  Get a cool light/shadow sweep effect when warping
     *  Will need a slower warp for this effect though
     *  
     *  Puzzle elements!!
     *  Other pieces of scenery that rotate when gravity shifts
     *  paths that can only be accessed when gravity is in a particular direction?
     */

    /*
     * Handles GravityChangeNotification, updates local current gravity variable
     */
    void OnGravityChange(object sender, object args)
    {
        currentGravVector = GlobalGravityControl.GetCurrentGravityVector();
    }

    /*
     * Handler for BoostNotification from BoostRing objects
     * 
     * If the player is boosted, apply the boost velocity
     */
    void OnBoostNotification(object sender, object args)
    {
        Info<GameObject, Vector3> info = (Info<GameObject, Vector3>)args;
        if (info.arg0 != gameObject)
            return;

        // boosted object is the player
        rb.velocity = info.arg1;
        jumpTimer = jumpTimerDefault;
    }

    /*
     * Checks player orientation against gravity vector
     * realigns if player does not match gravity direction
     */
    void CheckPlayerGravityAlignment()
    {
        if (currentGravVector == -transform.up)
            return;

        // transition player orientation if not aligned to gravity
        UpdateGravityDirection(currentGravVector);
    }

    /*
     * Updates player rotation to align to gravity
     */
    public void UpdateGravityDirection(Vector3 _newGrav)
    {
        float angle = Vector3.Angle(-transform.up, _newGrav);
        Vector3 axis;

        if (_newGrav != transform.up)
            axis = Vector3.Cross(-transform.up, _newGrav);
        else
            axis = transform.forward;

        transform.Rotate(axis, angle, Space.World);
    }

    /*
     * Perform movement based on input variables
     */
    private void PerformMovement()
    {
        if (UseGroundMovement() && jumpTimer <= 0f)
        {
            // Use grounded movement physics
            //Debug.Log("Ground");
            GroundMovement();
            return;
        }
        // Use air/slide movement physics
        AirSlideMovement();
    }

    /*
     * Checks if the player should use ground movement physics
     */
    private bool UseGroundMovement()
    {
        /*
         * return true if:
         * 
         * IsOnGround() - defined by jumpCollider and if jumpTimer is <= 0
         * And
         * The slope angle below the player is below the slide threshold 
         * 
         * OR
         * 
         * the sliding variable is false (set by PlayerCollisionController)
         */
        CheckSlopeAngle();
        return (IsOnGround() && slopeAngle < PlayerCollisionController.slideThreshold) || !sliding;
    }

    /*
     * Gets the angle of the slope directly below the player
     */
    private void CheckSlopeAngle()
    {
        Vector3 up = transform.up;
        RaycastHit hitInfo;

        Ray ray = new Ray(transform.position - (up * 0.95f), -up);
        if (Physics.Raycast(ray, out hitInfo, 0.4f))
        {
            if (hitInfo.normal != transform.up)
            {
                slopeNormal = hitInfo.normal;
                slopeAngle = Vector3.Angle(hitInfo.normal, up);
                return;
            }
            slopeNormal = up;
            slopeAngle = 0f;
            return;
        }

        slopeAngle = float.MaxValue;
    }


    /*
     * TODO: You might have heard at some point someone describe controls as fluid, 
     * which basically means that the movement is smooth and consistent (even 
     * when colliding.) I noticed that if the avatar moved alongside a wall 
     * at an angle the velocity slowed down considerably. Ray casting might 
     * seem to provide a logical solution, but after some experimentation I 
     * found the simplest and most effective solution to this problem by 
     * observing the resulting velocity after collision: When the avatar 
     * collided, it moved slightly in the direction I wanted. So by normalizing 
     * that movement and multiplying it by our desired speed (magnitude), 
     * we get smooth movement while colliding every time.
     * 
     * NOTE: this may need to be applied to air movement as well re physics against walls
     */

    /*
     * Perform ground movement
     */
    void GroundMovement()
    {
        Vector3 newVel = rb.velocity;

        // Check if player has input
        if (velocity != Vector3.zero)
        {
            newVel = velocity * velMod;

            if (sprinting)
                newVel *= PlayerController.SprintModifier();

            // rotate input vector to align with surface normal
            Vector3 up = transform.up;
            Vector3 rotatedNormal = Vector3.RotateTowards(up, slopeNormal, PlayerCollisionController.slideThreshold * Mathf.Deg2Rad, 0);
            Quaternion rot = Quaternion.FromToRotation(up, rotatedNormal);
            newVel = rot * newVel;

            if (crouching)
                newVel *= crouchVelFactor;
        }
        else
        {
            // dampen velocity but allow for free vertical movement
            newVel -= (Vector3.ProjectOnPlane(newVel, transform.up) * 0.1f);
        }

        // MomentumSlide here
        newVel = MomentumSlide(newVel);

        // TODO: Wall slide logic here

        rb.velocity = newVel;
    }

    /*
     * Allows retaining speed for a while above the sprint speed threshold
     * 
     * TODO: restrict momentum retained through tight turns
     */
    private Vector3 MomentumSlide(Vector3 _newVel)
    {
        Vector3 vel = rb.velocity;
        float surfaceMagnitude = Vector3.Project(vel, _newVel).magnitude;
        bool aboveSpeedThreshold = surfaceMagnitude > groundSpeedThreshold;

        // if not above the speed threshold
        if (!aboveSpeedThreshold)
            return _newVel;

        bool inputInMovementDir = Vector3.Dot(_newVel, vel) > 0f;
        bool facingMovementDir = Vector3.Dot(transform.forward, vel) > 0.5f;

        if (sprinting && inputInMovementDir && facingMovementDir)
        {
            // Maintain higher velocity
            float newMagnitude = surfaceMagnitude * (1f - (Time.fixedDeltaTime * 0.1f));
            _newVel = (vel + _newVel).normalized * newMagnitude;
        }
        else
        {
            // Decelerate
            if (inputInMovementDir)
                _newVel -= Vector3.Project(_newVel, vel);

            Vector3 brakeVelocity = vel * 0.96f;

            _newVel = brakeVelocity + (_newVel * 0.2f);

        }

        return _newVel;
    }

    /*
     * Combined logic for sliding and airborne movement
     */
    void AirSlideMovement()
    {
        if (velocity == Vector3.zero)
            return;

        Vector3 newVel = velocity * airVelMod;
        Vector3 flatVel = Vector3.ProjectOnPlane(rb.velocity, currentGravVector);

        // if input velocity in direction of flight and velocity above threshold
        if (Vector3.Dot(newVel, flatVel) > 0 && flatVel.magnitude > airSpeedThreshold)
        {
            // cancel positive movement in direction of movement
            newVel -= Vector3.Project(newVel, flatVel);
        }

        if (colliding && Vector3.Dot(newVel, slideSurfaceVector) > 0)
        {
            // SLIDING
            // cancel positive movement in direction of slide normal
            newVel -= Vector3.Project(newVel, slideSurfaceVector);
        }

        // use impulse force to allow gradual speed/direction changes when midair
        rb.AddForce(newVel, ForceMode.Impulse);
    }

    /*
     * Determines if the player can jump.
     * 
     * Different conditions determine this if the player is in midair.
     */
    public bool CanJump(bool _midAir = false)
    {
        return ((_midAir && !JumpCollider.IsColliding() && jumpTimer <= 0f) || (!_midAir && IsOnGround())) && !vaulting && !frozen;
    }

    /*
     * Perform jump
     */
    public void Jump(float _jumpStrength, bool _midAirJump = false)
    {
        if (!CanJump(_midAirJump))
            return;

        Vector3 up = transform.up;

        // if already moving up, keeps current vertical momentum
        // allows higher jumps when moving up slopes
        if (Vector3.Dot(rb.velocity, up) < 0)
        {
            // moving down slope while jumping
            // cancel downward momentum when jump
            Vector3 yComponent = Vector3.Project(rb.velocity, up);
            rb.velocity -= yComponent;
        }

        rb.AddForce(up * _jumpStrength, ForceMode.VelocityChange);

        JumpCollider.Jump();

        if (!_midAirJump)
            jumpTimer = jumpTimerDefault;

        SetCrouching(false);
    }

    /*
     * Called when the jump button is held down
     * 
     * Triggers a ledge vault if a suitable ledge is in front of the player
     */
    public void JumpHold()
    {
        if (!vaulting && PlayerCollisionController.GetVaultHeight() > 0f)
        {
            StartCoroutine("PerformVault");
        }
    }

    /*
     * Performs a vault to the height of the ledge in front of the player
     */
    private IEnumerator PerformVault()
    {
        vaulting = true;

        while (PlayerCollisionController.GetVaultHeight() > 0f)
        {
            // Get the force required to move the player up to the level of the ledge
            float force = Mathf.Sqrt(PlayerCollisionController.GetVaultHeight() * -2 *
                                     -GlobalGravityControl.GetGravityStrength());

            rb.velocity = transform.up * force;
            yield return 0;
        }

        vaulting = false;
    }

    /*
     * Sets the crouching variable
     */
    public void SetCrouching(bool _crouching)
    {
        crouching = _crouching;
    }

    /*
     * Sets the collision state of the player
     * 
     * Called from PlayerCollisionController
     */
    public void SetCollisionState(bool _sliding, bool _colliding, Vector3 _slideNormal)
    {
        sliding = _sliding;
        colliding = _colliding;
        slideSurfaceVector = -Vector3.ProjectOnPlane(_slideNormal, currentGravVector);
    }

    /*
     * Warps the player to the current knife position, inheriting velocity and moving gravity vectors if required
     */
    public void WarpToKnife(bool _shiftGravity, KnifeController _knifeController, FibreOpticController _fibreOpticController = null)
    {
        if (frozen) return;

        // record starting position and rotation for transitionCamera
        Vector3 camStartPos = cam.transform.position;
        Quaternion camStartRot = cam.transform.rotation;
        Vector3 relativeFacing = cam.transform.forward;

        Vector3 newGravDirection = _knifeController.GetGravVector();

        bool fibreOpticWarp = _fibreOpticController != null;

        // Shift gravity if the difference between current gravity and surface normal is
        // above the threshold defined by warpGravShiftAngle
        float surfaceDiffAngle = Vector3.Angle(-transform.up, newGravDirection);
        bool gravityShift = (_shiftGravity && surfaceDiffAngle > warpGravShiftAngle && newGravDirection != Vector3.zero);

        if (gravityShift)
            this.PostNotification(GravityWarpNotification, _knifeController.GetStuckObject());
        else
            this.PostNotification(WarpNotification);

        // rotate to face new direction at end of warp.
        // needed for gravity and fibreoptic warps
        if (gravityShift)
            RotateToDirection(newGravDirection, relativeFacing);
        else if (fibreOpticWarp)
            RotateToDirection(currentGravVector, _fibreOpticController.GetExitDirection(), true);

        // Determine transitionCamera end rotation
        Quaternion camEndRot = (gravityShift) ? transform.rotation : camStartRot;

        // Setup transitionCamera
        transCamController.Setup(cam.fieldOfView, camStartPos, _knifeController, cameraRelativePos, camStartRot, camEndRot, gravityShift, _fibreOpticController);

        cam.enabled = false;
        Freeze();

        // Begin warp transition
        transCamController.StartTransition();

        if (gravityShift)
        {
            GlobalGravityControl.TransitionGravity(newGravDirection, transCamController.GetDuration());

            // Begin gravity shift countdown coroutine
            if (gravShiftTimerCoroutine != null)
                StopCoroutine(gravShiftTimerCoroutine);

            gravShiftTimerCoroutine = StartCoroutine("GravShiftCountdown");
        }
    }

    /*
     * Triggers a transition back to default gravity after a certain time.
     * 
     * Used to apply time limit to player gravity shifting
     */
    IEnumerator GravShiftCountdown()
    {
        // TODO: MAY WANT TO REMOVE THIS
        float t = 0.0f;
        while (t < 1f)
        {
            // Don't count down if frozen (stops countdown from running while warp transition is happening)
            if (!frozen)
                t += Time.deltaTime * (Time.timeScale / gravShiftTimeLimit);

            yield return 0;
        }

        // TODO: uncomment this if removing above
        //yield return new WaitForSeconds(gravShiftTimeLimit);

        // trigger transition back to default gravity
        GlobalGravityControl.TransitionToDefault();
    }

    /*
     * Reenables player camera/control etc when warp ends
     */
    void EndWarp(object sender, object args)
    {
        Info<Vector3, Vector3, bool, FibreOpticController> info = (Info<Vector3, Vector3, bool, FibreOpticController>)args;
        transform.position = info.arg0;

        // make sure camera is properly angled before reenabling
        cam.transform.localEulerAngles = new Vector3(currentCamRotX, 0f, 0f);
        cam.enabled = true;
        UnFreeze();

        GetComponent<UtiliseGravity>().TempDisableGravity(0f, 0.2f);

        // Check if knife velocity should be inherited
        Vector3 knifeVel = info.arg1.normalized;
        if ((info.arg2 || info.arg3) && knifeVel != Vector3.zero)
        {
            // adding magnitude here allows cumulative velocity gain
            // dot product to get component of velocity in direction of travel
            // If fibreoptic knife, use velocity relative to direction of start of fibre
            float projectedVelMagnitude = Vector3.Dot(rb.velocity, (info.arg3) ? -info.arg3.GetDirection() : knifeVel);

            // This line makes sure we only add player momentum if moving faster than base inherited momentum
            float relativeSpeed = Mathf.Max(projectedVelMagnitude - warpVelocityModifier, 0f);
            // adds component of current velocity along axis of knife movement

            // inherit player velocity
            rb.velocity = knifeVel * (warpVelocityModifier + relativeSpeed);
        }
        else
        {
            rb.velocity = knifeVel;

            // extend gravity disable slightly for blinkknife - to give time to vault if needed
            utiliseGravity.TempDisableGravity(0f, 0.5f);
        }
    }

    /*
     * Rotates the player to align with a new surface normal/facing direction.
     * 
     * Also aims the view as close as possible to the pre-warp view direction.
     *  - this should prevent the player from rotating wildly when changing gravity.
     *  
     * Also rotates player momentum to new direction
     *  - allows fun things as well as removing enormous jump by falling
     */
    private void RotateToDirection(Vector3 _gravDirection, Vector3 _lookDirection, bool _alignCameraElevation = false)
    {
        Vector3 newUpDir = -_gravDirection;

        // store local velocity before rotation
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);

        // aim the player at the new look vector
        Vector3 newAimVector = Vector3.ProjectOnPlane(_lookDirection, newUpDir).normalized;

        // LookAt can take the surface normal
        transform.LookAt(transform.position + newAimVector, newUpDir);

        if (_alignCameraElevation)
        {
            float angle = Vector3.Angle(newAimVector, _lookDirection);
            currentCamRotX = (Vector3.Dot(newUpDir, _lookDirection) > 0) ? -angle : angle;
        }
        else
        {
            // reset camera pitch to parallel with surface
            currentCamRotX = 0f;
        }

        // rotate velocity to new direction
        rb.velocity = transform.TransformDirection(localVelocity);
    }

    /*
     * Freezes player RigidBody physics and movement and hides player mesh etc
     */
    public void Freeze()
    {
        if (frozen)
            return;
        frozen = true;
        frozenVel = rb.velocity;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        rb.detectCollisions = false;
        velocity = Vector3.zero;
        sprinting = false;
        Hide();
    }

    /*
     * Unfreezes RigidBody and reenables movment and visuals 
     */
    public void UnFreeze()
    {
        if (!frozen)
            return;
        frozen = false;
        rb.isKinematic = false;
        rb.detectCollisions = true;
        rb.velocity = frozenVel;
        UnHide();
    }

    /*
     * Hides the player visuals and viewmodel
     */
    private void Hide()
    {
        playerRenderer.enabled = false;
        playerKnifeController.HideKnife(true);
    }

    /*
     * Shows the player visuals and viewmodel
     */
    private void UnHide()
    {
        playerRenderer.enabled = true;
        playerKnifeController.HideKnife(false);
    }

    /*
     * Checks if the player if frozen
     */
    public bool IsFrozen()
    {
        return frozen;
    }

    /*
     * Checks if the player is on the ground and not jumping
     */
    public bool IsOnGround()
    {
        return JumpCollider.IsColliding() && jumpTimer <= 0;
    }

    /*
     * Returns the ground velocity modifier
     */
    public static float VelMod()
    {
        return velMod;
    }

    /*
     * Returns the last input velocity value
     */
    public Vector3 GetInputVelocity()
    {
        return velocity;
    }
}
