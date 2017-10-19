using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostRing : MonoBehaviour
{
    private float boostStrength = 2f;

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
            //rb.velocity = transform.up * boostStrength;
        }
        else
        {
            rb.velocity = -transform.up * (boostStrength + magnitude);
            //rb.velocity = -transform.up * boostStrength;
        }

        this.PostNotification(BoostNotification, col.gameObject);
    }
}
