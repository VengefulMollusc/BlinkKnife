﻿using UnityEngine;

public class DoubleJumpAbility : Ability
{
    /*
     * Allows the player to perform a second jump in midair
     */
    private const string displayName = "Double Jump";
    private const KeyCode jumpKey = KeyCode.Space;
    private const float doubleJumpStrength = 12f;

    private bool hasGrounded;
    private PlayerMotor playerMotor;

    void Start()
    {
        playerMotor = transform.parent.GetComponent<PlayerMotor>();
        hasGrounded = true;
    }
    
    void Update()
    {
        if (playerMotor.IsOnGround())
        {
            hasGrounded = true;
            return;
        }

        // attempt double jump
        if (hasGrounded && Input.GetKeyDown(jumpKey) && playerMotor.CanJump(true))
        {
            playerMotor.Jump(doubleJumpStrength, true);
            hasGrounded = false;
        }
    }

    public override string GetDisplayName()
    {
        return displayName;
    }
}