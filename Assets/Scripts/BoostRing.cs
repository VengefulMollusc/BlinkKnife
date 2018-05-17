using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls boost ring behaviour.
 * Needs to be attached to gameobject with boost ring trigger collider -
 * assumes is child of boost ring root object
 * 
 * TODO: rename/change to light theme. eg: Lens?
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

    public const string BoostNotification = "BoostRing.BoostNotification";

    private void Start()
    {
        CheckRingChain();
    }

    /*
     * Used to set previous or next rings when a chain of rings is needed
     */
    private void CheckRingChain()
    {
        // check for next ring
        if (nextRing != null)
            nextTarget = nextRing.position;

        // check for previous ring
        if (previousRing != null)
            previousTarget = previousRing.position;
    }

    private Vector3 GetBoostVector(Vector3 toBoost, Vector3 target)
    {
        return (target - toBoost).normalized;
    }

    void OnTriggerEnter(Collider col) // could be OnTriggerStay/Enter
    {
        Rigidbody rb = col.GetComponent<Rigidbody>();

        if (rb == null || col.isTrigger)
            return;
        
        BoomerangKnifeController boom = col.GetComponent<BoomerangKnifeController>();
        Vector3 vel = (boom != null) ? boom.GetVelocity() : rb.velocity;
        vel = Vector3.Project(vel, transform.up);

        Vector3 boostDirection;
        float magnitude = Mathf.Max(vel.magnitude + boostStrength, minMagnitude);

        if (Vector3.Dot(transform.up, vel) > 0f)
        {
            if (nextTarget != Vector3.zero)
            {
                boostDirection = GetBoostVector(col.transform.position, nextTarget);
                // record distance/time here to next ring if needed for gravity cancelling duration
            }
            else
                boostDirection = transform.up;
        }
        else
        {
            if (previousTarget != Vector3.zero)
            {
                boostDirection = GetBoostVector(col.transform.position, previousTarget);
                // record distance/time here to next ring if needed for gravity cancelling duration
            }
            else
                boostDirection = -transform.up;
        }

        Info<GameObject, Vector3> info = new Info<GameObject, Vector3>(col.gameObject, boostDirection * magnitude);
        // notification so individual objects can handle different logic
        this.PostNotification(BoostNotification, info);
    }
}
