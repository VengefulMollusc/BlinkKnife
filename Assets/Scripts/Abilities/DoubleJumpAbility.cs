using UnityEngine;

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
        playerMotor = GetComponent<PlayerMotor>();
        hasGrounded = true;
    }

    /*
     * Allows Double Jump if player has grounded since the last jump
     */
    void Update()
    {
        if (playerMotor.IsOnGround())
        {
            if (!hasGrounded)
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
