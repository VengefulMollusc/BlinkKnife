﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/**
 * Attaches to a planet sphere object, and contributes to the cumulative gravity on the player
 */
public class PlanetaryGravitySource : MonoBehaviour
{
    [SerializeField] private float gravMinDistance = 20f;
    [SerializeField] private float gravMaxDistance = 100f;
    [SerializeField] private float gravStrength = 35f;

    private float planetRadius;

    private Transform target;

    void OnEnable()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;

        planetRadius = transform.localScale.x / 2f;
    } 

    public Vector3 GetGravityVector()
    {
        if (target == null)
            return Vector3.zero;

        Vector3 gravDirection = transform.position - target.position;
        float distToPlayer = gravDirection.magnitude - planetRadius;

        // value for to2 here acts as absolute min grav value - stops flying into space
        float strengthRatio = Utilities.MapValues(distToPlayer, gravMinDistance, gravMaxDistance, 1f, 0.25f, true);

        float currentStrength = strengthRatio * strengthRatio * gravStrength; // exponential dropoff?

        return gravDirection.normalized * currentStrength;
    }
}
