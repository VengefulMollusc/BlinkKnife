using UnityEngine;

public class JumpDashAbility : Ability
{
    /*
     * Allows the player to perform an omni-directional horizontal dash while in midair.
     * 
     * Uses boostRing notification to apply dash physics.
     * Could use this notification for redirecting knife etc as well
     */
    private const string displayName = "Jump Dash";
    private const KeyCode jumpKey = KeyCode.Space;
    private const float dashStrength = 4f;

    private bool hasGrounded;
    private PlayerMotor playerMotor;

    void Start()
    {
        playerMotor = GetComponent<PlayerMotor>();
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

            // Post boost notification
            // this will also cancel gravity for a short time
            Info<GameObject, Vector3> info = new Info<GameObject, Vector3>(gameObject, inputVel * dashStrength);
            this.PostNotification(BoostRing.BoostNotification, info);

            hasGrounded = false;
        }
    }

    public override string GetDisplayName()
    {
        return displayName;
    }
}
