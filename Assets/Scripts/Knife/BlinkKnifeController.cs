using UnityEngine;
using System.Collections;
using AssemblyCSharp;

[RequireComponent(typeof(Rigidbody))]
public class BlinkKnifeController : KnifeController {

    [SerializeField]
    private float throwStrengthMod = 1f;

 //   [SerializeField]
	//private GameObject visuals;

	//public override void Setup (PlayerKnifeController _controller){
 //       base.Setup(_controller);
 //       // add random throw angle
 //       // replace this with constant once animated
 //       //visuals.transform.Rotate (0f, 0f, ((Random.value*2f)-1f) * 90f);

 //   }

	//void FixedUpdate (){
	//	if (HasStuck() || rb == null)
	//		return;
        
 //       // align transform.forward to travel direction
	//	if (rb.velocity != Vector3.zero)
	//		transform.forward = rb.velocity;


 //       // this statement should stop size from recalculating every step
	//    //if (col.size.z == initColZSize)
	//    //    SizeColliderToSpeed(); // probably only needs to happen once, as speed should be pretty constant
	//}

    public override void Throw (Vector3 _velocity)
    {
        // throw the knife in the given direction with a certain force
        rb.AddForce(_velocity * throwStrengthMod, ForceMode.VelocityChange);

        // disable gravity for a moment to allow more accurate throws at close range
        //GetComponent<UtiliseGravity>().TempDisableGravity(0.1f);

        this.PostNotification(AttachLookAheadColliderNotification, this);
    }

	void OnCollisionEnter (Collision _col)
	{
	    if (HasStuck() || rb == null)
	        return;

        ContactPoint collide = _col.contacts [0];
	    GameObject other = collide.otherCollider.gameObject;

        // If collided surface is not a HardSurface, stick knife into it
        // else post bounce notification
        if (other.GetComponent<HardSurface>() == null)
            StickToSurface (collide.point, collide.normal, other);
        else
            this.PostNotification(KnifeBounceNotification);
    }
}
