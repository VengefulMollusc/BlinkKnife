﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpLookAheadCollider : MonoBehaviour
{
    private Collider[] lookAheadColliders;
    private List<Collider> colliding;

    void OnEnable()
    {
        colliding = new List<Collider>();
        lookAheadColliders = GetComponents<Collider>();

        Enabled(false);
    }

    public void Enabled(bool _enabled)
    {
        foreach (Collider col in lookAheadColliders)
        {
            col.enabled = _enabled;
        }
    }

    public void ProjectWarpPosition(Vector3 _warpPos, Vector3 _newGravVector)
    {
        transform.up = -_newGravVector;
        ProjectWarpPosition(_warpPos);
    }

    public void ProjectWarpPosition(Vector3 _warpPos, Quaternion _rotation)
    {
        transform.rotation = _rotation;
        ProjectWarpPosition(_warpPos);
    }

    public void ProjectWarpPosition(Vector3 _warpPos)
    {
        transform.position = _warpPos;
        Enabled(true);
    }
    
    // should return true only if the internal trigger collider is not touching any other colliders
    public bool CanWarp()
    {
        return colliding.Count <= 0;
    }

    public Vector3 WarpPosition()
    {
        return transform.position;
    }

    void OnTriggerStay(Collider col)
    {
        if (!colliding.Contains(col) && !col.isTrigger)
            colliding.Add(col);
    }

    void OnTriggerExit(Collider col)
    {
        colliding.Remove(col);
    }
}
