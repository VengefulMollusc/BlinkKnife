using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class BounceKnifeController : KnifeController
{

    [SerializeField]
    private float throwStrengthMod = 1f;

    [SerializeField]
    private bool mustBounceToWarp = true;
    private bool hasCollided;

    [SerializeField]
    private float bounceWarpWaitTime = 0.2f;

    private Vector3 warpVelocity;

    //[SerializeField]
    //private GameObject visuals;

    //void FixedUpdate()
    //{
    //    if (HasStuck() || rb == null)
    //        return;

    //    if (rb.velocity != Vector3.zero)
    //        transform.forward = rb.velocity;
    //}

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

    void OnEnable()
    {
        this.AddObserver(OnWarpNotification, PlayerMotor.WarpNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnWarpNotification, PlayerMotor.WarpNotification);
    }

    void OnWarpNotification(object sender, object args)
    {
        warpVelocity = rb.velocity;
        rb.isKinematic = true;
    }

    public override void Throw(Vector3 _velocity)
    {
        // throw the knife in the given direction with a certain force
        rb.AddForce(_velocity * throwStrengthMod, ForceMode.VelocityChange);
        this.PostNotification(AttachLookAheadColliderNotification, this);
    }

    void OnCollisionEnter(Collision _col)
    {
        if (HasStuck() || rb == null)
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

    public override bool IsBounceKnife()
    {
        return true;
    }

    public override Vector3 GetVelocity()
    {
        return warpVelocity;
    }

    //public override void Setup (PlayerKnifeController _controller)
    //{
    //    base.Setup(_controller);

    //    // add random throw angle
    //    // could raycast throw angle to match surface hit?
    //    // WITH BOTH KNIVES?
    //    //visuals.transform.Rotate (0f, 0f, ((Random.value * 2f) - 1f) * 90f);

    //    //SetThrowRotation();
    //}

    /*
     * Unsure how useful this is, if knife too fast then you probably cant even see it
     */
    //private void SetThrowRotation(){
    //    RaycastHit hit;

    //    if (Physics.Raycast(transform.position, transform.forward, out hit, 100f))
    //    {
    //        Vector3 hitNormal = hit.normal;
    //        Vector3 projected = Vector3.ProjectOnPlane(hitNormal, transform.forward).normalized;

    //        float angle = Vector3.Angle(transform.up, projected);
    //        bool positive = (Vector3.Dot(projected, transform.right) >= 0f);
    //        if (positive)
    //        {
    //            visuals.transform.Rotate(0f, 0f, angle);
    //        } else
    //        {
    //            visuals.transform.Rotate(0f, 0f, -angle);
    //        }
    //    } else
    //    {
    //        //visuals.transform.Rotate(0f, 0f, ((Random.value * 2f) - 1f) * 90f);
    //        visuals.transform.Rotate(0f, 0f, 90f);
    //    }
    //}
}
