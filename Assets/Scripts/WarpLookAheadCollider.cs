using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEngine;

public class WarpLookAheadCollider : MonoBehaviour
{
    private Collider[] lookAheadColliders;
    private bool colliding;

    private GameObject knifeObject;
    private KnifeController knifeController;

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

        Utilities.IgnoreCollisions(lookAheadColliders, GameObject.FindGameObjectWithTag("Player").GetComponents<Collider>(), true);

        Enabled(false);

        this.AddObserver(OnGravityChange, GlobalGravityControl.GravityChangeNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnGravityChange, GlobalGravityControl.GravityChangeNotification);
    }

    void FixedUpdate()
    {
        if (!enabled)
            return;

        if (knifeObject == null)
        {
            Enabled(false);
            return;
        }

        rb.velocity = Vector3.zero;

        if (!colliding)
        {
            // if not colliding, this spot is safe
            lastUsablePos = transform.position;
            //backCheckDistance = Vector3.zero;
        }
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

        MatchKnifePosition();

        colliding = false;
    }

    public void LockToKnife(GameObject _knife)
    {
        knifeObject = _knife;
        knifeController = knifeObject.GetComponent<KnifeController>();
        colliding = false;
        Utilities.IgnoreCollisions(lookAheadColliders, knifeObject.GetComponents<Collider>(), true);
        Enabled(true);
        transform.position = knifeObject.transform.position;
        lastUsablePos = knifeObject.transform.position;
        lastKnifePos = knifeObject.transform.position;
        //backCheckDistance = Vector3.zero;
    }

    // Update position to match knife position
    private void MatchKnifePosition()
    {
        rb.MovePosition(Vector3.MoveTowards(transform.position, knifeController.GetWarpTestPosition(), 1f));

        if (lastKnifePos != knifeObject.transform.position)
        {
            //rb.MovePosition(knifeController.GetWarpTestPosition());
            //rb.MovePosition(Vector3.MoveTowards(transform.position, knifeController.GetWarpTestPosition(), 1f));
            
            lastKnifePos = knifeObject.transform.position;
        }
    }

    private void OnGravityChange(object sender, object args)
    {
        transform.rotation = GlobalGravityControl.GetGravityRotation();
    }

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
