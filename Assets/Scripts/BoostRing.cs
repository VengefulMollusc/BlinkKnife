using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostRing : MonoBehaviour
{
    private float boostStrength = 1f;

    public const string BoostNotification = "BoostRing.BoostNotification";

    void OnTriggerEnter(Collider col) // could be OnTriggerStay/Enter
    {
        Rigidbody rb = col.GetComponent<Rigidbody>();

        if (rb == null || col.isTrigger)
            return;

        float magnitude = rb.velocity.magnitude;

        if (Vector3.Dot(transform.up, rb.velocity) > 0f)
        {
            rb.velocity = transform.up * (boostStrength + magnitude);
        }
        else
        {
            rb.velocity = -transform.up * (boostStrength + magnitude);
        }

        this.PostNotification(BoostNotification, col.gameObject);
    }
}
