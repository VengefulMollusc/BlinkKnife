using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using ProBuilder2.Common;
using UnityEngine;

public class SafeWarpCollider : MonoBehaviour
{
    [SerializeField]
    private LayerMask collisionOffsetLayerMask;

    private KnifeController knifeController;

    private bool safeToWarp;

    private Vector3 lastKnifePosition;

    public const string UpdateLookAheadColliderNotification = "SafeWarpCollider.UpdateLookAheadColliderNotification";

    // Use this for initialization
    void OnEnable ()
	{
	    knifeController = transform.parent.GetComponent<KnifeController>();
	    safeToWarp = true;
	    lastKnifePosition = knifeController.GetPosition();
	}
	
	void FixedUpdate ()
	{
        if (knifeController.GetPosition() == lastKnifePosition)
        {
            this.PostNotification(UpdateLookAheadColliderNotification, transform);
            return;
        }

        lastKnifePosition = knifeController.GetPosition();

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
            newPosition += SurfaceRaycastOffset(newPosition);
            transform.position = newPosition;
	    }
	    else if (!safeToWarp)
	    {
            transform.position = knifeController.transform.position + OmniRaycastOffset(knifeController.transform.position);
        }
        
	    this.PostNotification(UpdateLookAheadColliderNotification, transform);
    }

    // Offsets collider position when knife has stuck to a surface
    // Places collider so that it is touching the point where the knife has collided
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
        if (dot >= -0.001) // was > 0.001
            closestPointBase -= (transform.up * 0.5f);
        else if (dot < -0.001)
            closestPointBase += (transform.up * 0.5f);

        Vector3 closestPointOnCollider = GetComponent<Collider>()
            .ClosestPoint(closestPointBase - collisionNormal);

        Vector3 pointDiff = closestPointOnCollider - transform.position;

        return knifeController.GetPosition() - pointDiff;
    }

    // raycasts in 4 cardinal directions parallel to the surface the knife collided with.
    // shifts the collider along the surface according to the raycast result
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

        Collider col = GetComponent<Collider>();
        float forwardDist = Vector3.Distance(transform.position, col.ClosestPoint(transform.position + forward));
        float rightDist = Vector3.Distance(transform.position, col.ClosestPoint(transform.position + right));

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

    // raycasts in 6 cardinal directions while the knife is airborne
    // shifts the collider to avoid obstacles
    private Vector3 OmniRaycastOffset(Vector3 basePosition)
    {
        Vector3 offset = Vector3.zero;

        // Check distance at cardinal directions
        Vector3 up = transform.up;
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        
        float verDist = 1f;
        float horDist = 0.5f;

        RaycastHit hitInfo;

        // Up/Down raycasts
        if (Physics.Raycast(basePosition, up, out hitInfo, verDist, collisionOffsetLayerMask))
            offset -= up * (verDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, -up, out hitInfo, verDist, collisionOffsetLayerMask))
            offset += up * (verDist - hitInfo.distance);


        // forward/back raycasts
        if (Physics.Raycast(basePosition, forward, out hitInfo, horDist, collisionOffsetLayerMask))
            offset -= forward * (horDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, -forward, out hitInfo, horDist, collisionOffsetLayerMask))
            offset += forward * (horDist - hitInfo.distance);


        // right/left raycasts
        if (Physics.Raycast(basePosition, right, out hitInfo, horDist, collisionOffsetLayerMask))
            offset -= right * (horDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, -right, out hitInfo, horDist, collisionOffsetLayerMask))
            offset += right * (horDist - hitInfo.distance);

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
