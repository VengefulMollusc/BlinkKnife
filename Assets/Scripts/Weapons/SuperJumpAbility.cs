using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperJumpAbility : Ability
{
    private float superJumpStrength = 30f;
    private PlayerMotor playerMotor;

    void Start()
    {
        playerMotor = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMotor>();
    }

    public override void Activate()
    {
        playerMotor.Jump(superJumpStrength);
    }
}
