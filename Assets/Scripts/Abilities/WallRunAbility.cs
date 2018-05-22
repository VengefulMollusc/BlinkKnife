using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunAbility : Ability
{
    private const string displayName = "Wall Run";
    private const KeyCode jumpKey = KeyCode.Space;

    private Transform playerTransform;
    private PlayerMotor playerMotor;

    [SerializeField]
    private WallRunColliderController leftWallRunCollider;

    [SerializeField]
    private WallRunColliderController rightWallRunCollider;

    void Start()
    {
        playerTransform = transform.parent;
        playerMotor = playerTransform.GetComponent<PlayerMotor>();
    }

    void Update()
    {
        if (playerMotor.IsOnGround())
            return;

        if (Input.GetKey(jumpKey))
        {
            // wall run logic
            Vector3 inputVector = playerMotor.GetInputVelocity().normalized;
            float dot = Vector3.Dot(inputVector, playerTransform.right);
            if (dot < -0.5f && leftWallRunCollider.CanWallRun())
            {

            }
            else if (dot > 0.5f && rightWallRunCollider.CanWallRun())
            {

            }
        }
    }

    public override string GetDisplayName()
    {
        return displayName;
    }
}
