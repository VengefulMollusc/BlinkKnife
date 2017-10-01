using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Attaches to a planet sphere object, and contributes to the cumulative gravity on the player
 */
public class PlanetaryGravitySource : MonoBehaviour
{

    [SerializeField] private float gravDistFromSurface = 100f;
    [SerializeField] private float gravStrength = 35f;

    private float planetRadius;
    private float maxGravDist;

    private Transform target;

    void OnEnable()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;

        planetRadius = transform.localScale.x / 2f;
        maxGravDist = planetRadius + gravDistFromSurface;
    }

    /**
     * Stop player flying into oblivion? :/
     * 
     * TODO: refactor so strength and range are both relative to planetRadius (by default - still have override options)
     */ 
    public Vector3 GetGravityVector()
    {
        if (target == null)
            return Vector3.zero;

        Vector3 dirToPlayer = target.position - transform.position;
        float distToPlayer = dirToPlayer.magnitude;

        float strengthRatio = Utilities.MapValues(distToPlayer, planetRadius, maxGravDist, 1f, 0f, true);

        float currentStrength = strengthRatio * strengthRatio * gravStrength; // exponential dropoff

        return dirToPlayer.normalized * currentStrength;
    }
}
