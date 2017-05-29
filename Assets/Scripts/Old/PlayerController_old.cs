using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(PlayerMotor_old))]
public class PlayerController_old : MonoBehaviour {

	[SerializeField]
	private float speed = 5.0f;
	[SerializeField]
	private float lookSensitivity = 3.0f;

//	[SerializeField]
//	private float jumpStrength = 100f;

	[SerializeField]
	private float thrusterForce = 1000.0f;

	[SerializeField]
	private float thrusterFuelBurnSpeed = 1f;
	[SerializeField]
	private float thrusterFuelRegenSpeed = 0.3f;
	private float thrusterFuelAmount = 1f;

	[SerializeField]
	private LayerMask environmentMask;

	[Header("Spring Options")]
	[SerializeField]
	private float jointSpring = 20f;
	[SerializeField]
	private float jointMaxForce = 40f;

	// controller axis settings
	[Header("Control Options")]
	[SerializeField]
	private string xMovAxis = "Horizontal";
	[SerializeField]
	private string zMovAxis = "Vertical";
	[SerializeField]
	private string xLookAxis = "LookX";
	[SerializeField]
	private string yLookAxis = "LookY";
	[SerializeField]

	// component caching
	private PlayerMotor_old motor;
	private ConfigurableJoint joint;
	private Animator animator;

	void Start(){
		motor = GetComponent<PlayerMotor_old> ();
		joint = GetComponent<ConfigurableJoint> ();
		animator = GetComponent<Animator> ();

		SetJointSettings (jointSpring);
	}

	void Update (){
		// setting spring target position, lets you jump on objects properly
		RaycastHit _hit;
//		if (Physics.Raycast (transform.position, Vector3.down, out _hit, 100f, environmentMask)) {
		if (Physics.SphereCast (transform.position, 0.5f, Vector3.down, out _hit, 100f, environmentMask)) {
			joint.targetPosition = new Vector3 (0f, -_hit.point.y, 0f);
		} else {
			joint.targetPosition = new Vector3 (0f, 0f, 0f);
		}

		// calculate movement as 3d vector
		float xMov = Input.GetAxis (xMovAxis);
		float zMov = Input.GetAxis (zMovAxis);

		// multiply local directions by current movement values
		Vector3 movHorizontal = transform.right * xMov;
		Vector3 movVertical = transform.forward * zMov;

		// final movement vector
		Vector3 velocity = movHorizontal + movVertical;
		if (velocity.magnitude > 1.0f)
			velocity.Normalize ();
		velocity = velocity * speed;

		// animate movement
		animator.SetFloat ("ForwardVelocity", zMov);

		// apply movement
		motor.Move (velocity);

		// calculate rotation as 3d vector: for turning on y axis
		float yRot = Input.GetAxisRaw (xLookAxis);

		Vector3 rotation = new Vector3 (0.0f, yRot, 0.0f) * lookSensitivity;

		// apply rotation
		motor.Rotate (rotation);

		// calculate camera rotation as 3d vector: for turning on x axis
		float xRot = Input.GetAxisRaw (yLookAxis);

		float cameraRotationX = xRot * lookSensitivity;

		// apply rotation
		motor.RotateCamera (cameraRotationX);

		// calculate and apply thruster force - activated from either jump or boost button
		Vector3 _thrustDirection = Vector3.zero;
		if ((Input.GetButton ("Boost") || Input.GetButton ("Jump")) && thrusterFuelAmount > 0f){
			thrusterFuelAmount -= thrusterFuelBurnSpeed * Time.deltaTime;
			if (thrusterFuelAmount >= 0.01f) {
				// apply thrust direction
				_thrustDirection = (Vector3.up * 0.5f) + velocity;

				SetJointSettings (0f);
			}


		} else {
			thrusterFuelAmount += thrusterFuelRegenSpeed * Time.deltaTime;

			SetJointSettings (jointSpring);
		}

		if (_thrustDirection != Vector3.zero){
			// normalise thrust vector and set to force amount
			_thrustDirection.Normalize();
			_thrustDirection = _thrustDirection * thrusterForce;
		}

		thrusterFuelAmount = Mathf.Clamp (thrusterFuelAmount, 0f, 1f);

		motor.ApplyThruster (_thrustDirection);

		// jump code
//		if (Input.GetButtonDown ("Jump")) {
//			// jump code here, pass to motor
//			motor.Jump(jumpStrength);
//		} 
	}

	private void SetJointSettings(float _jointSpring){
		joint.yDrive = new JointDrive {positionSpring = _jointSpring, 
			maximumForce = jointMaxForce};
	}

	public float GetThrusterFuelAmount (){
		return thrusterFuelAmount;
	}
}
