using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSensor : MonoBehaviour
{

    private GameObject sunlightObject;

    private bool isLit;

	void OnEnable ()
	{
	    sunlightObject = GameObject.FindGameObjectWithTag("Sunlight");

	    if (sunlightObject == null)
	    {
	        Debug.LogError("No Sunlight object found");
	    }

	    isLit = false;
	}
	
	void FixedUpdate () {
		// raycast in opposite direction to sunlight direction for long distance
        // if another object is hit then this gameobject is in shadow

        // also need a way of tracking and checking local light sources
	}

    public bool IsLit()
    {
        return isLit;
    }
}
