using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/**
 * Attaches to a planet sphere object, and contributes to the cumulative gravity on the player
 */
public class PlanetaryGravitySource : MonoBehaviour
{
    //[SerializeField] private bool overrideDefaultGravVals = false;
    [SerializeField] private float gravDistFromSurfaceOverride = 100f;
    [SerializeField] private float gravStrengthOverride = 35f;

    //private const float gravDistanceRatio = 3f;
    //private const float gravStrengthRatio = 1.5f;

    private float planetRadius;
    private float maxGravDist;
    private float gravDistance;
    private float gravStrength;

    private Transform target;

    void OnEnable()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;

        CalculateGravValues();
    }

    void CalculateGravValues()
    {
        planetRadius = transform.localScale.x / 2f;

        gravDistance = gravDistFromSurfaceOverride;
        gravStrength = gravStrengthOverride;

        //if (overrideDefaultGravVals)
        //{
        //    gravDistance = gravDistFromSurfaceOverride;
        //    gravStrength = gravStrengthOverride;
        //}
        //else
        //{
        //    // strength and range are both relative to planetRadius
        //    gravDistance = planetRadius * gravDistanceRatio;
        //    gravStrength = planetRadius * gravStrengthRatio;
        //}

        maxGravDist = planetRadius + gravDistance;
    }

    /**
     * Stop player flying into oblivion? :/
     * 
     */ 
    public Vector3 GetGravityVector()
    {
        if (target == null)
            return Vector3.zero;

        Vector3 dirToPlayer = target.position - transform.position;
        float distToPlayer = dirToPlayer.magnitude;

        float strengthRatio = Utilities.MapValues(distToPlayer, planetRadius, maxGravDist, 1f, 0f, true);

        float currentStrength = strengthRatio * strengthRatio * gravStrength; // exponential dropoff?

        return dirToPlayer.normalized * currentStrength;
    }
}
