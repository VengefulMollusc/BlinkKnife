using AssemblyCSharp;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour {

	[SerializeField]
	private Camera cam;

    [SerializeField]
    private float cameraRotLimit = 85f;

    [SerializeField]
    private GameObject transitionCameraPrefab;

    //[SerializeField]
    //private float gravity = 0.3f;

    [SerializeField]
    private float sprintDeceleration = 0.9f;

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
    private float maxAirLowerBound = 20f;

    [SerializeField]
    [Range(0.0f, 50.0f)]
    private float warpVelocityModifier = 20f;


    //[SerializeField]
    //[Range(0f, 45f)]
    //private float gravityShiftAngleMax = 15f;

    public const string WarpNotification = "PlayerMotor.WarpNotification";
    public const string GravityWarpNotification = "PlayerMotor.GravityWarpNotification";

    private bool frozen = false;
    private Vector3 frozenPos = Vector3.zero;
    private Vector3 frozenVel = Vector3.zero;

	private bool onGround = false;
	private bool colliding = false;

    private bool crouching = false;
    private float crouchVelFactor = 1f;

    private Vector3 velocity = Vector3.zero;
	private Vector3 rotation = Vector3.zero;
    private bool sprinting = false;
	private float cameraRotationX = 0f;
	private float currentCamRotX = 0f;

	private int jumpTimer = 0;

    private float maxAirMagnitude = 0.0f;

    private Rigidbody rb;

    private Vector3 cameraRelativePos;

    //[SerializeField]
    //private float healthMax = 100f;
    //private float health;
    //[SerializeField]
    //private float healthRechargeDelay = 5f;
    //private float healthRechargeCounter = 0f;
    //[SerializeField]
    //private float healthRechargeRate = 10f;

    //[SerializeField]
    //private float energyMax = 100f;
    //private float energy;
    //[SerializeField]
    //private float energyRechargeDelay = 1f;
    //private float energyRechargeCounter = 0f;
    //[SerializeField]
    //private float energyRechargeRate = 30f;

    private HealthController healthEnergy;

    void Awake(){
        healthEnergy = GetComponent<HealthController>();
    }

    void Start (){
		rb = GetComponent<Rigidbody> ();
        if (transitionCameraPrefab == null)
        {
            throw new MissingReferenceException("No transitionCameraPrefab in PlayerMotor");
        }

        cameraRelativePos = cam.transform.position - transform.position;
        //health = healthMax;
        //energy = energyMax;
    }

	// gets movement vector from PlayerController
	public void Move (Vector3 _velocity, bool _sprinting){
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
	public void Rotate (Vector3 _rotation){
        if (frozen)
        {
            rotation = Vector3.zero;
            return;
        }

        rotation = _rotation;
	}

	// gets camera rotation vector 
	public void RotateCamera (float _cameraRotationX){
        if (frozen)
        {
            cameraRotationX = 0f;
            return;
        }

        cameraRotationX = _cameraRotationX;
	}

    void Update(){
        //RechargeEnergy();
        //RechargeHealth();
    }

    // Run every physics iteration
    void FixedUpdate(){
        // if frozen, dont perform any movement
		if (!frozen) {
			PerformMovement ();
			onGround = false;
			colliding = false;
			jumpTimer--;
		}

        // Freezing needs to stop rotation too
		PerformRotation();

        //Debug.Log(rb.velocity.magnitude);
    }

//	private void KeepGrounded(){
//		RaycastHit hit;
//		if (Physics.SphereCast(transform.position, 0.5f, -transform.up, out hit, 2f)){
////			Debug.Log ("Grounding");
//			//rb.MovePosition (transform.position + new Vector3(0.0f, -hit.distance, 0.0f));
//            rb.MovePosition(transform.position - (transform.up * hit.distance));
//            rb.velocity = transform.rotation * new Vector3 (rb.velocity.x, 0.0f, rb.velocity.z);
//		}
//	}

	// perform movement based on velocity variable
	private void PerformMovement(){
        
        //rb.velocity += Vector3.down * gravity;
        rb.AddForce(-transform.up * GlobalGravityControl.GetGravityStrength(), ForceMode.Acceleration);

        float localXVelocity = Vector3.Dot(rb.velocity, transform.right);
        float localYVelocity = Vector3.Dot(rb.velocity, transform.up);
        float localZVelocity = Vector3.Dot(rb.velocity, transform.forward);

        if (onGround) {

            Vector3 newVel = rb.velocity;

            if (velocity != Vector3.zero) {

//				rb.MovePosition (rb.position + (velocity * 0.1f));
//				rb.AddForce (velocity * groundVelMod, ForceMode.VelocityChange);

				// Vector3 newVel = new Vector3(velocity.x * velMod, 0.0f, velocity.z * velMod);
                newVel = velocity * velMod;

                // if sprinting and new speed less than old speed, keep old speed
                //if (sprinting && (rb.velocity.magnitude > newVel.magnitude)) {
                //    newVel = newVel.normalized * rb.velocity.magnitude * sprintDeceleration;
                //}

				if (jumpTimer <= 0) {
					// rotate to face ground normals
					float rayDistance = 0.5f;
					RaycastHit hitInfo;

					Ray ray = new Ray (transform.position - transform.up, -transform.up);
					if (Physics.Raycast (ray, out hitInfo, rayDistance)) {
						if (hitInfo.normal != transform.up) {

                            // Decide here whether to rotate player as well
                            float surfaceAngleDiff = Vector3.Angle(hitInfo.normal, transform.up);
                            if (surfaceAngleDiff < 45f)
                            {
                                // needs to be replaced by a proper value
                                // may also cause weird behaviour when transitioning on curved surfaces

                                // possibly need to modify velocity if surface normal above threshold?
                                // stop sticking to walls when too steep
                                Quaternion rot = Quaternion.FromToRotation(transform.up, hitInfo.normal);
                                newVel = rot * newVel;
                            }
						}
					}
				} else {
                    // TODO:
					newVel = newVel + (transform.up * localYVelocity);
				}

                if (crouching)
                {
                    crouchVelFactor = 0.5f;
                    newVel *= crouchVelFactor;
                }


                newVel = MomentumSlide(newVel, PlayerController.speed);

                rb.velocity = newVel;

            } else {
				// no input vector
                // needs to dampen movement along local xz axes
				//newVel = transform.up * localYVelocity;

                newVel = MomentumSlide(Vector3.ProjectOnPlane(newVel, transform.up), 0f);
                //newVel += transform.up * localYVelocity; // makes stationary jumps much higher

                rb.velocity = newVel;
            }


        } else {
            // airborne
            //Vector3 inputVel = velocity * airVelMod;

            //rb.AddForce(inputVel, ForceMode.Acceleration);

            // old code
            if (maxAirMagnitude == 0.0f)
            {
                maxAirMagnitude = new Vector2(localXVelocity, localZVelocity).magnitude;
            }

            if (maxAirMagnitude < maxAirLowerBound) maxAirMagnitude = maxAirLowerBound;

            if (velocity != Vector3.zero)
            {
                // midair position adjust
                // needs to not allow extra velocity in same direction as jump

                // add new XZ velocity to old XZ velocity
                // no Y movement at this point (makes magnitude calculations easier)
                Vector3 newVel = (velocity * airVelMod)
                    + (localXVelocity * transform.right)
                    + (localZVelocity * transform.forward);

                // magnitude of relative XZ movement
                float newVelMagnitude = newVel.magnitude;

                if (newVelMagnitude < maxAirMagnitude && maxAirMagnitude > maxAirLowerBound)
                {
                    maxAirMagnitude = newVelMagnitude;
                }
                else if (newVelMagnitude > maxAirMagnitude)
                {
                    float factor = maxAirMagnitude / newVelMagnitude;
                    newVel = newVel * factor;
                }

                // restore Y velocity
                newVel = newVel + (transform.up * localYVelocity);

                rb.velocity = newVel;
            }

            if (crouching)
            {
                rb.velocity *= crouchVelFactor;
                crouchVelFactor *= 1.01f;
                if (crouchVelFactor >= 1f)
                {
                    SetCrouching(false);
                }
            }
        }
	}

    private Vector3 MomentumSlide(Vector3 _newVel, float _baseSpeed) {
        // PERHAPS
        // slide when above certain speed (running speed? - walking speed could be interesting)?
        // - don't replace momentum, add new velocity but make sure magnitude decreases.

        // NEED TO MAKE SURE THAT VERTICAL MOMENTUM/SPEED NOT AFFECTED
        float speedThreshold = _baseSpeed * velMod;
        if (sprinting) speedThreshold *= PlayerController.sprintModifier;
        if (rb.velocity.magnitude > speedThreshold)
        {
            float speed = rb.velocity.magnitude;
            _newVel += rb.velocity;
            if (_newVel.magnitude > speed)
            {
                float newMagnitude = DampenSpeed(speed, speedThreshold);
                _newVel = _newVel.normalized * newMagnitude;
            }
        }

        return _newVel;
    }

    private float DampenSpeed(float _speed, float _baseSpeed) {
        float sprintSpeed = PlayerController.speed * PlayerController.sprintModifier * velMod;

        // REDO THIS SO:
        // Decceleration based on difference between speed and base speed
        // Perhaps increase decceleration as base speed -> 0

        // Rapid (linear?) braking if base speed 0
        // else ease down to value with decceleration decreases as speed -> base speed
        if (_speed > sprintSpeed && _baseSpeed > 0.1f) {
            float newSpeed = (_baseSpeed > 0.1f) ? _speed * 0.99f : _speed * 0.9f;
            if (newSpeed < _baseSpeed) newSpeed = _baseSpeed;

            //Debug.Log(_baseSpeed + " " + newSpeed);

            return newSpeed;
        }
        // else speed less than sprint and no input
        return Mathf.Clamp(_speed -= 2f, 0f, sprintSpeed);
        
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
	private void PerformRotation(){
		rb.MoveRotation (transform.rotation  * Quaternion.Euler (rotation));
		if (cam != null){
			// rotation calculation - clamps to limit values
			currentCamRotX -= cameraRotationX;
			currentCamRotX = Mathf.Clamp (currentCamRotX, -cameraRotLimit, cameraRotLimit);

			// apply rotation to transform of camera
			cam.transform.localEulerAngles = new Vector3 (currentCamRotX, 0f, 0f);
		}
	}

	// perform jump when triggered by PlayerController
	public void Jump(float _jumpStrength){
		if (!onGround || frozen) 
			return;
        
		// keeps slope momentum on jump
		//rb.velocity = new Vector3(rb.velocity.x, _jumpStrength, rb.velocity.z);
        rb.velocity = rb.velocity + (transform.up * _jumpStrength);
        jumpTimer = 30;
        SetCrouching(false);
		// eliminates slope momentum
//		rb.velocity = velocity + (Vector3.up * _jumpStrength);
//		onGround = false;
	}

    // either begin or end crouching
    public void SetCrouching(bool _crouching)
    {
        if (_crouching)
        {
            crouching = true;
            crouchVelFactor = 0.5f;
        } else
        {
            crouching = false;
            crouchVelFactor = 1f;
        }
    }

	public void SetOnGround (bool _onGround){
		if (jumpTimer > 0)
			return;
		onGround = _onGround;
        maxAirMagnitude = 0.0f;
    }

	private void OnCollisionStay(){
		colliding = true;
	}
    
	//public void WarpToKnife(Vector3 _position, Vector3 _velocity, GameObject _gameObject, Vector3 _surfaceNormal)
 //   {
 //       Vector3 camStartPos = cam.transform.position;
 //       Quaternion camStartRot = cam.transform.rotation;
 //       Vector3 relativeFacing = cam.transform.forward;

 //       // Shift gravity if the difference between current gravity and surface normal is
 //       // above the threshold defined by warpGravShiftAngle
 //       float surfaceDiffAngle = Vector3.Angle(transform.up, _surfaceNormal);
 //       bool gravityShift = (surfaceDiffAngle > warpGravShiftAngle && _surfaceNormal != Vector3.zero);

 //       transform.SetParent(null);

 //       // Unsure if this needs to happen before or after the moveposition

 //       // rotate to surface normal
 //       if (gravityShift)
 //       {
 //           RotateToSurface(_surfaceNormal, relativeFacing);
 //       }

 //       // move to knife position
 //       rb.MovePosition(_position);

 //       Vector3 camEndPos = _position + (transform.rotation * cameraRelativePos); //Needs to get position camera will be in after move
 //       Quaternion camEndRot = camStartRot;
 //       if (gravityShift)
 //       {
 //           camEndRot = transform.rotation;
 //       } 
        
 //       // INHERITED VELOCITY MUST BE RELATIVE TO PLAYER DIRECTION

 //       if (_velocity != Vector3.zero)
 //       {
 //           rb.velocity = _velocity;

 //           // fixes horizontal momentum lock when warping
 //           onGround = false;
 //       }

 //       // fixes horizontal momentum lock when warping
 //       onGround = false;

 //       GameObject transCamera = (GameObject)Instantiate(transitionCameraPrefab, camStartPos, cam.transform.rotation);
        
 //       TransitionCameraController transCamController = transCamera.GetComponent<TransitionCameraController>();
	//	transCamController.Setup(cam, this, camStartPos, camEndPos, camStartRot, camEndRot, _gameObject, gravityShift);
 //       float duration = transCamController.GetDuration();
 //       transCamController.StartTransition();

 //       if (gravityShift)
 //       {
 //           GlobalGravityControl.TransitionGravity(_surfaceNormal, duration);
 //       }

 //   }

    public void WarpToKnife(bool _shiftGravity, Vector3 _velocity, KnifeController _knifeController)
    {
        if (frozen) return;
        Vector3 camStartPos = cam.transform.position;
        Quaternion camStartRot = cam.transform.rotation;
        Vector3 relativeFacing = cam.transform.forward;

        Vector3 newPos = _knifeController.GetWarpPosition();
        Vector3 newGravAngle = _knifeController.GetGravVector();

        // Shift gravity if the difference between current gravity and surface normal is
        // above the threshold defined by warpGravShiftAngle
        float surfaceDiffAngle = Vector3.Angle(transform.up, newGravAngle);
        bool gravityShift = (_shiftGravity && surfaceDiffAngle > warpGravShiftAngle && newGravAngle != Vector3.zero);

        if (gravityShift)
        {
            this.PostNotification(GravityWarpNotification, _knifeController.GetObjectCollided());
        }

        transform.SetParent(null);

        // Unsure if this needs to happen before or after the moveposition

        // rotate to surface normal
        if (gravityShift)
        {
            RotateToSurface(newGravAngle, relativeFacing);
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
            rb.velocity = (_velocity * (warpVelocityModifier + rb.velocity.magnitude));

            // fixes horizontal momentum lock when warping
            onGround = false;
        }

        // fixes horizontal momentum lock when warping
        onGround = false;

        GameObject transCamera = (GameObject)Instantiate(transitionCameraPrefab, camStartPos, cam.transform.rotation);

        TransitionCameraController transCamController = transCamera.GetComponent<TransitionCameraController>();
        transCamController.Setup(cam, this, camStartPos, camEndPos, camStartRot, camEndRot, gravityShift);
        float duration = transCamController.GetDuration();
        transCamController.StartTransition();

        if (gravityShift)
        {
            GlobalGravityControl.TransitionGravity(newGravAngle, duration);
        }

    }

    // This looks like it's working,
    public void UpdateGravityDirection(Vector3 _newUp)
    {
        // might be nessecary
        transform.LookAt(transform.position + transform.forward, _newUp); //?
        // probably need similar code to RotateToSurface to align in correct direction

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
    private void RotateToSurface(Vector3 _surfaceNormal, Vector3 _relativeFacing)
    {
        // store local velocity before rotation
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);

        // aim the player at the new look vector
        Vector3 newAimVector = Vector3.ProjectOnPlane(_relativeFacing, _surfaceNormal);

        // LookAt can take the surface normal
        transform.LookAt(transform.position + newAimVector.normalized, _surfaceNormal);

        // reset camera pitch to perpendicular to surface
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

    // Health and Energy recharging
    //private void RechargeHealth()
    //{
    //    if (healthRechargeCounter > 0f)
    //    {
    //        healthRechargeCounter -= Time.deltaTime;
    //        return;
    //    }

    //    if (health >= healthMax)
    //        return;

    //    health += Time.deltaTime * healthRechargeRate;
    //}

    //private void RechargeEnergy()
    //{
    //    if (energyRechargeCounter > 0f)
    //    {
    //        energyRechargeCounter -= Time.deltaTime;
    //        return;
    //    }

    //    if (energy >= energyMax)
    //        return;

    //    energy += Time.deltaTime * energyRechargeRate;
    //}

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
