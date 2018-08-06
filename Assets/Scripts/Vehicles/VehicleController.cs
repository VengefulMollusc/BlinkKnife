using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    public InputSettings inputSettings;

    private VehicleMotor motor;
    
    void Start()
    {
        motor = GetComponent<VehicleMotor>();
    }
    
    void Update()
    {
        float xMov = Input.GetAxisRaw(inputSettings.xMovAxis);
        float zMov = Input.GetAxisRaw(inputSettings.zMovAxis);
        bool boosting = Input.GetButton(inputSettings.sprintButton);

        motor.MovementInput(new Vector2(xMov, zMov), boosting);

        float xCam = Input.GetAxisRaw(inputSettings.xLookAxis);
        float yCam = Input.GetAxisRaw(inputSettings.yLookAxis);

        motor.CameraInput(new Vector2(xCam, yCam));
    }
}
