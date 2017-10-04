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

    void FixedUpdate()
    {
        UpdateGravity();
    }

    // Gets the collective gravity of all planets and updates the GlobalGravityControl
    private void UpdateGravity()
    {
        Vector3 newGravDir = Vector3.zero;

        Vector3 strongest = Vector3.zero;

        foreach (PlanetaryGravitySource source in gravitySources)
        {
            Vector3 sourceGravity = source.GetGravityVector();

            if (sourceGravity.magnitude > strongest.magnitude)
            {
                strongest = sourceGravity;
            }

            newGravDir += sourceGravity;
        }

        // Aligns gravity direction to cumulative direction - overall center of mass
        //float newGravStrength = newGravDir.magnitude;
        //newGravDir.Normalize();
        //GlobalGravityControl.ChangeGravity(newGravDir, newGravStrength, true);

        // Always aligns gravity direction to strongest source
        float newGravStrength = Vector3.Dot(newGravDir.normalized, strongest.normalized) * strongest.magnitude; // takes component along strongest source
        strongest.Normalize();
        GlobalGravityControl.ChangeGravity(strongest, newGravStrength, false);
    }
}
