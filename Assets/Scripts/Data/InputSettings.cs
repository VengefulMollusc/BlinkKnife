using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class InputSettings : ScriptableObject
{
    [Header("Movement Control Settings")]
    public string xMovAxis = "Horizontal";
    public string zMovAxis = "Vertical";
    public string xLookAxis = "LookX";
    public string yLookAxis = "LookY";
    public string jumpButton = "Jump";
    public string sprintButton = "Sprint";
    public string crouchButton = "Crouch";

    [Header("Knife Control Settings")]
    public string leftMouse = "Fire1";
    public string rightMouse = "Fire2";
    public string middleMouse = "Fire3";

    [Header("Constants")]
    public float lookSensitivity = 100f;
}
