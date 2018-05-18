﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    [SerializeField]
    private AbilityType abilityType;

    [SerializeField]
    private const float cooldown = 2f;

    private Rigidbody rb;
    private Renderer renderer;
    private bool active;

    /*
     * TODO: st up so only player collides with this
     */
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        renderer = GetComponent<Renderer>();
    }

    void OnTriggerStay(Collider col)
    {
        PlayerAbilityController abilityController = col.GetComponent<PlayerAbilityController>();

        if (abilityController == null)
            return;

        abilityController.EnableAbility(abilityType);

        StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        rb.detectCollisions = false;
        renderer.enabled = false;

        yield return new WaitForSeconds(cooldown);

        rb.detectCollisions = true;
        renderer.enabled = true;
    }

}
