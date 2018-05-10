using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour {

    
	private static float speed = 5.0f;
    private static float sprintModifier = 1.75f;
    private static float sprintThreshold = 0.45f;

    [Header("General Settings")]
    [SerializeField]
    [Range(0f, 1f)]
    private float backMoveMax = 0.9f;
	[SerializeField]
	private float lookSensitivity = 3.0f;
	[SerializeField]
	private float jumpStrength = 100f;

	// controller axis settings
	[Header("Control Settings")]
	[SerializeField]
	private string xMovAxis = "Horizontal";
	[SerializeField]
	private string zMovAxis = "Vertical";
	[SerializeField]
	private string xLookAxis = "LookX";
	[SerializeField]
	private string yLookAxis = "LookY";
	[SerializeField]
	private string jumpButton = "Jump";
	[SerializeField]
	private string sprintButton = "Sprint";
    [SerializeField]
    private string crouchButton = "Crouch";

    private PlayerMotor motor;

	void Start (){
		motor = GetComponent<PlayerMotor> ();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	void Update (){

		// calculate movement as 3d vector
		float xMov = Input.GetAxisRaw (xMovAxis); // USE GetAxisRaw for unsmoothed input
		float zMov = Input.GetAxisRaw (zMovAxis); // GetAxis for smoothed

        // apply backwards movement limit
        zMov = Mathf.Clamp(zMov, -backMoveMax, 1.0f);

		// multiply local directions by current movement values
		Vector3 movHorizontal = transform.right * xMov;
		Vector3 movVertical = transform.forward * zMov;

		// final movement vector
		Vector3 velocity = movHorizontal + movVertical;
		if (velocity.magnitude > 1.0f)
			velocity.Normalize ();
		velocity = velocity * speed;

        // apply sprint speed
  //      bool sprinting = false;
  //      if (zMov > sprintThreshold && Input.GetButton(sprintButton))
  //      {
  //          velocity = velocity * sprintModifier;
  //          sprinting = true;
		//}

	    bool sprinting = (zMov > sprintThreshold && Input.GetButton(sprintButton));

        // apply movement
        motor.Move (velocity, sprinting);

		// calculate rotation as 3d vector: for turning on y axis
		float yRot = Input.GetAxisRaw (xLookAxis) * Time.deltaTime;

		Vector3 rotation = new Vector3 (0.0f, yRot, 0.0f) * lookSensitivity;

		// apply rotation
		//motor.Rotate (rotation);

		// calculate camera rotation as 3d vector: for turning on x axis
		float xRot = Input.GetAxisRaw (yLookAxis) * Time.deltaTime;

		float cameraRotationX = xRot * lookSensitivity;

		// apply rotation
		//motor.RotateCamera (cameraRotationX);

	    motor.LookRotation(rotation, cameraRotationX);

		// jump code
		if (Input.GetButtonDown (jumpButton)) {
			// jump code here, pass to motor
			motor.Jump(jumpStrength);
		}
	    if (Input.GetButton(jumpButton))
	    {
	        // jump button being held
	        motor.JumpHold();
	    }

        // crouch code
        if (Input.GetButtonDown(crouchButton))
        {
            motor.SetCrouching(true);
        } else if (Input.GetButtonUp(crouchButton))
        {
            motor.SetCrouching(false);
        }


        // Test controls for health/energy
        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    float mod = Random.Range(-2f, -30f);
        //    motor.ModifyHealth(mod);
        //    motor.ModifyEnergy(mod);
        //    Debug.Log("Health/Energy decreased: " + mod);
        //}
        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    float mod = Random.Range(2f, 30f);
        //    motor.ModifyHealth(mod);
        //    motor.ModifyEnergy(mod);
        //    Debug.Log("Health/Energy increased: " + mod);
        //}

    }

    public static float Speed()
    {
        return speed;
    }

    public static float SprintModifier()
    {
        return sprintModifier;
    }

}
