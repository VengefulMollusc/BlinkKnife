using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VehicleMotor : MonoBehaviour
{
    public Transform cameraTransform;

    public abstract void MovementInput(Vector2 input, bool boosting);

    public abstract void CameraInput(Vector2 input);

    // TODO: build controller-switcher
}
