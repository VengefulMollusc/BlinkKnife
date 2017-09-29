using UnityEngine;
using System.Collections;

public class UtiliseGravity : MonoBehaviour {

    [SerializeField]
    private bool reverseGravityEffect = false;

    private Rigidbody rb;

    /*
     * This could have a lot more options,
     * multiplication factor for strength
     * torque rather than velocity?
     * 
     * start and end positions?
     * 
     * axis/rotation lock achieved by Rigidbody settings
     * 
     * settings for kinematic?
     * make sure not effected by hitting player model?
     * 
     * Only affected when player parented to?
     * Stand on level to move?
     * stand on edge of cog to rotate?
     * 
     * Shouldn't need dynamic collisions as won't be moving too fast (so far)
     */

	void Start () {
        rb = GetComponent<Rigidbody>();

        Debug.Log("No Rigidbody found (UtiliseGravity)");

        // Just in case
        rb.useGravity = false;
	}
	

	void FixedUpdate () {
        // apply gravity
        Vector3 currentGravity;
        if (reverseGravityEffect)
        {
            currentGravity = GlobalGravityControl.GetCurrentGravityUpVector();
        } else
        {
            currentGravity = GlobalGravityControl.GetCurrentGravityDownVector();
        }
        
        float gravityStrength = GlobalGravityControl.GetGravityStrength();
        rb.AddForce(currentGravity * gravityStrength, ForceMode.Acceleration);
	}
}
