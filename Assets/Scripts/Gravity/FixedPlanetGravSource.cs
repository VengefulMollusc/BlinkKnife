using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedPlanetGravSource : MonoBehaviour {

    [SerializeField] private float gravStrength = 35f;

    private Transform target;

    void OnEnable()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public float GetDistToTarget()
    {
        return Vector3.Distance(transform.position, target.position);
    }

    public Vector3 GetGravityVector()
    {
        return (target.position - transform.position).normalized;
    }

    public float GetGravityStrength()
    {
        return gravStrength;
    }
}
