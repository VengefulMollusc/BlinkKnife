using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEngine;

public class WarpLookAheadCollider : MonoBehaviour
{
    public static float updateFrequency = 0.1f;

    private Collider[] lookAheadColliders;
    private bool colliding;

    private GameObject knifeObject;
    private KnifeController knifeController;

    private SafeWarpCollider safeWarpCollider;

    private Vector3 lastKnifePos;

    private Rigidbody rb;

    private Vector3 lastUsablePos;
    //private Vector3 backCheckDistance;

    private bool enabled;

    /*
     * Several way I could do this.
     * 
     * find closest point to final knife position where collider will fit.
     * 
     * Follow path of knife and record last position where not colliding with anything. 
     * (THIS ONE LOOKING MOST LIKELY - not too complicated and provides easy fix for knife in crevice issues. 
     * Wouldn't have to have warp fizzle state)
     */

    void OnEnable()
    {
        colliding = false;
        lookAheadColliders = GetComponents<Collider>();

        rb = GetComponent<Rigidbody>();

        Enabled(false);

        this.AddObserver(OnAttachKnife, KnifeController.AttachLookAheadColliderNotification);
        this.AddObserver(UpdateWarpLookAhead, SafeWarpCollider.UpdateLookAheadColliderNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnAttachKnife, KnifeController.AttachLookAheadColliderNotification);
        this.RemoveObserver(UpdateWarpLookAhead, SafeWarpCollider.UpdateLookAheadColliderNotification);
    }

    void UpdateWarpLookAhead(object sender, object args)
    {
        if (!enabled)
            return;

        if (knifeObject == null)
        {
            transform.position = lastUsablePos; // TODO: THIS LINE PURELY FOR DEBUGGING - Visual indicator of warp position

            Enabled(false);
            return;
        }

        rb.velocity = Vector3.zero;
        transform.rotation = safeWarpCollider.transform.rotation;

        if (safeWarpCollider.IsSafeToWarp())
        {
            lastUsablePos = safeWarpCollider.transform.position;
            transform.position = lastUsablePos;
            //colliding = false;
            //return;
        }

        transform.position = safeWarpCollider.transform.position;

        if (!colliding)
        {
            // if not colliding, this spot is safe
            lastUsablePos = transform.position;
            //backCheckDistance = Vector3.zero;
        }

        transform.position = lastUsablePos;

        colliding = false;

        return;
        //else
        //{
        //    // if colliding at current position
        //    // move slightly back towards last safe position.
        //    // this continues until either the knife reaches a new safe position, or we move back far enough to be safe
        //    rb.position = Vector3.MoveTowards(transform.position, lastUsablePos, 0.2f);

        //    // TODO: rework this so it checks from lastUsablePos towards knife rather than the other way round
        //    // May need second collider?????

        //    //float newDist = backCheckDistance.magnitude - 0.1f;

        //    //if (newDist <= 0)
        //    //    backCheckDistance = Vector3.zero;
        //    //else
        //    //    backCheckDistance = backCheckDistance.normalized * newDist;

        //    //rb.position = lastUsablePos + backCheckDistance;
        //}

        //CheckGravityWarp();

        //MatchKnifePosition();

        //colliding = false;
    }

    void OnAttachKnife(object sender, object args)
    {
        knifeController = (KnifeController)args;
        knifeObject = knifeController.gameObject;
        safeWarpCollider = knifeObject.GetComponentInChildren<SafeWarpCollider>();
        colliding = false;
        Utilities.IgnoreCollisions(lookAheadColliders, knifeObject.GetComponents<Collider>(), true);
        Enabled(true);
        transform.position = knifeObject.transform.position;
        transform.rotation = GlobalGravityControl.GetGravityRotation();
        lastUsablePos = knifeObject.transform.position;
        lastKnifePos = knifeObject.transform.position;
    }

    // Update position to match knife position
    //private void MatchKnifePosition()
    //{
    //    //Vector3 warpPosition = CalculateWarpPosition();
    //    Vector3 targetPosition = safeWarpCollider.transform.position;

    //    if (lastKnifePos != safeWarpCollider.transform.position ||
    //        Vector3.Distance(transform.position, targetPosition) > 1f)
    //    {
    //        //rb.MovePosition(knifeController.GetWarpTestPosition());
    //        rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, 1f));

    //        lastKnifePos = knifeObject.transform.position;
    //    }
    //}

    // Uses the position and collisionNormal from the knife to calculate where the player should warp to
    //private Vector3 CalculateWarpPosition()
    //{
    //    Vector3 collisionNormal = knifeController.GetCollisionNormal();

    //    if (collisionNormal == Vector3.zero)
    //    {
    //        return knifeController.GetPosition();
    //    }

    //    Vector3 collisionPos = knifeController.GetCollisionPosition();
    //    Vector3 closestPointOnCollider = lookAheadColliders[0]
    //        .ClosestPointOnBounds(transform.position - collisionNormal);

    //    Vector3 pointDiff = closestPointOnCollider - transform.position;

    //    return collisionPos - pointDiff;
    //}

    // Updates collider rotation if knife is going to perform a gravwarp
    //private void CheckGravityWarp()
    //{
    //    if (knifeController.ShiftGravity() && transform.up != -knifeController.GetGravVector())
    //    {
    //        transform.rotation = Quaternion.FromToRotation(Vector3.down, knifeController.GetGravVector());
    //    }
    //}

    //private void OnGravityChange(object sender, object args)
    //{
    //    transform.rotation = GlobalGravityControl.GetGravityRotation();
    //}

    public void Enabled(bool _enabled)
    {
        foreach (Collider col in lookAheadColliders)
        {
            col.enabled = _enabled;
        }
        enabled = _enabled;

        if (!enabled)
            rb.velocity = Vector3.zero;
    }

    public Vector3 WarpPosition()
    {
        if (colliding)
            return lastUsablePos;

        return transform.position;
    }

    void OnTriggerStay(Collider col)
    {
        //if (!colliding.Contains(col) && !col.isTrigger)
        //    colliding.Add(col);
        colliding = true;

        //if (backCheckDistance == Vector3.zero)
        //    backCheckDistance = transform.position - lastUsablePos;
    }

    //void OnTriggerExit(Collider col)
    //{
    //    //colliding.Remove(col);
    //    colliding = false;
    //}
}
