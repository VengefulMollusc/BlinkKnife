using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using ProBuilder2.Common;
using UnityEngine;

public class SafeWarpCollider : MonoBehaviour
{
    private KnifeController knifeController;

    private bool safeToWarp;

	// Use this for initialization
	void OnEnable ()
	{
	    knifeController = transform.parent.GetComponent<KnifeController>();
	    safeToWarp = true;
	}
	
	void FixedUpdate () {
	    // if knife has stuck, get position offset from wall
	    if (knifeController.HasStuck())
	    {
	        // Check if gravity warp
	        if (knifeController.ShiftGravity())
	        {
	            if (transform.up != -knifeController.GetGravVector())
	                transform.rotation = Quaternion.FromToRotation(Vector3.down, knifeController.GetGravVector());
	        }
	        else if (transform.up != -GlobalGravityControl.GetCurrentGravityVector())
	        {
	            transform.rotation = GlobalGravityControl.GetGravityRotation();
            }

	        transform.position = CollisionOffsetPosition();
	    }
    }

    // Uses the position and collisionNormal from the knife to calculate where the player should warp to
    private Vector3 CollisionOffsetPosition()
    {
        Vector3 collisionPos = knifeController.GetCollisionPosition();
        Vector3 collisionNormal = knifeController.GetCollisionNormal();

        if (collisionNormal == Vector3.zero)
        {
            return Vector3.zero;
        }

        Vector3 closestPointBase = transform.position;
        float dot = Vector3.Dot(transform.up, collisionNormal);

        // offset base position to use upper/lower hemispheres of collider
        if (dot > 0.001)
            closestPointBase -= (transform.up * 0.5f);
        else if (dot < -0.001)
            closestPointBase += (transform.up * 0.5f);

        Vector3 closestPointOnCollider = GetComponent<Collider>()
            .ClosestPointOnBounds(closestPointBase - collisionNormal);

        Vector3 pointDiff = closestPointOnCollider - transform.position;

        return collisionPos - pointDiff;
    }

    public bool IsSafeToWarp()
    {
        return safeToWarp;
    }

    void OnTriggerStay(Collider col)
    {
        if (col.isTrigger)
            return;

        safeToWarp = false;
    }

    void OnTriggerExit(Collider col)
    {
        if (col.isTrigger)
            return;

        safeToWarp = true;
    }
}
