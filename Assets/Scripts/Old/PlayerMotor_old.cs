using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor_old: MonoBehaviour {

	[SerializeField]
	private Camera cam;

	private Vector3 velocity = Vector3.zero;
	private Vector3 rotation = Vector3.zero;
	private float cameraRotationX = 0f;
	private float currentCamRotX = 0f;
	private Vector3 thrusterForce = Vector3.zero;

	[SerializeField]
	private float cameraRotLimit = 85f;

	private Rigidbody rb;

	void Start (){
		rb = GetComponent<Rigidbody> ();
	}

	// gets movement vector from PlayerController
	public void Move (Vector3 _velocity){
		velocity = _velocity;
	}

	// gets rotation vector 
	public void Rotate (Vector3 _rotation){
		rotation = _rotation;
	}

	// gets camera rotation vector 
	public void RotateCamera (float _cameraRotationX){
		cameraRotationX = _cameraRotationX;
	}

	// gets thruster force vector 
	public void ApplyThruster (Vector3 _thrusterForce){
		thrusterForce = _thrusterForce;
	}

	// Run every physics iteration
	void FixedUpdate(){
		PerformMovement ();
		PerformRotation ();
	}

	// perform movement based on velocity variable
	private void PerformMovement(){
		if (velocity != Vector3.zero){
//			rb.MovePosition (transform.position + velocity * Time.fixedDeltaTime);
			rb.AddForce (velocity * 5f * Time.fixedDeltaTime, ForceMode.VelocityChange); // physics based?
		}

		if (thrusterForce != Vector3.zero){
			rb.AddForce (thrusterForce * Time.fixedDeltaTime, ForceMode.Acceleration);
		}
	}

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
//	public void Jump(float _jumpStrength){
//		rb.AddForce (Vector3.up * _jumpStrength, ForceMode.VelocityChange);
//	}

}
