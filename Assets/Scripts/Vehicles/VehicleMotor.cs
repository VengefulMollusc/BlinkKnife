using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VehicleMotor : MonoBehaviour
{
    public Transform cameraTransform;

    public abstract void MovementInput(Vector2 input, bool boosting);

    // TODO: build controller-switcher
}
