using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedPlanetGravController : MonoBehaviour {

    private Vector3 currentGravDir;
    private float currentGravStrength;

    private FixedPlanetGravSource[] gravitySources;

    void OnEnable()
    {
        gravitySources = FindObjectsOfType<FixedPlanetGravSource>();

    }

    void FixedUpdate()
    {
        UpdateGravity();
    }

    // applies a constant gravity value to the closest grav source
    void UpdateGravity()
    {
        FixedPlanetGravSource closestSource = null;
        float closestDist = float.MaxValue;

        foreach (FixedPlanetGravSource source in gravitySources)
        {
            float distToTarget = source.GetDistToTarget();
            if (distToTarget < closestDist)
            {
                closestDist = distToTarget;
                closestSource = source;
            }
        }

        if (closestSource == null)
            return;

        GlobalGravityControl.ChangeGravity(closestSource.GetGravityVector(), closestSource.GetGravityStrength());
    }
}
