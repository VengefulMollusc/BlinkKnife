using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverBikeController : MonoBehaviour
{
    public InputSettings inputSettings;
    public HoverMotor motor;

    void Start()
    {
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
    }
}
