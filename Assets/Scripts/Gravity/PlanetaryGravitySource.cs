using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Attaches to a planet sphere object, and contributes to the cumulative gravity on the player
 */
public class PlanetaryGravitySource : MonoBehaviour
{

    [SerializeField] private float maxGravDist = 100f;
    [SerializeField] private float gravStrength = 35f;

    private Transform target;

    void OnEnable()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    /**
     * Maybe make gravity dropoff non-linear? Would allow a much longer tail to gravity wells
     * Stop player flying into oblivion :/
     */ 
    public Vector3 GetGravityVector()
    {
        Vector3 dirToPlayer = target.position - transform.position;
        float distToPlayer = dirToPlayer.magnitude;

        float currentStrength = Utilities.MapValues(distToPlayer, 0f, maxGravDist, gravStrength, 0f, true);

        return dirToPlayer.normalized * currentStrength;
    }
}
