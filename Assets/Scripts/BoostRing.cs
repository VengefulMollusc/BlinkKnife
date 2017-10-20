﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostRing : MonoBehaviour
{
    [SerializeField]
    private float boostStrength = 2f;
    [SerializeField]
    private float minMagnitude = 12f;

    float spinSpeed;

    public const string BoostNotification = "BoostRing.BoostNotification";

    private void OnEnable()
    {
        spinSpeed = boostStrength * boostStrength * 0.05f;
    }

    private void Update()
    {
        //transform.Rotate(transform.up * boostStrength * Time.deltaTime); // FOR SOME STUPID REASON THIS DOESNT ROTATE AROUND THE RIGHT AXIS
        transform.RotateAroundLocal(transform.up, spinSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider col) // could be OnTriggerStay/Enter
    {
        Rigidbody rb = col.GetComponent<Rigidbody>();

        if (rb == null || col.isTrigger)
            return;

        float magnitude = Mathf.Max(rb.velocity.magnitude, minMagnitude);

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