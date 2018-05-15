using UnityEngine;

public class SuperJumpAbility : Ability
{
    /*
     * Allows the player to perform an extra-high jump
     */
    private const string displayName = "Rocket Jump";
    private const KeyCode useAbilityKey = KeyCode.E;
    private const float superJumpStrength = 30f;
    private const float cooldownTime = 1f;

    private PlayerMotor playerMotor;
    private float cooldown;

    void Start()
    {
        playerMotor = transform.parent.GetComponent<PlayerMotor>();
    }

    public override void Enable()
    {
        base.Enable();
        cooldown = 0f;
    }

    /*
     * Check if can super jump
     */
    void Update()
    {
        if (Input.GetKeyDown(useAbilityKey) && cooldown <= 0f && playerMotor.CanJump())
        {
            playerMotor.Jump(superJumpStrength);
            cooldown = cooldownTime;
        }

        if (cooldown > 0f)
            cooldown -= Time.deltaTime;
    }

    public override string GetDisplayName()
    {
        return displayName;
    }
}
