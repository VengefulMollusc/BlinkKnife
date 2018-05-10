using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperJumpAbility : MonoBehaviour, Ability
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

    public void Activate()
    {
        if (cooldown <= 0f && playerMotor.CanJump())
        {
            playerMotor.Jump(superJumpStrength);
            cooldown = cooldownTime;
        }
    }

    public void EndActivation()
    {
        
    }

    public string GetDisplayName()
    {
        return displayName;
    }
}
