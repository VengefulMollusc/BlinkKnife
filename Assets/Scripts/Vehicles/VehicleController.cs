using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    public InputSettings inputSettings;
    public VehicleMotor motor;
    public Transform cameraPositionTransform;

    private Transform cameraTransform;
    private const float cameraRotLimit = 90f;
    private float currentCamRotX;

    void Start()
    {
        cameraTransform = transform.GetChild(0);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        float xMov = Input.GetAxisRaw(inputSettings.xMovAxis);
        float zMov = Input.GetAxisRaw(inputSettings.zMovAxis);
        bool boosting = Input.GetButton(inputSettings.sprintButton);

        motor.MovementInput(new Vector2(xMov, zMov), boosting);

        float xCam = Input.GetAxisRaw(inputSettings.xLookAxis) * inputSettings.lookSensitivity;
        float yCam = Input.GetAxisRaw(inputSettings.yLookAxis) * inputSettings.lookSensitivity;

        MoveCamera(new Vector2(xCam, yCam));
    }

    void LateUpdate()
    {
        transform.position = cameraPositionTransform.position;
    }

    void MoveCamera(Vector2 input)
    {
        // calculate rotation as 3d vector: for turning on y axis
        float yRot = input.x * Time.deltaTime;
        Vector3 rotation = new Vector3(0.0f, yRot, 0.0f);

        // calculate camera rotation as angle: for turning on x axis
        float xRot = input.y * Time.deltaTime;

        // Rotate player for horizontal camera movement
        transform.rotation *= Quaternion.Euler(rotation);

        // rotation calculation - clamps to limit values
        currentCamRotX -= xRot;
        currentCamRotX = Mathf.Clamp(currentCamRotX, -cameraRotLimit, cameraRotLimit);

        // apply rotation to transform of camera
        cameraTransform.localEulerAngles = new Vector3(currentCamRotX, 0f, 0f);
    }
}
