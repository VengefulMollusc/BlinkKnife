﻿using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{


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

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {

        // Gets vertical and horizontal input vectors
        float xMov = Input.GetAxisRaw(xMovAxis);
        float zMov = Input.GetAxisRaw(zMovAxis);

        // apply backwards movement limit
        zMov = Mathf.Clamp(zMov, -backMoveMax, 1.0f);

        // multiply local directions by current movement values
        Vector3 movHorizontal = transform.right * xMov;
        Vector3 movVertical = transform.forward * zMov;

        // final movement vector
        Vector3 velocity = movHorizontal + movVertical;
        if (velocity.magnitude > 1.0f)
            velocity.Normalize();
        velocity = velocity * speed;

        // get sprinting boolean
        bool sprinting = (zMov > sprintThreshold && Input.GetButton(sprintButton));

        // Pass movement variables to PlayerMotor
        motor.Move(velocity, sprinting);

        // calculate rotation as 3d vector: for turning on y axis
        float yRot = Input.GetAxisRaw(xLookAxis) * Time.deltaTime;
        Vector3 rotation = new Vector3(0.0f, yRot, 0.0f) * lookSensitivity;

        // calculate camera rotation as angle: for turning on x axis
        float xRot = Input.GetAxisRaw(yLookAxis) * Time.deltaTime;
        float cameraRotationX = xRot * lookSensitivity;

        // Pass rotation variables to PlayerMotor
        motor.LookRotation(rotation, cameraRotationX);

        // Check for Jump
        if (Input.GetButtonDown(jumpButton))
        {
            // jump code here, pass to motor
            motor.Jump(jumpStrength);
        }
        else if (Input.GetButton(jumpButton))
        {
            // jump button being held
            motor.JumpHold();
        }

        // Check for Crouch
        if (Input.GetButtonDown(crouchButton))
        {
            motor.SetCrouching(true);
        }
        else if (Input.GetButtonUp(crouchButton))
        {
            motor.SetCrouching(false);
        }
    }

    /*
     * Returns the base speed value
     */
    public static float Speed()
    {
        return speed;
    }

    /*
     * returns the sprint modifier
     */
    public static float SprintModifier()
    {
        return sprintModifier;
    }

}
