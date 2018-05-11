using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private float cameraRotLimit = 90f;

    //[SerializeField]
    //private GameObject transitionCameraPrefab;

    //[SerializeField]
    //private float gravity = 0.3f;

    //[SerializeField]
    //private float sprintDeceleration = 0.9f;

    private static float velMod = 1.5f;
    private static float airVelMod = 1.2f;

    // Shift gravity if the difference between current gravity and surface normal is
    // above the threshold defined by warpGravShiftAngle
    [SerializeField]
    private float warpGravShiftAngle = 1f;

    //[SerializeField]
    private float gravShiftTimeLimit = 2f;

    private Coroutine gravShiftTimerCoroutine;

    //[SerializeField]
    //[Range(0f, 20f)]
    //private float airVelThreshold = 10f;

    [SerializeField]
    [Range(0.0f, 50.0f)]
    private float warpVelocityModifier = 20f;


    //[SerializeField]
    //[Range(0f, 45f)]
    //private float gravityShiftAngleMax = 15f;

    public const string WarpNotification = "PlayerMotor.WarpNotification";
    public const string GravityWarpNotification = "PlayerMotor.GravityWarpNotification";

    private bool frozen;
    //private Vector3 frozenPos = Vector3.zero;
    private Vector3 frozenVel = Vector3.zero;

    //private bool onGround;
    private bool sliding;
    private bool colliding;

    private bool crouching;
    private float crouchVelFactor = 1f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private bool sprinting;
    private float cameraRotationX;
    private float currentCamRotX;

    private float jumpTimer;
    private float jumpTimerDefault = 0.2f; // was 0.6f
    //private bool momentumFlight;

    private Rigidbody rb;

    //private UtiliseGravity grav;

    private Vector3 cameraRelativePos;

    private Vector3 currentGravVector;
    //private float currentGravStrength;

    //private const float gravViewAlignSpeed = 4f;

    //private HealthController healthEnergy;


    private float groundSpeedThreshold;
    private float airSpeedThreshold;

    private Vector3 slopeNormal;

    private bool vaulting;

    private TransitionCameraController transCamController;

    void OnEnable()
    {
        //healthEnergy = GetComponent<HealthController>();

        UpdateGravityValues();
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
        //grav = GetComponent<UtiliseGravity>();
        //if (transitionCameraPrefab == null)
        //{
        //    throw new MissingReferenceException("No transitionCameraPrefab in PlayerMotor");
        //}

        transCamController = GameObject.Find("TransitionCamera").GetComponent<TransitionCameraController>();

        cameraRelativePos = cam.transform.position - transform.position;

        groundSpeedThreshold = PlayerController.Speed() * velMod * PlayerController.SprintModifier();
        airSpeedThreshold = PlayerController.Speed() * airVelMod * PlayerController.SprintModifier();
    }

    // gets movement vector from PlayerController
    public void Move(Vector3 _velocity, bool _sprinting)
    {
        if (frozen)
        {
            velocity = Vector3.zero;
            sprinting = false;
            return;
        }

        velocity = _velocity;
        sprinting = _sprinting;
    }

    // gets rotation vector 
    //public void Rotate(Vector3 _rotation)
    //{
    //    if (frozen)
    //    {
    //        rotation = Vector3.zero;
    //        return;
    //    }

    //    rotation = _rotation;
    //}

    //// gets camera rotation vector 
    //public void RotateCamera(float _cameraRotationX)
    //{
    //    if (frozen)
    //    {
    //        cameraRotationX = 0f;
    //        return;
    //    }

    //    cameraRotationX = _cameraRotationX;
    //}

    // Recieves rotation values from PlayerController and applies rotation
    public void LookRotation(Vector3 _rotation, float _cameraRotationX)
    {
        if (frozen)
        {
            rotation = Vector3.zero;
            cameraRotationX = 0f;
            return;
        }

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

    //void Update()
    //{
    //    // Move rotation code to here
    //    PerformRotation();
    //}

    // Run every physics iteration
    void FixedUpdate()
    {
        // if frozen, dont perform any movement
        if (!frozen)
        {
            CheckPlayerGravityAlignment();

            PerformMovement();
            //onGround = false;

            if (jumpTimer > 0)
                jumpTimer -= Time.fixedDeltaTime;
        }

        // Freezing needs to stop rotation too
        //PerformRotation();
    }

    /*
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

    //perform rotation based on rotation variable
    //private void PerformRotation()
    //{
    //    //rb.MoveRotation(transform.rotation * Quaternion.Euler(rotation)); // switched off this as update to 2017 seems to have bugged it
    //    //transform.rotation *= Quaternion.Euler(rotation);
    //    rb.rotation *= Quaternion.Euler(rotation);

    //    // rotation calculation - clamps to limit values
    //    currentCamRotX -= cameraRotationX;
    //    currentCamRotX = Mathf.Clamp(currentCamRotX, -cameraRotLimit, cameraRotLimit);

    //    // apply rotation to transform of camera
    //    cam.transform.localEulerAngles = new Vector3(currentCamRotX, 0f, 0f);
    //}

    void OnGravityChange(object sender, object args)
    {
        UpdateGravityValues();
    }

    void UpdateGravityValues()
    {
        // update gravity vector and strength from GlobalGravityControl
        currentGravVector = GlobalGravityControl.GetCurrentGravityVector();
        //currentGravStrength = GlobalGravityControl.GetGravityStrength();
    }

    /*
     * Handler for BoostNotification from BoostRing objects
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
        // transition player orientation if not aligned to gravity
        if (currentGravVector == -transform.up)
            return;

        //Vector3 transitionUp = Vector3.RotateTowards(transform.up, -currentGravVector, gravViewAlignSpeed * Mathf.Deg2Rad, 0f);

        //UpdateGravityDirection(-transitionUp);

        UpdateGravityDirection(currentGravVector);
    }

    // Updates player rotation to gravity
    public void UpdateGravityDirection(Vector3 _newGrav)
    {
        float angle = Vector3.Angle(-transform.up, _newGrav);
        Vector3 axis;

        if (_newGrav != transform.up)
            axis = Vector3.Cross(-transform.up, _newGrav);
        else
            axis = transform.forward;

        transform.Rotate(axis, angle, Space.World);

        //transform.LookAt(transform.position + transform.forward, -_newGrav);
    }

    // perform movement based on velocity variable
    private void PerformMovement()
    {

        // Apply the current gravity
        //if (useGravity)
        //    rb.AddForce(currentGravVector * currentGravStrength, ForceMode.Acceleration); // changed from -transform.up to stop grav transitions from changing velocity

        if (UseGroundMovement() && jumpTimer <= 0f)
        {
            // Grounded
            //momentumFlight = false;
            GroundMovement();
            return;
            //Debug.Log("Ground: " + Vector3.ProjectOnPlane(rb.velocity, transform.up).magnitude);
        }
        if (colliding) // TODO: double-check that jumpTimer <= 0f check is not needed here
        {
            // Sliding
            SlideMovement();
            return;
            //Debug.Log("Slide:  " + Vector3.ProjectOnPlane(rb.velocity, transform.up).magnitude);
        }
        // Airborne
        AirMovement();
        //Debug.Log("Air:    " + Vector3.ProjectOnPlane(rb.velocity, transform.up).magnitude);
    }

    private bool UseGroundMovement()
    {
        return (IsOnGround() && GetSlopeAngle() < PlayerCollisionController.slideThreshold) || !sliding;
    }

    // returns the angle of the slope directly below the player
    private float GetSlopeAngle()
    {
        float rayDistance = 0.5f;
        RaycastHit hitInfo;

        Ray ray = new Ray(transform.position - (transform.up * 0.9f), -transform.up);
        if (Physics.Raycast(ray, out hitInfo, rayDistance))
        {
            if (hitInfo.normal != transform.up)
            {
                slopeNormal = hitInfo.normal;
                return Vector3.Angle(hitInfo.normal, transform.up);
            }
            slopeNormal = transform.up;
            return 0f;
        }

        return float.MaxValue;
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
    void GroundMovement()
    {
        Vector3 newVel = rb.velocity;

        if (velocity != Vector3.zero)
        {
            newVel = velocity * velMod;

            if (sprinting)
                newVel *= PlayerController.SprintModifier();

            // rotate input vector to align with surface normal
            Quaternion rot = Quaternion.FromToRotation(transform.up, slopeNormal);
            newVel = rot * newVel;

            if (crouching)
            {
                newVel *= 0.5f;
            }

        }
        else
        {
            newVel = rb.velocity - (Vector3.ProjectOnPlane(rb.velocity, transform.up) * 0.1f);
            // may not be nessecary, friction already does a bit of this

            //rb.velocity = Vector3.zero;
        }

        // MomentumSlide here
        newVel = MomentumSlide(newVel);

        rb.velocity = newVel;
    }

    /*
     * while moving above sprint speed, retain gained speed
     */
    private Vector3 MomentumSlide(Vector3 _newVel)
    {
        float surfaceMagnitude = Vector3.Project(rb.velocity, _newVel).magnitude;
        //bool aboveSpeedThreshold = rb.velocity.magnitude > groundSpeedThreshold;
        bool aboveSpeedThreshold = surfaceMagnitude > groundSpeedThreshold;
        bool inputInMovementDir = Vector3.Dot(_newVel, rb.velocity) > 0f;
        bool facingMovementDir = Vector3.Dot(transform.forward, rb.velocity) > 0.5f;

        if (!aboveSpeedThreshold)
        {
            return _newVel;
        }

        if (sprinting && inputInMovementDir && facingMovementDir)
        {
            // maintain velocity for a while
            float newMagnitude = surfaceMagnitude * (1f - (Time.fixedDeltaTime * 0.1f));
            _newVel = (rb.velocity + _newVel).normalized * newMagnitude;
        }
        else
        {
            // decelerate
            if (inputInMovementDir)
                _newVel -= Vector3.Project(_newVel, rb.velocity);

            Vector3 brakeVelocity = rb.velocity * 0.96f;

            _newVel = brakeVelocity + (_newVel * 0.2f);

        }

        return _newVel;
    }

    /*
     * Handle movement physics while sliding
     */
    void SlideMovement()
    {
        Vector3 velocityTemp = velocity * airVelMod;
        //rb.AddForce(velocityTemp, ForceMode.Impulse);

        // THE FOLLOWING CODE LIFTED FROM AIR MOVEMENT - TESTING
        Vector3 flatVel = Vector3.ProjectOnPlane(rb.velocity, currentGravVector);

        float threshold = PlayerController.Speed() * airVelMod;
        if (sprinting)
            threshold *= PlayerController.SprintModifier();

        // if input velocity in direction of movement and velocity above threshold
        if (Vector3.Dot(velocityTemp, flatVel) > 0 && flatVel.magnitude > threshold)
        {
            // cancel positive movement in direction of flight
            velocityTemp -= Vector3.Project(velocityTemp, flatVel);
        }

        // use impulse force to allow slower changes to direction/speed when at high midair speed
        rb.AddForce(velocityTemp, ForceMode.Impulse);
    }


    /*
     * Handle movement physics while midair
     */
    void AirMovement()
    {
        Vector3 flatVel = Vector3.ProjectOnPlane(rb.velocity, currentGravVector);

        if (flatVel.magnitude > airSpeedThreshold)
        {
            //momentumFlight = true;
        }

        if (velocity != Vector3.zero)
        {
            HandleMidairInput(flatVel);
        }
    }

    /*
     * Handles midair movement with an input vector
     * 
     * If current speed is above the threshold defined by the run/sprint speed 
     * then removes component of input vector in direction of movement
     * (disallows increase of speed)
     * 
     * Applies modified input vector to RigidBody depending on whether 
     * we are above the momentumFlight threshold
     */
    private void HandleMidairInput(Vector3 _flatVel)
    {

        Vector3 velocityTemp = velocity * airVelMod;

        float threshold = PlayerController.Speed() * airVelMod;
        if (sprinting)
            threshold *= PlayerController.SprintModifier();

        // if input velocity in direction of flight and velocity above threshold
        if (Vector3.Dot(velocityTemp, _flatVel) > 0 && _flatVel.magnitude > threshold)
        {
            // cancel positive movement in direction of flight
            velocityTemp -= Vector3.Project(velocityTemp, _flatVel);
        }

        // use impulse force to allow gradual speed/direction changes when midair
        rb.AddForce(velocityTemp, ForceMode.Impulse);
    }

    public bool CanJump(bool _midAir = false)
    {
        return ((_midAir && !JumpCollider.IsColliding() && jumpTimer <= 0f) || (!_midAir && IsOnGround())) && !vaulting && !frozen;
    }

    // perform jump when triggered
    public void Jump(float _jumpStrength, bool _midAirJump = false)
    {
        if (!CanJump(_midAirJump))
            return;

        // if already moving up, keeps current vertical momentum
        // allows higher jumps when moving up slopes
        if (Vector3.Dot(rb.velocity, transform.up) < 0)
        {
            // moving down slope while jumping
            // cancel downward momentum when jump
            Vector3 yComponent = Vector3.Project(rb.velocity, transform.up);
            rb.velocity -= yComponent;
        }

        rb.AddForce(transform.up * _jumpStrength, ForceMode.VelocityChange);

        JumpCollider.Jump();

        if (!_midAirJump)
            jumpTimer = jumpTimerDefault;
        SetCrouching(false);
    }

    // handle jump button being held
    public void JumpHold()
    {
        if (!vaulting && PlayerCollisionController.GetVaultHeight() > 0f)
        {
            StartCoroutine("PerformVault");
        }
    }

    private IEnumerator PerformVault()
    {
        vaulting = true;
        while (PlayerCollisionController.GetVaultHeight() > 0f)
        {
            float force = Mathf.Sqrt(PlayerCollisionController.GetVaultHeight() * -2 *
                                     -GlobalGravityControl.GetGravityStrength());

            rb.velocity = transform.up * force;
            //jumpTimer = jumpTimerDefault;
            yield return 0;
        }
        vaulting = false;
    }

    // either begin or end crouching
    public void SetCrouching(bool _crouching)
    {
        crouching = _crouching;
    }

    public void SetCollisionState(bool _sliding, bool _colliding)
    {
        sliding = _sliding;
        colliding = _colliding;
    }

    /*
     * Warps the player to the current knife position, inheriting velocity and moving gravity vectors if required
     */
    public void WarpToKnife(bool _shiftGravity, KnifeController _knifeController, bool _bounceWarp, FibreOpticController _fibreOpticController = null)
    {
        if (frozen) return;
        Vector3 camStartPos = cam.transform.position;
        Quaternion camStartRot = cam.transform.rotation;
        Vector3 relativeFacing = cam.transform.forward;

        //Vector3 newPos = _knifeController.GetWarpPosition();
        Vector3 newGravDirection = _knifeController.GetGravVector();

        bool fibreOpticWarp = _fibreOpticController != null;

        // Shift gravity if the difference between current gravity and surface normal is
        // above the threshold defined by warpGravShiftAngle
        float surfaceDiffAngle = Vector3.Angle(-transform.up, newGravDirection);
        bool gravityShift = (_shiftGravity && surfaceDiffAngle > warpGravShiftAngle && newGravDirection != Vector3.zero);

        if (gravityShift)
        {
            this.PostNotification(GravityWarpNotification, _knifeController.GetStuckObject());
        }
        else
        {
            this.PostNotification(WarpNotification);
        }

        // TODO: parenting may be obsolete
        //transform.SetParent(null);

        // Unsure if this needs to happen before or after the moveposition

        // rotate to surface normal
        if (gravityShift)
            RotateToDirection(newGravDirection, relativeFacing);
        else if (fibreOpticWarp)
            RotateToDirection(currentGravVector, _fibreOpticController.GetExitDirection(), true);

        // move to knife position
        // NOTE: this needs to be double-checked, moving rb by itself doesnt update position properly for grav warp
        //rb.MovePosition(newPos);
        //rb.position = newPos;
        //transform.position = newPos;

        //Vector3 camEndPos = newPos + (transform.rotation * cameraRelativePos); //Needs to get position camera will be in after move
        Quaternion camEndRot = camStartRot;
        if (gravityShift)
        {
            camEndRot = transform.rotation;
        }

        // fixes horizontal momentum lock when warping
        //onGround = false;

        //GameObject transCamera = (GameObject)Instantiate(transitionCameraPrefab, camStartPos, cam.transform.rotation);
        //TransitionCameraController transCamController = transCamera.GetComponent<TransitionCameraController>();
        transCamController.Setup(cam.fieldOfView, camStartPos, _knifeController, cameraRelativePos, camStartRot, camEndRot, gravityShift, _fibreOpticController);

        cam.enabled = false;
        Freeze();

        transCamController.StartTransition();

        if (gravityShift)
        {
            GlobalGravityControl.TransitionGravity(newGravDirection, transCamController.GetDuration());

            // Begin gravity shift countdown coroutine
            if (gravShiftTimerCoroutine != null)
            {
                StopCoroutine(gravShiftTimerCoroutine);
            }
            gravShiftTimerCoroutine = StartCoroutine("GravShiftCountdown");
        }
    }

    // counts for the given gravity shift duration then triggers a transition back to default gravity
    IEnumerator GravShiftCountdown()
    {
        float t = 0.0f;
        while (t < 1f)
        {
            // Don't count down if frozen (stops countdown from running while warp transition is happening)
            if (!frozen)
                t += Time.deltaTime * (Time.timeScale / gravShiftTimeLimit);

            yield return 0;
        }

        // trigger transition back to default gravity
        GlobalGravityControl.TransitionToDefault();
    }

    // Resets/reenables player once warp transition has completed
    void EndWarp(object sender, object args)
    {
        Info<Vector3, Vector3, bool, FibreOpticController> info = (Info<Vector3, Vector3, bool, FibreOpticController>)args;
        transform.position = info.arg0;

        // make sure camera is properly angled before reenabling
        cam.transform.localEulerAngles = new Vector3(currentCamRotX, 0f, 0f);
        cam.enabled = true;
        UnFreeze();

        GetComponent<UtiliseGravity>().TempDisableGravity(0f, 0.2f);

        // Inherit knife velocity at end of warp
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

            //rb.velocity = (_velocity * warpVelocityModifier) + (_velocity.normalized * projectedVelMagnitude);

            // inherit player velocity
            rb.velocity = (knifeVel * (warpVelocityModifier + relativeSpeed));
            //if (rb.velocity.magnitude > airSpeedThreshold)
            //    momentumFlight = true;

            // fixes horizontal momentum lock when warping
            //onGround = false;

        }
        else
        {
            rb.velocity = knifeVel;

            // extend gravity disable slightly for blinkknife - to give time to vault if needed
            GetComponent<UtiliseGravity>().TempDisableGravity(0f, 0.5f);
        }
    }

    /*
     * Rotates the player to align with a new surface normal.
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

    //  public bool WallHang()
    //  {
    //      if (onGround)
    //          return false;
    //      //if (velocity.magnitude <= 0.1f) // if not moving or crouching?
    //      if (crouching) // crouching?
    //      {
    //          StartCoroutine(WallHangCoroutine());
    //          return true;
    //      }
    //      return false;
    //  }

    //  // hangs on wall while crouching through warp
    //  IEnumerator WallHangCoroutine()
    //  {
    //crouchVelFactor = 0.5f;
    //      // do not inherit player velocity if wall hanging
    //      frozenVel = Vector3.zero;
    //      yield return new WaitWhile(() => velocity.magnitude == 0f);
    //      transform.SetParent(null);
    //      UnFreeze();
    //  }

    /*
     * Returns the current position of the feet of the player, relative to the current gravity direction
     */
    //Vector3 GetFootPosition()
    //{
    //    return transform.position + GlobalGravityControl.GetCurrentGravityVector();
    //}

    public void Freeze()
    {
        if (frozen)
            return;
        frozen = true;
        //        frozenPos = rb.position;
        frozenVel = rb.velocity;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        Hide();
    }

    public void UnFreeze()
    {
        if (!frozen)
            return;
        frozen = false;
        rb.isKinematic = false;
        //        rb.position = frozenPos;
        rb.velocity = frozenVel;
        UnHide();
    }

    private void Hide()
    {
        GetComponent<Renderer>().enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        cam.GetComponent<PlayerKnifeController>().HideKnife(true);
    }

    private void UnHide()
    {
        GetComponent<Renderer>().enabled = true;
        GetComponent<CapsuleCollider>().enabled = true;
        cam.GetComponent<PlayerKnifeController>().HideKnife(false);
    }

    public bool IsFrozen()
    {
        return frozen;
    }

    public bool IsOnGround()
    {
        return JumpCollider.IsColliding() && jumpTimer <= 0;
    }

    public static float VelMod()
    {
        return velMod;
    }

    /*
     * Changes the player's current health
     * Positive numbers increase, negative decrease
     */
    //public void ModifyHealth(float _changeAmt)
    //{
    //    healthEnergy.ModifyHealth(_changeAmt);
    //}

    //public void ModifyEnergy(float _changeAmt)
    //{
    //    healthEnergy.ModifyEnergy(_changeAmt);
    //}

    //public float GetHealthNormalised()
    //{
    //    return healthEnergy.GetHealthNormalised();
    //}

    //public float GetEnergyNormalised()
    //{
    //    return healthEnergy.GetEnergyNormalised();
    //}
}
