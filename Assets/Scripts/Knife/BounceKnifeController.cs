using UnityEngine;
using System.Collections;
using AssemblyCSharp;

[RequireComponent(typeof(Rigidbody))]
public class BounceKnifeController : KnifeController {

    public const string BounceKnifeCollisionNotification = "BounceKnife.BounceKnifeCollisionNotification";

    [SerializeField]
    private float throwStrengthMod = 1f;

	//[SerializeField]
	//private GameObject visuals;

    //void FixedUpdate()
    //{
    //    if (HasStuck() || rb == null)
    //        return;

    //    if (rb.velocity != Vector3.zero)
    //        transform.forward = rb.velocity;
    //}

    public override void Throw(Vector3 _velocity)
    {
        // throw the knife in the given direction with a certain force
        rb.AddForce(_velocity * throwStrengthMod, ForceMode.VelocityChange);
    }

    void OnCollisionEnter(Collision _col)
    {
        if (HasStuck() || rb == null)
            return;

        ContactPoint collide = _col.contacts[0];
        GameObject other = collide.otherCollider.gameObject;

        // If collided surface is not a HardSurface, stick knife into it
        if (other.GetComponent<SoftSurface>() != null)
            StickToSurface(collide.normal, other);

        this.PostNotification(BounceKnifeCollisionNotification);
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
