using System;
using System.Collections;
using System.Security.Policy;
using UnityEngine;

public class KnifeController : MonoBehaviour
{
    /*
     * Active Feature Additions:
     *  Missile Redirect
     *      Becomes the target of all active (launched?) missiles when thrown
     *  
     *  Create Geometry
     *      Similar to cube weapon functionality.
     *      Spawn geometry when knife lands?
     *      raise shield while knife is grounded?
     *      
     *  EMP
     *      Disable enemies/tech within radius of landing
     *      
     *  Movement alteration?
     *      Launch player at surface normal when warp?
     *      Temporary speed boost on warp?
     *      Allow midair warp?
     *      
     *  Gravity shift
     *      Surface knife collided with becomes 'down'
     *      temporary?
     *      
     *  Multi-warp
     *      throw multiple knifes up to a limit, then warp to all of them in sequence
     *      
     *  Infinite warp
     *      temporary infinite warps
     *      
     *      
     * Passive/Behaviour Change:
     *  Instant Travel (Longbow - borderlands)
     *      Raycast target then warp knife instantly
     *      Through transparent surfaces?
     *      Pinpoint accurate
     *      Potential instant warp, no wait time?
     *      
     *  Homing
     *      lock on to enemies?
     *      act like missile?
     */

    // Transform of player's view model knife/hand object
    // used to target return animation
    [HideInInspector]
    public Transform ownerTransform;

    public float throwStrengthMod = 1f;

    [HideInInspector]
    public Rigidbody rb;

    [HideInInspector]
    public float warpTimer;

    private WarpLookAheadCollider warpLookAheadCollider;

    public bool autoWarp = false;

    private DontGoThroughThings dontGoThroughThings;
    private TrailRenderer trailRenderer;

    private bool stuckInSurface;
    private GameObject objectStuck;

    [HideInInspector]
    public bool returning;

    private GravityPanel gravPanel;
    private Vector3 gravShiftVector;

    public const string ShowKnifeMarkerNotification = "KnifeController.ShowKnifeMarkerNotification";
    public const string ReturnKnifeNotification = "KnifeController.ReturnKnifeNotification";
    public const string ReturnKnifeTransitionNotification = "KnifeController.ReturnKnifeTransitionNotification";

    public const string AttachLookAheadColliderNotification = "KnifeController.AttachLookAheadColliderNotification";

    public const string FibreOpticWarpNotification = "KnifeController.FibreOpticWarpNotification";

    public virtual void OnEnable()
    {
        this.AddObserver(OnBoostNotification, BoostRing.BoostNotification);
        this.AddObserver(OnReturnTransitionNotification, ReturnKnifeTransitionNotification);
    }

    public virtual void OnDisable()
    {
        this.RemoveObserver(OnBoostNotification, BoostRing.BoostNotification);
        this.RemoveObserver(OnReturnTransitionNotification, ReturnKnifeTransitionNotification);
    }

    /*
     * Passes the knifecontroller and parameter spin speed to the knife
     */
    public virtual void Setup(Transform _ownerTransform, WarpLookAheadCollider _lookAhead)
    {
        ownerTransform = _ownerTransform;
        warpLookAheadCollider = _lookAhead;

        stuckInSurface = false;

        gravShiftVector = Vector3.zero;

        rb = GetComponent<Rigidbody>();
        dontGoThroughThings = GetComponent<DontGoThroughThings>();
        trailRenderer = GetComponent<TrailRenderer>();

        // Attach the WarpLookAheadCollider to this knife
        this.PostNotification(AttachLookAheadColliderNotification, this);

        //transform.LookAt(transform.position + _controller.transform.forward, _controller.transform.up); //? <-(why is this question mark here?)
    }

    /*
     * Throws the knife at the given velocity
     */
    public virtual void Throw(Vector3 _velocity)
    {
        // throw the knife in the given direction with a certain force
        rb.AddForce(_velocity * throwStrengthMod, ForceMode.VelocityChange);
        this.PostNotification(AttachLookAheadColliderNotification, this);
    }

