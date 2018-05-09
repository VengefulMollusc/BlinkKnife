using System;
using System.Collections;
using System.Security.Policy;
using UnityEngine;

public class KnifeController : MonoBehaviour
{
    [HideInInspector]
    public Transform ownerTransform;

    public float throwStrengthMod = 1f;

    [HideInInspector]
    public Rigidbody rb;

    [HideInInspector]
    public float warpTimer;

    private WarpLookAheadCollider warpLookAheadCollider;

    private bool stuckInSurface;
    private GameObject objectStuck;

    [HideInInspector]
    public bool returning;

    private GravityPanel gravPanel;
    private Vector3 gravShiftVector;

    public const string ShowKnifeMarkerNotification = "KnifeController.ShowKnifeMarkerNotification";
    public const string ReturnKnifeNotification = "KnifeController.ReturnKnifeNotification";

    public const string AttachLookAheadColliderNotification = "KnifeController.AttachLookAheadColliderNotification";

    public const string FibreOpticWarpNotification = "KnifeController.FibreOpticWarpNotification";

    /*
     * Passes the knifecontroller and parameter spin speed to the knife
     */
    public virtual void Setup(Transform _ownerTransform, WarpLookAheadCollider _lookAhead)
    {
        ownerTransform = _ownerTransform;
        warpLookAheadCollider = _lookAhead;
        rb = GetComponent<Rigidbody>();

        stuckInSurface = false;

        gravShiftVector = Vector3.zero;

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
    public void StickToSurface(Vector3 _position, Vector3 _normal, GameObject _other, bool _cancelNotifications = false)
    {
        // disable rigidbody
        rb.isKinematic = true;
        GetComponent<DontGoThroughThings>().enabled = false;
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
            GetComponentInChildren<TrailRenderer>().enabled = false;
            return;
        }

        if (objectStuck.GetComponent<GravityPanel>() != null)
        {
            // Prepare to shift gravity if warping to GravityPanel
            gravPanel = objectStuck.GetComponent<GravityPanel>();
            gravShiftVector = gravPanel.GetGravityVector();

            if (gravShiftVector == Vector3.zero)
                gravShiftVector = -_normal;
        }
        else if (objectStuck.GetComponent<FibreOpticController>() != null)
        {
            // Activate fibre optic warp
            this.PostNotification(FibreOpticWarpNotification, objectStuck.GetComponent<FibreOpticController>());
            return;
        }

        // activate knife marker ui
        Info<Transform, bool> info = new Info<Transform, bool>(transform, gravPanel != null);
        this.PostNotification(ShowKnifeMarkerNotification, info);
    }

    public void ReturnKnifeTransition()
    {
        returning = true;
        StartCoroutine("ReturnKnifeAnimation");
    }

    private IEnumerator ReturnKnifeAnimation()
    {
        // freeze knife and disable collisions
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        rb.detectCollisions = false;
        Vector3 startPos = transform.position;

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
            t += Time.deltaTime / 0.5f; // 0.5f here defines length of transition
            yield return 0;
        }

        // tell playerKnifeController to destroy knife
        this.PostNotification(ReturnKnifeNotification);
    }

    public virtual Vector3 GetPosition()
    {
        return transform.position;
    }

    //public virtual Vector3 GetCollisionPosition()
    //{
    //    return transform.position + collisionPositionOffset;
    //}

    public virtual Vector3 GetCollisionNormal()
    {
        if (!HasStuck())
            return Vector3.zero;

        return transform.up;
    }

    /*
     * Returns position player will warp to
     * 
     * this eventually needs to be converted to the closest place
     * for the player collider to move
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
        return transform.position + (transform.up * 0.5f); // need to change this depending on gravity angle to match distance to edge of collider
    }

    /*
     * Gets the current velocity of the knife
     * 
     * bool determines whether to return the velocity at point of throw
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

    public virtual bool IsBounceKnife()
    {
        return false;
    }

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

