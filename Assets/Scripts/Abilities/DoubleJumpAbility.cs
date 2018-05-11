using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleJumpAbility : Ability
{
    private KeyCode jumpKey = KeyCode.Space;
    private string displayName = "Double Jump";
    private float doubleJumpStrength = 10f;

    private PlayerMotor playerMotor;

    void Start()
    {
        playerMotor = transform.parent.GetComponent<PlayerMotor>();
    }

    
    void Update()
    {
        if (playerMotor.IsOnGround())
        {
            return;
        }

        if (Input.GetKeyDown(jumpKey))
        {
            playerMotor.Jump(doubleJumpStrength, true);
        }
    }

    public override string GetDisplayName()
    {
        return displayName;
    }
}
