using AssemblyCSharp;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private float cameraRotLimit = 85f;

    [SerializeField]
    private GameObject transitionCameraPrefab;

    //[SerializeField]
    //private float gravity = 0.3f;

    //[SerializeField]
    //private float sprintDeceleration = 0.9f;

    [SerializeField]
    private float velMod = 2f;

    [SerializeField]
    private float airVelMod = 0.02f;

    // Shift gravity if the difference between current gravity and surface normal is
    // above the threshold defined by warpGravShiftAngle
    [SerializeField]
    private float warpGravShiftAngle = 1f;

    [SerializeField]
    [Range(0f, 20f)]
    private float airVelThreshold = 20f;

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

    private Rigidbody rb;

    private Vector3 cameraRelativePos;

    private Vector3 currentGravVector;
    private float currentGravStrength;

    private float viewShiftSpeed;
    private const float maxViewShiftSpeed = 4f;
    private const float viewShiftStrengthMax = 35f;

    private HealthController healthEnergy;

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

        //if (sliding)
        //    return;

        float localXVelocity = Vector3.Dot(rb.velocity, transform.right);
        float localYVelocity = Vector3.Dot(rb.velocity, transform.up);
        float localZVelocity = Vector3.Dot(rb.velocity, transform.forward);

        if (UseGroundMovement() && jumpTimer <= 0)
        {
            // grounded
            //GroundMovement(localYVelocity);
            NewGroundMovement();
        }
        else if (!colliding)
        {
            // airborne
            AirMovement(localXVelocity, localYVelocity, localZVelocity);
        }
    }

    private bool UseGroundMovement()
    {
        return (IsOnGround() && GetSlopeAngle() < PlayerCollisionController.slideThreshold) || !sliding ;
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

    void NewGroundMovement()
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

            // MomentumSlide here
            newVel = MomentumSlide(newVel);

            rb.velocity = newVel;
        }
        else
        {
            rb.velocity -= (Vector3.ProjectOnPlane(rb.velocity, transform.up) * 0.1f);
        }
    }

    //void GroundMovement(float localYVelocity)
    //{
    //    Vector3 newVel = rb.velocity;

    //    if (velocity != Vector3.zero)
    //    {
    //        newVel = velocity * velMod;

    //        if (sprinting)
    //            newVel *= PlayerController.SprintModifier();

    //        if (jumpTimer <= 0)
    //        {
    //            // rotate to face ground normals
    //            float rayDistance = 0.5f;
    //            RaycastHit hitInfo;

    //            Ray ray = new Ray(transform.position - transform.up, -transform.up);
    //            if (Physics.Raycast(ray, out hitInfo, rayDistance))
    //            {
    //                if (hitInfo.normal != transform.up)
    //                {
    //                    // rotate input vector to align with surface normal
    //                    Quaternion rot = Quaternion.FromToRotation(transform.up, hitInfo.normal);
    //                    newVel = rot * newVel;

    //                    //if (sliding)
    //                    //{
    //                    //    Vector3 flatNormal = Vector3.ProjectOnPlane(hitInfo.normal, transform.up);

    //                    //    if (Vector3.Dot(newVel, flatNormal) < 0)
    //                    //    {
    //                    //        // don't allow movement up slope
    //                    //        newVel -= Vector3.Project(newVel, flatNormal);
    //                    //    }
    //                    //}


    //                    //// Decide here whether to rotate player as well
    //                    //float surfaceAngleDiff = Vector3.Angle(hitInfo.normal, transform.up);
    //                    //if (surfaceAngleDiff < slideAngleThreshold)
    //                    //{
    //                    //    // needs to be replaced by a proper value
    //                    //    // may also cause weird behaviour when transitioning on curved surfaces

    //                    //    // possibly need to modify velocity if surface normal above threshold?
    //                    //    // stop sticking to walls when too steep
    //                    //    Quaternion rot = Quaternion.FromToRotation(transform.up, hitInfo.normal);
    //                    //    newVel = rot * newVel;
    //                    //}
    //                    //else
    //                    //{
    //                    //    Vector3 flatNormal = Vector3.ProjectOnPlane(hitInfo.normal, transform.up);

    //                    //    if (Vector3.Dot(newVel, flatNormal) < 0)
    //                    //    {
    //                    //        // don't allow movement up slope
    //                    //        newVel -= Vector3.Project(newVel, flatNormal);
    //                    //    }
    //                    //    //newVel = rb.velocity;
    //                    //}
    //                }
    //            }

    //            //newVel = MomentumSlide(newVel);
    //        }
    //        else
    //        {
    //            // TODO: figure out why the following TODO is here...

    //            // TODO:
    //            newVel += (transform.up * localYVelocity);
    //        }

    //        if (crouching)
    //        {
    //            crouchVelFactor = 0.5f;
    //            newVel *= crouchVelFactor;
    //        }

    //        //newVel = MomentumSlide(newVel);

    //        rb.velocity = newVel;
    //    }
    //    else
    //    {
    //        // no input vector
    //        // needs to dampen movement along local xz axes
    //        // newVel = transform.up * localYVelocity;

    //        newVel = (Vector3.ProjectOnPlane(newVel, transform.up) * 0.9f) + transform.up * localYVelocity;

    //        //newVel = MomentumSlide(Vector3.ProjectOnPlane(newVel, transform.up), 0f);

    //        //newVel += transform.up * localYVelocity; // makes stationary jumps much higher

    //        rb.velocity = newVel;
    //    }
    //}

    /*
     * while moving above base speed, apply small slowdown until at base speed
     */
    private Vector3 MomentumSlide(Vector3 _newVel)
    {

        float speedThreshold = PlayerController.Speed() * velMod * PlayerController.SprintModifier();

        if (rb.velocity.magnitude > speedThreshold)
        {

            if (Vector3.Dot(_newVel, rb.velocity) > 0)
            {
                // input in direction of movement
                //_newVel = _newVel.normalized * rb.velocity.magnitude;
                //float forwardComponent = Vector3.Project(_newVel, rb.velocity).magnitude;
                //float newMagnitude = Utilities.MapValues(forwardComponent, 0f, speedThreshold, speedThreshold,
                //    rb.velocity.magnitude, true);

                _newVel = _newVel.normalized * rb.velocity.magnitude;
            }
        }

        return _newVel;
    }


    /*
     * Handle movement changes while midair
     */
    void AirMovement(float localXVelocity, float localYVelocity, float localZVelocity)
    {

        if (velocity != Vector3.zero)
        {
            HandleMidairInput(localXVelocity, localYVelocity, localZVelocity);
        }

        if (crouching && canHover)
        {
            canHover = false;
            StartCoroutine(Hover());
        }
    }

    /*
     * Split this off as it's a complicated scenario
     * Needs to allow limited velocity adjustment while midair, 
     * without increasing speed over a certain threshold
     * 
     * needs to allow limited slowing and turning
     * 
     * needs to feel like you have SOME control while
     * still being dedicated to momentum of jump
     */
    private void HandleMidairInput(float localXVelocity, float localYVelocity, float localZVelocity)
    {
        Vector3 flatVel = Vector3.ProjectOnPlane(rb.velocity, transform.up);

        Vector3 velocityTemp = velocity * airVelMod;

        // if input velocity in direction of flight and velocity above threshold
        if (Vector3.Dot(velocityTemp, flatVel) > 0 && flatVel.magnitude > airVelThreshold)
        {
            // cancel positive movement in direction of flight
            velocityTemp -= Vector3.Project(velocityTemp, flatVel);
        }

        rb.AddForce(velocityTemp, ForceMode.Impulse);
    }

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

    public void WarpToKnife(bool _shiftGravity, Vector3 _velocity, KnifeController _knifeController)
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
        rb.MovePosition(newPos);

        Vector3 camEndPos = newPos + (transform.rotation * cameraRelativePos); //Needs to get position camera will be in after move
        Quaternion camEndRot = camStartRot;
        if (gravityShift)
        {
            camEndRot = transform.rotation;
        }

        // INHERITED VELOCITY MUST BE RELATIVE TO PLAYER DIRECTION

        if (_velocity != Vector3.zero)
        {
            // adding magnitude here allows cumulative velocity gain
            // dot product to get component of velocity in direction of travel
            float projectedVelMagnitude = Vector3.Dot(rb.velocity, _velocity);
            projectedVelMagnitude = Mathf.Max(projectedVelMagnitude, 0f) * 0.5f; // 0.5f here controls how much velocity is added
            // adds component of current velocity along axis of knife movement

            // TODO: add vel here? or add component to initial throw velocity of knife?
            rb.velocity = (_velocity * warpVelocityModifier) + (_velocity.normalized * projectedVelMagnitude);

            // fixes horizontal momentum lock when warping
            //onGround = false;
        }
        else if (_knifeController.HasCollided())
        {
            // cancel momentum
            rb.velocity = Vector3.zero;
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
