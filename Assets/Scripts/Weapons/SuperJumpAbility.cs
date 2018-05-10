using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperJumpAbility : Ability
{
    private string displayName = "Rocket Jump";
    private float superJumpStrength = 30f;
    private float cooldownTime = 1f;

    private PlayerMotor playerMotor;
    private float cooldown;

    void Start()
    {
        playerMotor = transform.parent.GetComponent<PlayerMotor>();
    }

    void Update()
    {
        if (cooldown > 0f)
            cooldown -= Time.deltaTime;
    }

    public override void Activate()
    {
        if (cooldown <= 0f && playerMotor.CanJump())
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
