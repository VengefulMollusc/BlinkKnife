using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverAbility : Ability
{
    /*
     * Allows the player to hover in midair for a short time
     */
    private const string displayName = "Hover";
    private const KeyCode hoverKey = KeyCode.LeftControl;
    private const float hoverTime = 1.5f;
    private const float forceMod = 15f;

    private bool hasGrounded;
    private PlayerMotor playerMotor;
    private Rigidbody playerRb;

    void Start()
    {
        playerMotor = GetComponent<PlayerMotor>();
        playerRb = GetComponent<Rigidbody>();
        hasGrounded = true;
    }

    void Update()
    {
        if (playerMotor.IsOnGround())
        {
            if (!hasGrounded)
            {
                hasGrounded = true;
                StopCoroutine("HoverCoroutine");
            }
            return;
        }

        // attempt hover
        if (hasGrounded && Input.GetKeyDown(hoverKey) && playerMotor.CanJump(true))
        {
            // Start hover coroutine
            StartCoroutine("HoverCoroutine");
            hasGrounded = false;
        }
    }

    // TODO: STOP HOVER ON BOOST??

    /*
     * Performs hover.
     * 
     * Adds acceleration force to cancel out downward player movement
     */
    private IEnumerator HoverCoroutine()
    {
        float t = hoverTime;
        Vector3 vel;
        Vector3 up;
        while (t > 0f)
        {
            t -= Time.deltaTime;

            // Hover logic
            vel = playerRb.velocity;
            up = playerRb.transform.up;

            if (Vector3.Dot(vel, up) < 0)
            {
                Vector3 upVel = Vector3.Project(vel, up);
                playerRb.AddForce(-upVel * forceMod, ForceMode.Acceleration);
            }

            yield return 0;
        }
    }

    public override string GetDisplayName()
    {
        return displayName;
    }
}
