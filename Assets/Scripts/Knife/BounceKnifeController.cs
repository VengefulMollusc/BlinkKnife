using UnityEngine;
using System.Collections;
using AssemblyCSharp;

[RequireComponent(typeof(Rigidbody))]
public class BounceKnifeController : MonoBehaviour, KnifeController {

	private Rigidbody rb;

    public const string BounceKnifeCollisionNotification = "BounceKnife.BounceKnifeCollisionNotification";

    [SerializeField]
    private float throwStrengthMod = 1f;

//	private PlayerKnifeController playerKnifeController;

	[SerializeField]
	private GameObject visuals;
	private Vector3 spinVector;
//	private Vector3 throwVelocity;

	void Start (){
		rb = GetComponent<Rigidbody> ();
	}

	public void Setup (PlayerKnifeController _controller, Vector3 _gravityDir, float _spinSpeed){
//		playerKnifeController = _controller;
		rb = GetComponent<Rigidbody> ();
		spinVector = new Vector3(_spinSpeed, 0.0f, 0.0f);

        // add random throw angle
        // could raycast throw angle to match surface hit?
        // WITH BOTH KNIVES?
        visuals.transform.Rotate (0f, 0f, ((Random.value * 2f) - 1f) * 90f);

        //SetThrowRotation();
	}

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

	void Update (){
		visuals.transform.Rotate (spinVector);
//		if (rb.velocity.magnitude != 0f)
//			transform.forward = rb.velocity;
	}

	public void Throw (Vector3 _velocity){

        // set the player owner of the knife
        //		playerKnifeController = _playerKnifeController;

        // throw the knife in the given direction with a certain force
        //_direction.Normalize();
		rb.AddForce (_velocity * throwStrengthMod, ForceMode.VelocityChange);
//		throwVelocity = _velocity;
	}

    void OnCollisionEnter(Collision col)
    {
        this.PostNotification(BounceKnifeCollisionNotification);
    }

    //	// called when a thrown knife collides with an object
    //	void Collide (Vector3 _point, Vector3 _normal, GameObject _other){
    //		// disable rigidbody
    //		rb.detectCollisions = false;
    //		rb.useGravity = false;
    //		rb.isKinematic = true;
    //
    //		collided = true;
    //		hitSurfaceNormal = _normal;
    //
    //		// stick knife out of surface at collision point
    //		rb.velocity = Vector3.zero;
    //		visuals.transform.forward = transform.forward;
    //
    //		// parent knife to other gameobject (to handle moving objects)
    //		transform.SetParent (_other.transform);
    //	}

    public Vector3 GetPosition (){
		return transform.position;
	}

	public Vector3 GetWarpPosition (){
		return transform.position;
	}

    public Vector3 GetGravVector()
    {
        return Vector3.zero;
    }

    public Vector3 GetVelocity (bool _throwVelocity){
		return rb.velocity;
	}

	public bool HasCollided (){
		return false;
	}

	public GameObject GetObjectCollided(){
		return null;
	}

    public bool ShiftGravity()
    {
        return false;
    }
}
