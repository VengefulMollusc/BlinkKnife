using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEngine;

public class WarpLookAheadCollider : MonoBehaviour
{
    private Collider[] lookAheadColliders;
    private List<Collider> colliding;

    private GameObject knifeObject;
    private KnifeController knifeController;

    private Vector3 lastUsablePos;

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
        colliding = new List<Collider>();
        lookAheadColliders = GetComponents<Collider>();

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

        if (CanWarp())
            lastUsablePos = transform.position;
        
        MatchKnifePosition();
    }

    public void LockToKnife(GameObject _knife)
    {
        knifeObject = _knife;
        knifeController = knifeObject.GetComponent<KnifeController>();
        Utilities.IgnoreCollisions(lookAheadColliders, knifeObject.GetComponents<Collider>(), true);
        Enabled(true);
        MatchKnifePosition();
        lastUsablePos = Vector3.negativeInfinity;
    }

    // Update position to match knife position
    private void MatchKnifePosition()
    {
        transform.position = knifeController.GetWarpTestPosition();
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
    bool CanWarp()
    {
        return colliding.Count <= 0;
    }

    public Vector3 WarpPosition()
    {
        return lastUsablePos;
    }

    void OnTriggerStay(Collider col)
    {
        if (!colliding.Contains(col) && !col.isTrigger)
            colliding.Add(col);
    }

    void OnTriggerExit(Collider col)
    {
        colliding.Remove(col);
    }
}
