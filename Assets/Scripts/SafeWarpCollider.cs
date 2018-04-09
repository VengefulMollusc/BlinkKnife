using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using ProBuilder2.Common;
using UnityEngine;

public class SafeWarpCollider : MonoBehaviour
{
    [SerializeField]
    private LayerMask raycastMask;

    private KnifeController knifeController;

    private bool safeToWarp;

    public const string UpdateLookAheadColliderNotification = "SafeWarpCollider.UpdateLookAheadColliderNotification";

    // Use this for initialization
    void OnEnable ()
	{
	    knifeController = transform.parent.GetComponent<KnifeController>();
	    safeToWarp = true;
	}
	
	void FixedUpdate () {
        // reset to knife position and gravity rotation
        if (transform.position != knifeController.transform.position)
            transform.position = knifeController.transform.position;

        // if knife has stuck, get position offset from wall
        if (knifeController.HasStuck())
	    {
	        // Check if gravity warp
	        if (knifeController.ShiftGravity())
	        {
                if (transform.up != -knifeController.GetGravVector())
                {
                    transform.rotation = Quaternion.FromToRotation(Vector3.down, knifeController.GetGravVector());
                }
            }
            else if (transform.up != -GlobalGravityControl.GetCurrentGravityVector())
            {
                transform.rotation = GlobalGravityControl.GetGravityRotation();
            }

            Vector3 newPosition = CollisionOffset();
            //newPosition += SurfaceRaycastOffset(newPosition);
            transform.position = newPosition;
	    }
	    else if (!safeToWarp)
	    {
	        transform.position = knifeController.transform.position + OmniRaycastOffset(knifeController.transform.position);
	    }

        this.PostNotification(UpdateLookAheadColliderNotification, transform);
    }

    // Uses the position and collisionNormal from the knife to calculate where the player should warp to
    private Vector3 CollisionOffset()
    {
        Vector3 collisionNormal = knifeController.GetCollisionNormal();

        if (collisionNormal == Vector3.zero)
        {
            return transform.position;
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

        return knifeController.GetCollisionPosition() - pointDiff;
    }

    private Vector3 SurfaceRaycastOffset(Vector3 basePosition)
    {
        Vector3 collisionNormal = knifeController.GetCollisionNormal();

        if (collisionNormal == Vector3.zero)
            return Vector3.zero;

        Vector3 offset = Vector3.zero;

        // Check directions at right angles from collision normal
        Quaternion rotateToNormal = Quaternion.FromToRotation(Vector3.up, collisionNormal);
        Vector3 forward = rotateToNormal * Vector3.forward;
        Vector3 right = rotateToNormal * Vector3.right;

        //Debug.DrawLine(basePosition, basePosition + (forward * 2), Color.cyan, 5f);
        //Debug.DrawLine(basePosition, basePosition + (right * 2), Color.magenta, 5f);

        Collider col = GetComponent<Collider>();
        float forwardDist = Vector3.Distance(transform.position, col.ClosestPointOnBounds(transform.position + forward));
        float rightDist = Vector3.Distance(transform.position, col.ClosestPointOnBounds(transform.position + right));

        RaycastHit hitInfo;

        if (Physics.Raycast(basePosition, forward, out hitInfo, forwardDist))
            offset -= forward * (forwardDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, -forward, out hitInfo, forwardDist))
            offset += forward * (forwardDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, right, out hitInfo, rightDist))
            offset -= right * (rightDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, -right, out hitInfo, rightDist))
            offset += right * (rightDist - hitInfo.distance);

        //Debug.DrawLine(basePosition, basePosition + offset, Color.cyan, 5f);

        return offset;
    }

    private Vector3 OmniRaycastOffset(Vector3 basePosition)
    {
        Vector3 offset = Vector3.zero;

        // Check distance at cardinal directions
        Vector3 up = transform.up;
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Collider col = GetComponent<Collider>();
        float verDist = 1f;
        float horDist = 0.5f;

        RaycastHit hitInfo;

        if (Physics.Raycast(basePosition, up, out hitInfo, verDist, raycastMask))
            offset -= up * (verDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, -up, out hitInfo, verDist, raycastMask))
            offset += up * (verDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, forward, out hitInfo, horDist, raycastMask))
            offset -= forward * (horDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, -forward, out hitInfo, horDist, raycastMask))
            offset += forward * (horDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, right, out hitInfo, horDist, raycastMask))
            offset -= right * (horDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, -right, out hitInfo, horDist, raycastMask))
            offset += right * (horDist - hitInfo.distance);

        //Debug.DrawLine(basePosition, basePosition + offset, Color.cyan, 5f);

        return offset;
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
