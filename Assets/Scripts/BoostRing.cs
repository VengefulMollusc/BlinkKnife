using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostRing : MonoBehaviour
{
    [SerializeField]
    private float boostStrength = 2f;
    [SerializeField]
    private float minMagnitude = 12f;

    private Vector3 previousTarget;
    private Vector3 nextTarget;

    //float spinSpeed;

    public const string BoostNotification = "BoostRing.BoostNotification";

    private void OnEnable()
    {
        //spinSpeed = boostStrength * boostStrength * 0.05f;
        previousTarget = transform.position + (-transform.up * 10f);
        nextTarget = transform.position + (transform.up * 10f);
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

    //private void Update()
    //{
    //    //transform.Rotate(transform.up * boostStrength * Time.deltaTime); // FOR SOME STUPID REASON THIS DOESNT ROTATE AROUND THE RIGHT AXIS
    //    transform.RotateAroundLocal(transform.up, spinSpeed * Time.deltaTime);
    //}

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

        float magnitude = Mathf.Max(rb.velocity.magnitude, minMagnitude);

        if (Vector3.Dot(transform.up, rb.velocity) > 0f)
        {
            rb.velocity = GetBoostVector(col.transform.position, nextTarget) * (boostStrength + magnitude);
            //rb.velocity = transform.up * boostStrength;
        }
        else
        {
            rb.velocity = GetBoostVector(col.transform.position, previousTarget) * (boostStrength + magnitude);
            //rb.velocity = -transform.up * boostStrength;
        }

        this.PostNotification(BoostNotification, col.gameObject);
    }
}
