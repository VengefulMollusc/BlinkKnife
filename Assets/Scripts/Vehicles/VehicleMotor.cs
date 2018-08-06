using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VehicleMotor : MonoBehaviour
{
    public Transform cameraTransform;

    public abstract void MovementInput(Vector2 input);

    public abstract void CameraInput(Vector2 input);

    // TODO: Add methods for jump/sprint/knife inputs etc.
    // TODO: need to revamp control system first though
}
