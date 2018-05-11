using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpDashAbility : Ability
{
    /*
     * Allows the player to perform a dash while in midair.
     * Uses boostRing notification to apply dash
     */
    private const string displayName = "Jump Dash";
    private const KeyCode jumpKey = KeyCode.Space;
    private const float dashStrength = 4f;

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
            // Boost player in direction of input vector
            Vector3 inputVel = playerMotor.GetInputVelocity();
            if (inputVel == Vector3.zero)
                return;

            Info<GameObject, Vector3> info = new Info<GameObject, Vector3>(playerMotor.gameObject, inputVel * dashStrength);
            this.PostNotification(BoostRing.BoostNotification, info);

            hasGrounded = false;
        }
    }

    public override string GetDisplayName()
    {
        return displayName;
    }
}
