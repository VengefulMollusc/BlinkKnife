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

    private Vector3 previousTarget;
    private Vector3 nextTarget;

    private Transform boostRingRoot;

    public const string BoostNotification = "BoostRing.BoostNotification";

    private void OnEnable()
    {
        boostRingRoot = transform.parent;

        //previousTarget = boostRingRoot.position + (-boostRingRoot.up * 10f);
        //nextTarget = boostRingRoot.position + (boostRingRoot.up * 10f);
    }

    /*
     * Used to set previous or next rings when a chain of rings is needed
     */
    public void SetNextRing(Vector3 next)
    {
        nextTarget = next;
    }
    public void SetPreviousRing(Vector3 previous)
    {
        nextTarget = previous;
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

        //col.transform.position = transform.position + (transform.up * 0.5f); // Added 0.5 up here as position zero currently sits at one edge

        float magnitude = Mathf.Max(rb.velocity.magnitude + boostStrength, minMagnitude);

        if (Vector3.Dot(transform.up, rb.velocity) > 0f)
        {
            if (nextTarget != Vector3.zero)
                rb.velocity = GetBoostVector(col.transform.position, nextTarget) * magnitude;
            else
                rb.velocity = transform.up * magnitude;
        }
        else
        {
            if (previousTarget != Vector3.zero)
                rb.velocity = GetBoostVector(col.transform.position, previousTarget) * magnitude;
            else
                rb.velocity = -transform.up * boostStrength;
        }

        this.PostNotification(BoostNotification, col.gameObject);
    }
}