    /*
    * Sticks knife into surface when colliding with an object
    */
    public virtual void StickToSurface(Vector3 _position, Vector3 _normal, GameObject _other, bool _cancelNotifications = false)
    {
        // disable rigidbody
        rb.isKinematic = true;
        dontGoThroughThings.enabled = false;
        stuckInSurface = true;

        // align knife position with collision position
        transform.position = _position;
        transform.rotation = Quaternion.FromToRotation(Vector3.up, _normal);

        // stick knife out of surface at collision point
        rb.velocity = Vector3.zero;

        // parent knife to other gameobject (to handle moving objects)
        transform.SetParent(_other.transform);
        objectStuck = _other;

        if (_cancelNotifications)
        {
            trailRenderer.enabled = false;
            return;
        }

        KnifeInteractionTrigger knifeTrigger = objectStuck.GetComponent<KnifeInteractionTrigger>();
        if (knifeTrigger != null)
        {
            // disable autoWarp
            autoWarp = false;
            // attach knife to interactable object
            knifeTrigger.AttachKnife();
        }

        GravityPanel tempGravPanel = objectStuck.GetComponent<GravityPanel>();
        if (tempGravPanel != null)
        {
            // Prepare to shift gravity if warping to GravityPanel
            gravPanel = tempGravPanel;
            gravShiftVector = gravPanel.GetGravityVector();

            if (gravShiftVector == Vector3.zero)
                gravShiftVector = -_normal;
        }

        FibreOpticController fibreController = objectStuck.GetComponent<FibreOpticController>();
        if (fibreController != null)
        {
            // Activate fibre optic warp
            this.PostNotification(FibreOpticWarpNotification, fibreController);
            return;
        }

        // activate knife marker ui
        Info<Transform, bool> info = new Info<Transform, bool>(transform, gravPanel != null);
        this.PostNotification(ShowKnifeMarkerNotification, info);
    }

    void OnReturnTransitionNotification(object sender, object args)
    {
        ReturnKnifeTransition();
    }

    /*
     * Triggers animation of knife returning to player
     */
    public void ReturnKnifeTransition()
    {
        // this is a lot of return :O
        if (returning)
            return;

        // reset knife collision variables (in case it was stuck to something)
        returning = true;
        objectStuck = null;
        stuckInSurface = false;
        gravPanel = null;
        transform.SetParent(null);
        this.PostNotification(ShowKnifeMarkerNotification, null);

        StartCoroutine("ReturnKnifeAnimation");
    }

    /*
     * Animates knife returning to player.
     * 
     * knife 'hangs' for an instant before travelling straight back to the player.
     * collisions etc are ignored
     */
    private IEnumerator ReturnKnifeAnimation()
    {
        // freeze knife and disable collisions
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        rb.detectCollisions = false;
        Vector3 startPos = transform.position;
        float dist = (ownerTransform.position - startPos).magnitude;
        float duration = dist * 0.01f;

        // hold knife in position for a short time
        float t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            yield return 0;
        }

        // animate knife returning to player
        t = 0f;
        while (t <= 1f)
        {
            transform.position = Vector3.Lerp(startPos, ownerTransform.position, t);
            t += Time.deltaTime / duration; // 0.5f here defines length of transition
            yield return 0;
        }

        // tell playerKnifeController to destroy knife
        this.PostNotification(ReturnKnifeNotification);
    }

    public virtual void OnBoostNotification(object sender, object args)
    {
        Info<GameObject, Vector3> info = (Info<GameObject, Vector3>)args;
        if (info.arg0 != gameObject)
            return;

        // boosted object is this knife
        rb.velocity = info.arg1;
    }

    public virtual Vector3 GetPosition()
    {
        return transform.position;
    }

    public virtual Vector3 GetCollisionNormal()
    {
        if (!HasStuck())
            return Vector3.zero;

        return transform.up;
    }

    /*
     * Returns position player will warp to
     * Determined by WarpLookahead colliders
     */
    public virtual Vector3 GetWarpPosition()
    {
        return warpLookAheadCollider != null ? warpLookAheadCollider.WarpPosition() : GetWarpTestPosition();
    }

    /*
     * Gives the position needed to be tested by the warp lookahead collider
     */
    public virtual Vector3 GetWarpTestPosition()
    {
        return transform.position + (transform.up * 0.5f);
    }

    /*
     * Gets the current velocity of the knife
     */
    public virtual Vector3 GetVelocity()
    {
        return rb.velocity;
    }

    /*
     * Returns the object the knife collided with (if existing)
     */
    public virtual GameObject GetStuckObject()
    {
        return objectStuck;
    }

    public virtual Vector3 GetGravVector()
    {
        return gravShiftVector;
    }

    public virtual bool HasStuck()
    {
        return stuckInSurface;
    }

    /*
     * Tells the playerKnifeController to warp as soon as the CanWarp conditions are met
     */
    public virtual bool AutoWarp()
    {
        return autoWarp;
    }

    /*
     * Conditions determining whether the player can warp to the knife currently
     */
    public virtual bool CanWarp()
    {
        return HasStuck();
    }

    /*
     * whether to shift gravity on warp
     */
    public virtual bool ShiftGravity()
    {
        return (stuckInSurface && gravPanel != null);
    }
}

