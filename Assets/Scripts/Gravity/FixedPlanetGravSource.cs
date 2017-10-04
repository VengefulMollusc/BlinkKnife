using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedPlanetGravSource : MonoBehaviour {

    [SerializeField] private float gravStrength = 35f;

    private float planetRadius;

    private Transform target;

    void OnEnable()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        planetRadius = transform.localScale.x / 2f;
    }

    public float GetDistToTarget()
    {
        return Vector3.Distance(transform.position, target.position);
    }

    public float GetDistToSurface()
    {
        return GetDistToTarget() - planetRadius;
    }

    public Vector3 GetGravityVector()
    {
        return (transform.position - target.position).normalized;
    }

    public float GetGravityStrength()
    {
        return gravStrength;
    }
}
