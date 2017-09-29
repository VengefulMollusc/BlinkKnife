﻿using UnityEngine;
using System.Collections;

public class GravityPanel : MonoBehaviour {

    [SerializeField]
    private bool customGravity = false;
    [SerializeField]
    private Vector3 customGravUpVector;

    [SerializeField]
    private bool useSurfaceNormal = false;

    /*
     * return the gravity vector this panel will shift to
     */
	public Vector3 GetGravityVector()
    {
        if (useSurfaceNormal) return Vector3.zero;
        if (customGravity) return customGravUpVector;
        return transform.up;
    }
}