using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperJumpAbility : Ability
{
    private KeyCode jumpKey = KeyCode.E;
    private string displayName = "Rocket Jump";
    private float superJumpStrength = 30f;
    private float cooldownTime = 1f;

    private PlayerMotor playerMotor;
    private float cooldown;

    void Start()
    {
        playerMotor = transform.parent.GetComponent<PlayerMotor>();
    }

    public override void Activate()
    {
        base.Activate();
        cooldown = 0f;
    }

    void Update()
    {
        if (Input.GetKeyDown(jumpKey) && cooldown <= 0f)
        {
            PerformJump();
        }

        if (cooldown > 0f)
            cooldown -= Time.deltaTime;
    }

    void PerformJump()
    {
        if (playerMotor.CanJump())
        {
            playerMotor.Jump(superJumpStrength);
            cooldown = cooldownTime;
        }
    }

    public override string GetDisplayName()
    {
        return displayName;
    }
}
