using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverBikeController : MonoBehaviour
{
    public InputSettings inputSettings;
    public HoverMotor motor;
    //public Transform cameraPositionTransform;

    private Transform cameraTransform;
    //private const float cameraRotLimit = 90f;
    //private float currentCamRotX;

    void Start()
    {
        //cameraTransform = transform.GetChild(0);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        float xMov = Input.GetAxisRaw(inputSettings.xMovAxis);
        float zMov = Input.GetAxisRaw(inputSettings.zMovAxis);

        motor.Move(xMov, zMov);

        if (Input.GetButtonDown(inputSettings.sprintButton))
            motor.Boost(true);
        if (Input.GetButtonUp(inputSettings.sprintButton))
            motor.Boost(false);

        float xCam = Input.GetAxisRaw(inputSettings.xLookAxis);
        float yCam = Input.GetAxisRaw(inputSettings.yLookAxis);

        motor.Turn(xCam, yCam);

        //MoveCamera(new Vector2(xCam, yCam) * inputSettings.lookSensitivity);
    }

    //void LateUpdate()
    //{
    //    transform.position = cameraPositionTransform.position;
    //}

    //void MoveCamera(Vector2 input)
    //{
    //    // calculate rotation as 3d vector: for turning on y axis
    //    float yRot = input.x * Time.deltaTime;
    //    Vector3 rotation = new Vector3(0.0f, yRot, 0.0f);

    //    // calculate camera rotation as angle: for turning on x axis
    //    float xRot = input.y * Time.deltaTime;

    //    // Rotate player for horizontal camera movement
    //    transform.rotation *= Quaternion.Euler(rotation);

    //    // rotation calculation - clamps to limit values
    //    currentCamRotX -= xRot;
    //    currentCamRotX = Mathf.Clamp(currentCamRotX, -cameraRotLimit, cameraRotLimit);

    //    // apply rotation to transform of camera
    //    cameraTransform.localEulerAngles = new Vector3(currentCamRotX, 0f, 0f);
    //}
}
