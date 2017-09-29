using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * Gets distance-based gravity values from each PlanetaryGravitySource in the scene
 * and applies to the player
 */
public class PlanetaryGravityController : MonoBehaviour
{

    private Vector3 currentGravDir;
    private float currentGravStrength;

    private PlanetaryGravitySource[] gravitySources;

    void OnEnable()
    {
        gravitySources = FindObjectsOfType<PlanetaryGravitySource>();

    }

    void Update()
    {
        UpdateGravity();
    }

    // Gets the collective gravity of all planets and updates the GlobalGravityControl
    private void UpdateGravity()
    {
        Vector3 newGravDir = Vector3.zero;

        foreach (PlanetaryGravitySource source in gravitySources)
        {
            newGravDir += source.GetGravityVector();
        }

        float newGravStrength = newGravDir.magnitude;
        newGravDir.Normalize();

        GlobalGravityControl.ChangeGravity(newGravDir, newGravStrength, true);
    }
}
