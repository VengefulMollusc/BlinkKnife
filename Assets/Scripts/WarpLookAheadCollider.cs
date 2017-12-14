﻿using System.Collections;
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

    //private Vector3 lastUsablePos;

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

        //if (CanWarp())
        //    lastUsablePos = transform.position;

        //Debug.Log(CanWarp());

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
        //lastUsablePos = knifeObject.transform.position;
        lastKnifePos = knifeObject.transform.position;
    }

    // Update position to match knife position
    private void MatchKnifePosition()
    {
        if (lastKnifePos != knifeObject.transform.position)
        {
            rb.MovePosition(knifeController.GetWarpTestPosition());
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
    }

    // should return true only if the internal trigger collider is not touching any other colliders
    //bool CanWarp()
    //{
    //    //return colliding.Count <= 0;
    //    return !colliding;
    //}

    public Vector3 WarpPosition()
    {
        //return lastUsablePos;
        return transform.position;
    }

    //void OnTriggerStay(Collider col)
    //{
    //    //if (!colliding.Contains(col) && !col.isTrigger)
    //    //    colliding.Add(col);
    //    colliding = true;
    //}

    //void OnTriggerExit(Collider col)
    //{
    //    //colliding.Remove(col);
    //    colliding = false;
    //}
}
