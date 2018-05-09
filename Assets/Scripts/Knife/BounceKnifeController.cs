using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class BounceKnifeController : KnifeController
{
    /*
     * 'Bouncy' knife option.
     * Useful for navigating spaces too small for the player, 
     * or for rapid movement due to speed boost and ability to bounce round corners
     * 
     * travels for a short distance before recalling
     * If it hits something, the life timer resets and it bounces.
     * 
     * auto-warps when the timer has run out if it has bounced off any surface.
     * player recieved a speed boost in the direction the knife was travelling when warp occurs
     */

    [SerializeField]
    private bool mustBounceToWarp = true;
    private bool hasCollided;

    [SerializeField]
    private float bounceWarpWaitTime = 0.2f;

    private Vector3 warpVelocity;

    public override void OnEnable()
    {
        base.OnEnable();
        this.AddObserver(OnWarpNotification, PlayerMotor.WarpNotification);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        this.RemoveObserver(OnWarpNotification, PlayerMotor.WarpNotification);
    }

    void OnWarpNotification(object sender, object args)
    {
        warpVelocity = rb.velocity;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
    }

    void Update()
    {
        if (returning)
            return;

        warpTimer += Time.deltaTime;

        if (mustBounceToWarp && !hasCollided && warpTimer > bounceWarpWaitTime)
            // return knife
            ReturnKnifeTransition();
    }

    public override bool CanWarp()
    {
        return warpTimer > bounceWarpWaitTime && (hasCollided || !mustBounceToWarp);
    }

    void OnCollisionEnter(Collision _col)
    {
        if (HasStuck())
            return;

        ContactPoint collide = _col.contacts[0];
        GameObject other = collide.otherCollider.gameObject;

        // If collided surface is not a HardSurface, stick knife into it
        if (other.GetComponent<SoftSurface>() != null || other.GetComponent<FibreOpticController>() != null)
            StickToSurface(collide.point, collide.normal, other);
        else
            warpTimer = 0f;

        hasCollided = true;
    }

    public override bool AutoWarp()
    {
        return true;
    }

    public override Vector3 GetVelocity()
    {
        return warpVelocity;
    }

    public override void OnBoostNotification(object sender, object args)
    {
        Info<GameObject, Vector3> info = (Info<GameObject, Vector3>)args;
        if (info.arg0 != gameObject)
            return;

        // boosted object is this knife
        rb.velocity = info.arg1;

        hasCollided = true;
        warpTimer = 0f;
    }
}
