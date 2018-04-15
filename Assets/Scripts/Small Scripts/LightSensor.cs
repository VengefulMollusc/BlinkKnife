using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSensor : MonoBehaviour
{
    public static float updateFrequency = 0.1f;
    public static float raycastLength = 100f;

    private GameObject sunlightObject;

    private bool isLit;

    public const string LightStatusNotification = "LightSensor.LightStatusNotification";

    void OnEnable ()
	{
	    sunlightObject = GameObject.FindGameObjectWithTag("Sunlight");

	    if (sunlightObject == null)
	    {
	        Debug.LogError("No Sunlight object found");
	    }

	    isLit = false;

        InvokeRepeating("CheckLights", 0f, updateFrequency);
	}
	
	void CheckLights () {
		// raycast in opposite direction to sunlight direction for long distance
        // if another object is hit then this gameobject is in shadow
	    bool isInSunlight = !Physics.Raycast(transform.position, -sunlightObject.transform.forward, raycastLength);

	    // TODO: also need a way of tracking and checking local light sources
	    bool isInLocalLight = false;

	    isLit = isInSunlight || isInLocalLight;

        // send notification of light status (for UI etc)
	    this.PostNotification(LightStatusNotification, new Info<GameObject, bool>(gameObject, isLit));
    }

    public bool IsLit()
    {
        return isLit;
    }
}
