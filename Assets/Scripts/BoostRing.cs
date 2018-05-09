using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls boost ring behaviour.
 * Needs to be attached to gameobject with boost ring trigger collider -
 * assumes is child of boost ring root object
 */
public class BoostRing : MonoBehaviour
{
    [SerializeField]
    private float boostStrength = 2f;
    [SerializeField]
    private float minMagnitude = 12f;

    [SerializeField] private Transform nextRing;
    [SerializeField] private Transform previousRing;

    private Vector3 previousTarget;
    private Vector3 nextTarget;

    //private Transform boostRingRoot;

    public const string BoostNotification = "BoostRing.BoostNotification";

    private void OnEnable()
    {
        //boostRingRoot = transform.parent;

        //previousTarget = boostRingRoot.position + (-boostRingRoot.up * 10f);
        //nextTarget = boostRingRoot.position + (boostRingRoot.up * 10f);

        CheckRingChain();
    }

    /*
     * Used to set previous or next rings when a chain of rings is needed
     */
    private void CheckRingChain()
    {
        if (nextRing != null)
            nextTarget = nextRing.position;

        if (previousRing != null)
            previousTarget = previousRing.position;
    }

    private Vector3 GetBoostVector(Vector3 toBoost, Vector3 target)
    {
        return (target - toBoost);
    }

    void OnTriggerEnter(Collider col) // could be OnTriggerStay/Enter
    {
        Rigidbody rb = col.GetComponent<Rigidbody>();

        if (rb == null || col.isTrigger)
            return;

        //col.transform.position = transform.position + (transform.up * 0.5f); // Added 0.5 up here as position zero currently sits at one edge
        BoomerangKnifeController boom = col.GetComponent<BoomerangKnifeController>();
        Vector3 vel = (boom != null) ? boom.GetEffectiveVelocity() : rb.velocity;

        Vector3 boostDirection;
        float magnitude = Mathf.Max(vel.magnitude + boostStrength, minMagnitude);

        if (Vector3.Dot(transform.up, vel) > 0f)
        {
            if (nextTarget != Vector3.zero)
            {
                boostDirection = GetBoostVector(col.transform.position, nextTarget);
                // record distance/time here to next ring if needed for gravity cancelling duration
                boostDirection.Normalize();
            }
            else
            {
                boostDirection = transform.up;
            }
        }
        else
        {
            if (previousTarget != Vector3.zero)
            {
                boostDirection = GetBoostVector(col.transform.position, previousTarget);
                // record distance/time here to next ring if needed for gravity cancelling duration
                boostDirection.Normalize();
            }
            else
            {
                boostDirection = -transform.up;
            }
        }

        //// TODO: replace this with notification handler
        //if (boom != null)
        //    boom.Boost(boostDirection * magnitude);
        //else
        //    rb.velocity = boostDirection * magnitude;

        Debug.Log("Boosting: " + col.gameObject.name);

        Info<GameObject, Vector3> info = new Info<GameObject, Vector3>(col.gameObject, boostDirection * magnitude);

        // TODO: refactor this to post velocity and object in Info object.
        // Individual objects can handle different logic
        this.PostNotification(BoostNotification, info);
    }
}
