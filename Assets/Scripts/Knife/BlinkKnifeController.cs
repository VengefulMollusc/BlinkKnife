﻿using UnityEngine;
using System.Collections;
using AssemblyCSharp;

[RequireComponent(typeof(Rigidbody))]
public class BlinkKnifeController : MonoBehaviour, KnifeController {

	private Rigidbody rb;

	private PlayerKnifeController playerKnifeController;

    [SerializeField]
    private float throwStrengthMod = 1f;

    //[SerializeField]
    //private bool useGravity = true;

    //[SerializeField]
    //private float gravityForce = 0f;

    [SerializeField]
	private GameObject visuals;
	private Vector3 throwVelocity;

	private bool collided;
	private Vector3 collisionNormal;

	private GameObject objectCollided = null;

    private GravityPanel gravPanel;

	public void Setup (PlayerKnifeController _controller){
		playerKnifeController = _controller;
		rb = GetComponent<Rigidbody> ();

		collided = false;
		collisionNormal = Vector3.zero;

        // add random throw angle
        // replace this with constant once animated
        //visuals.transform.Rotate (0f, 0f, ((Random.value*2f)-1f) * 90f);

	    transform.LookAt(transform.position + _controller.transform.forward, _controller.transform.up); //?
    }

	void Update (){
		if (collided || rb == null)
			return;
        
        // align transform.forward to travel direction
		if (rb.velocity.magnitude != 0f)
			transform.forward = rb.velocity;
	}

    //private void FixedUpdate()
    //{
    //    if (useGravity) { 
    //        // manual gravity control
    //        rb.AddForce(gravDir * gravityForce, ForceMode.Acceleration);
    //    }

    //    // dampen horizontal velocity too?
    //    //rb.velocity = new Vector3(rb.velocity.x * horDragFactor, rb.velocity.y, rb.velocity.z * horDragFactor);
    //}



    public void Throw (Vector3 _velocity){

		// set the player owner of the knife
		// playerKnifeController = _playerKnifeController;

		// throw the knife in the given direction with a certain force
		rb.AddForce (_velocity * throwStrengthMod, ForceMode.VelocityChange);
		throwVelocity = _velocity;

        // disable gravity for a moment to allow more accurate throws at close range
        GetComponent<UtiliseGravity>().TempDisableGravity(0.2f);
	}

	void OnCollisionEnter (Collision col){
		ContactPoint _collide = col.contacts [0];
		Collide (_collide.point, _collide.normal, _collide.otherCollider.gameObject);
	}

	/*
     * Freezes knife when colliding with an object
     */
	void Collide (Vector3 _point, Vector3 _normal, GameObject _other){
		// disable rigidbody
		rb.detectCollisions = false;
		rb.isKinematic = true;

		collided = true;
		collisionNormal = _normal;

		// stick knife out of surface at collision point
		rb.velocity = Vector3.zero;
		visuals.transform.forward = transform.forward;

		// parent knife to other gameobject (to handle moving objects)
		transform.SetParent (_other.transform);
		objectCollided = _other;

        // Prepare to shift gravity if warping to GravityPanel
        if (objectCollided.GetComponent<GravityPanel>() != null)
        {
            gravPanel = objectCollided.GetComponent<GravityPanel>();
            Vector3 gravVector = gravPanel.GetGravityVector();
            if (gravVector != Vector3.zero)
                collisionNormal = -gravVector;
        }

		// activate knife marker ui
		playerKnifeController.SetKnifeMarkerTarget (transform, gravPanel != null);
	}

	public Vector3 GetPosition (){
		return transform.position;
	}

	public Vector3 GetWarpPosition (){
		return transform.position + (collisionNormal * 0.5f);
	}

    public Vector3 GetGravVector()
    {
        return -collisionNormal;
    }

	public Vector3 GetVelocity (){
        //if (_throwVelocity)
        //	return throwVelocity;
        return rb.velocity;
        //return Vector3.zero;
    }

	public bool HasCollided(){
		return collided;
	}

	public GameObject GetObjectCollided(){
		return objectCollided;
	}

    public bool ShiftGravity()
    {
        return (collided && gravPanel != null);
    }
}
