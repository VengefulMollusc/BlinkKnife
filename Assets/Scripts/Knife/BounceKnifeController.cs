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

    void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Setup (PlayerKnifeController _controller){
        //playerKnifeController = _controller;
		//rb = GetComponent<Rigidbody> ();

        // add random throw angle
        // could raycast throw angle to match surface hit?
        // WITH BOTH KNIVES?
        //visuals.transform.Rotate (0f, 0f, ((Random.value * 2f) - 1f) * 90f);
	    transform.LookAt(transform.position + _controller.transform.forward, _controller.transform.up); //? <-(why is this question mark here?)

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

    void Update()
    {
        if (rb == null)
            return;

        //visuals.transform.Rotate(spinVector);
        if (rb.velocity.magnitude != 0f)
            transform.forward = rb.velocity;
    }

    public void Throw (Vector3 _velocity){
        // throw the knife in the given direction with a certain force
		rb.AddForce (_velocity * throwStrengthMod, ForceMode.VelocityChange);
	}

    void OnCollisionEnter(Collision col)
    {
        this.PostNotification(BounceKnifeCollisionNotification);
    }

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

    public Vector3 GetVelocity (){
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
