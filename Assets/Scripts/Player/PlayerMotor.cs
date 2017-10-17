using AssemblyCSharp;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private float cameraRotLimit = 90f;

    [SerializeField]
    private GameObject transitionCameraPrefab;

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
    private bool canHover;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private bool sprinting;
    private float cameraRotationX;
    private float currentCamRotX;

    private int jumpTimer;
    private bool momentumFlight;

    private Rigidbody rb;

    private Vector3 cameraRelativePos;

    private Vector3 currentGravVector;
    private float currentGravStrength;

    private float viewShiftSpeed;
    private const float maxViewShiftSpeed = 4f;
    private const float viewShiftStrengthMax = 35f;

    private HealthController healthEnergy;

    private float speedThreshold;

    private Vector3 slopeNormal;

    void Awake()
    {
        healthEnergy = GetComponent<HealthController>();

        currentGravVector = GlobalGravityControl.GetCurrentGravityVector();
        currentGravStrength = GlobalGravityControl.GetGravityStrength();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (transitionCameraPrefab == null)
        {
            throw new MissingReferenceException("No transitionCameraPrefab in PlayerMotor");
        }

        cameraRelativePos = cam.transform.position - transform.position;
        canHover = true;

        speedThreshold = PlayerController.Speed() * velMod * PlayerController.SprintModifier();
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
    public void Rotate(Vector3 _rotation)
    {
        if (frozen)
        {
            rotation = Vector3.zero;
            return;
        }

        rotation = _rotation;
    }

    // gets camera rotation vector 
    public void RotateCamera(float _cameraRotationX)
    {
        if (frozen)
        {
            cameraRotationX = 0f;
            return;
        }

        cameraRotationX = _cameraRotationX;
    }

    // Run every physics iteration
    void FixedUpdate()
    {
        // if frozen, dont perform any movement
        if (!frozen)
        {
            GravityUpdate();

            PerformMovement();
            //onGround = false;

            jumpTimer--;

            if (IsOnGround())
                canHover = true;
        }

        // Freezing needs to stop rotation too
        PerformRotation();

        //Debug.Log(rb.velocity.magnitude);
    }

    void GravityUpdate()
    {
        // update gravity vector and strength from GlobalGravityControl
        currentGravVector = GlobalGravityControl.GetCurrentGravityVector();
        currentGravStrength = GlobalGravityControl.GetGravityStrength();

        // transition player orientation if not aligned to gravity
        if (currentGravVector == -transform.up)
            return;

        // gravShiftSpeed change relative to strength
        if (currentGravStrength >= viewShiftStrengthMax)
            viewShiftSpeed = maxViewShiftSpeed;
        else
            viewShiftSpeed = Utilities.MapValues(currentGravStrength, 0f, viewShiftStrengthMax, 0f, maxViewShiftSpeed, true);


        Vector3 transitionUp = Vector3.RotateTowards(transform.up, -currentGravVector, viewShiftSpeed * Mathf.Deg2Rad, 0f);

        UpdateGravityDirection(-transitionUp);
    }

    //private void KeepGrounded()
    //{
    //RaycastHit hit;
    //if (Physics.SphereCast(transform.position, 0.5f, -transform.up, out hit, 2f))
    //{
    //    //			Debug.Log ("Grounding");
    //    //rb.MovePosition (transform.position + new Vector3(0.0f, -hit.distance, 0.0f));
    //    rb.MovePosition(transform.position - (transform.up * hit.distance));
    //    //rb.velocity = Vector3.zero;
    //    //rb.velocity = transform.rotation * new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
    //}
    //}

    // perform movement based on velocity variable
    private void PerformMovement()
    {

        // Apply the current gravity
        rb.AddForce(currentGravVector * currentGravStrength, ForceMode.Acceleration); // changed from -transform.up to stop grav transitions from changing velocity


        if (UseGroundMovement() && jumpTimer <= 0)
        {
            // grounded
            momentumFlight = false;
            GroundMovement();
        }
        else if (colliding)
        {
            // Sliding
            Vector3 velocityTemp = velocity * airVelMod;
            rb.AddForce(velocityTemp, ForceMode.Impulse);
        }
        else
        {
            // airborne
            AirMovement();
        }
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

        Ray ray = new Ray(transform.position - transform.up, -transform.up);
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
     * You might have heard at some point someone describe controls as fluid, 
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
     * 
     * TODO: Add something here to use high-speed movement physics. IE: like slow vs fast air movement
     */
    private Vector3 MomentumSlide(Vector3 _newVel)
    {
        bool aboveSpeedThreshold = rb.velocity.magnitude > speedThreshold;
        bool inputInMovementDir = Vector3.Dot(_newVel, rb.velocity) > 0.5f;
        bool facingMovementDir = Vector3.Dot(transform.forward, rb.velocity) > 0.5f;

        if (aboveSpeedThreshold)
        {
            return _newVel;
        }

        if (sprinting && inputInMovementDir && facingMovementDir)
        {
            _newVel = (rb.velocity + _newVel).normalized * rb.velocity.magnitude;
        }
        else
        {
            // decelerate
            //Debug.Log(2 * Time.fixedDeltaTime);
            float newMagnitude = rb.velocity.magnitude - (Time.fixedDeltaTime * 20f); 
            _newVel = (rb.velocity + _newVel).normalized * newMagnitude;

            //_newVel = rb.velocity - (Vector3.ProjectOnPlane(rb.velocity, transform.up) * 0.1f);
        }

        return _newVel;
    }


    /*
     * Handle movement physics while midair
     */
    void AirMovement()
    {

        float threshold = PlayerController.Speed() * airVelMod * PlayerController.SprintModifier();
        Vector3 flatVel = Vector3.ProjectOnPlane(rb.velocity, currentGravVector);

        if (flatVel.magnitude > threshold)
        {
            momentumFlight = true;
        }

        if (velocity != Vector3.zero)
        {
            HandleMidairInput(flatVel);
        }
        else
        {
            // handle situation in which movement is below walk speed
            // cancel momentum to allow precise small jumps

            if (!momentumFlight)
            {
                // dampen movement
                rb.velocity -= (flatVel * 0.1f);
            }
        }

        if (crouching && canHover)
        {
            canHover = false;
            momentumFlight = false; // comment this out when reworking hover - should depend on air speed
            StartCoroutine(Hover());
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

        if (momentumFlight)
        {
            // use impulse force to allow slower changes to direction/speed when at high midair speed
            rb.AddForce(velocityTemp, ForceMode.Impulse);
        }
        else
        {
            // use direct velocity changes to allow more responsive jump control at slower speeds
            rb.velocity += (velocityTemp * 0.5f);
        }
    }

    /*
     * Currently cancels almost all air movement and holds height for a few seconds.
     * 
     * Due to momentum/sliding changes, hovering while at high speed feels jarring.
     * Perhaps only dampen vertical movement? or downward movement?
     * Or decelerate horizontal movement over a short period.
     */
    IEnumerator Hover()
    {
        crouchVelFactor = 0.5f;
        while (crouchVelFactor < 1f && !IsOnGround())
        {
            rb.velocity *= crouchVelFactor;
            crouchVelFactor *= 1.01f;
            yield return 0;
        }
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

    // TODO: There is a bug somewhere relating to rotation - sometimes camera horizontal movement is prevented while airborne
    //perform rotation based on rotation variable
    private void PerformRotation()
    {
        rb.MoveRotation(transform.rotation * Quaternion.Euler(rotation));
        if (cam != null)
        {
            // rotation calculation - clamps to limit values
            currentCamRotX -= cameraRotationX;
            currentCamRotX = Mathf.Clamp(currentCamRotX, -cameraRotLimit, cameraRotLimit);

            // apply rotation to transform of camera
            cam.transform.localEulerAngles = new Vector3(currentCamRotX, 0f, 0f);
        }
    }

    // perform jump when triggered by PlayerController
    public void Jump(float _jumpStrength)
    {
        if (!IsOnGround() || frozen)
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

        jumpTimer = 30;
        SetCrouching(false);
    }

    // either begin or end crouching
    public void SetCrouching(bool _crouching)
    {
        crouching = _crouching;

        //if (crouching)
        //    crouchVelFactor = 0.5f;
        //else
        //    crouchVelFactor = 1f;
    }

    //public void SetOnGround(bool _onGround)
    //{
    //    if (jumpTimer > 0)
    //        return;
    //    onGround = _onGround;

    //    if (onGround)
    //        canHover = true;
    //}

    public void SetCollisionState(bool _sliding, bool _colliding)
    {
        sliding = _sliding;
        colliding = _colliding;
    }

    //private void OnCollisionStay(){
    //	colliding = true;
    //}


    /*
     * Warps the player to the current knife position, inheriting velocity and moving gravity vectors if required
     * 
     * TODO: either here or somewhere in knife code - need to project player hitbox to see if can fit.
     * And cause warp to 'fizzle' if no good place for player to warp to is found (eg knife is between two close walls)
     */
    public void WarpToKnife(bool _shiftGravity, Vector3 _velocity, KnifeController _knifeController, bool _bounceWarp)
    {
        if (frozen) return;
        Vector3 camStartPos = cam.transform.position;
        Quaternion camStartRot = cam.transform.rotation;
        Vector3 relativeFacing = cam.transform.forward;

        Vector3 newPos = _knifeController.GetWarpPosition();
        Vector3 newGravDirection = _knifeController.GetGravVector();

        // Shift gravity if the difference between current gravity and surface normal is
        // above the threshold defined by warpGravShiftAngle
        float surfaceDiffAngle = Vector3.Angle(-transform.up, newGravDirection);
        bool gravityShift = (_shiftGravity && surfaceDiffAngle > warpGravShiftAngle && newGravDirection != Vector3.zero);

        if (gravityShift)
        {
            this.PostNotification(GravityWarpNotification, _knifeController.GetObjectCollided());
        }

        transform.SetParent(null);

        // Unsure if this needs to happen before or after the moveposition

        // rotate to surface normal
        if (gravityShift)
        {
            RotateToDirection(newGravDirection, relativeFacing);
        }
        
        // move to knife position
        // NOTE: this needs to be double-checked, moving rb by itself doesnt update position properly for grav warp
        //rb.MovePosition(newPos);
        //rb.position = newPos;
        transform.position = newPos;

        Vector3 camEndPos = newPos + (transform.rotation * cameraRelativePos); //Needs to get position camera will be in after move
        Quaternion camEndRot = camStartRot;
        if (gravityShift)
        {
            camEndRot = transform.rotation;
        }

        // INHERITED VELOCITY MUST BE RELATIVE TO PLAYER DIRECTION

        if (_bounceWarp && _velocity != Vector3.zero)
        {
            // adding magnitude here allows cumulative velocity gain
            // dot product to get component of velocity in direction of travel
            float projectedVelMagnitude = Vector3.Dot(rb.velocity, _velocity);
            // This line makes sure we only add player momentum if moving faster than base inherited momentum
            float relativeSpeed = Mathf.Max(projectedVelMagnitude - warpVelocityModifier, 0f);
            // adds component of current velocity along axis of knife movement

            //rb.velocity = (_velocity * warpVelocityModifier) + (_velocity.normalized * projectedVelMagnitude);

            // inherit player velocity
            rb.velocity = (_velocity * (warpVelocityModifier + relativeSpeed));
            momentumFlight = true;

            // fixes horizontal momentum lock when warping
            //onGround = false;
        }
        else
        {
            rb.velocity = _velocity;
        }

        // fixes horizontal momentum lock when warping
        //onGround = false;

        GameObject transCamera = (GameObject)Instantiate(transitionCameraPrefab, camStartPos, cam.transform.rotation);

        TransitionCameraController transCamController = transCamera.GetComponent<TransitionCameraController>();
        transCamController.Setup(cam, this, camStartPos, camEndPos, camStartRot, camEndRot, gravityShift);
        float duration = transCamController.GetDuration();
        transCamController.StartTransition();

        if (gravityShift)
        {
            GlobalGravityControl.TransitionGravity(newGravDirection, duration);
        }

        canHover = true;

    }

    // TODO: try out altering camera angles to maintain global look direction
    // TODO: - similar to RotateToDirection
    public void UpdateGravityDirection(Vector3 _newGrav)
    {
        Vector3 _newUp = -_newGrav;
        // might be nessecary
        transform.LookAt(transform.position + transform.forward, _newUp); //?
        // probably need similar code to RotateToDirection to align in correct direction

        // possible fix - take from site to eliminate setting transform.up issue
        transform.rotation = Quaternion.LookRotation(_newUp, -transform.forward);
        transform.Rotate(Vector3.right, 90f);
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
    private void RotateToDirection(Vector3 _gravDirection, Vector3 _relativeFacing)
    {
        Vector3 _newUpDir = -_gravDirection;

        // store local velocity before rotation
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);

        // aim the player at the new look vector
        Vector3 newAimVector = Vector3.ProjectOnPlane(_relativeFacing, _newUpDir);

        // LookAt can take the surface normal
        transform.LookAt(transform.position + newAimVector.normalized, _newUpDir);

        // reset camera pitch to parallel with surface
        currentCamRotX = 0f;

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
        cam.GetComponent<PlayerKnifeController>().HideKnife();
    }

    private void UnHide()
    {
        GetComponent<Renderer>().enabled = true;
        GetComponent<CapsuleCollider>().enabled = true;
        cam.GetComponent<PlayerKnifeController>().UnHideKnife();
    }

    public bool IsFrozen()
    {
        return frozen;
    }

    public bool IsOnGround()
    {
        return JumpCollider.IsColliding();
    }

    public static float VelMod()
    {
        return velMod;
    }

    /*
     * Changes the player's current health
     * Positive numbers increase, negative decrease
     */
    public void ModifyHealth(float _changeAmt)
    {
        healthEnergy.ModifyHealth(_changeAmt);
    }

    public void ModifyEnergy(float _changeAmt)
    {
        healthEnergy.ModifyEnergy(_changeAmt);
    }

    public float GetHealthNormalised()
    {
        return healthEnergy.GetHealthNormalised();
    }

    public float GetEnergyNormalised()
    {
        return healthEnergy.GetEnergyNormalised();
    }
}
