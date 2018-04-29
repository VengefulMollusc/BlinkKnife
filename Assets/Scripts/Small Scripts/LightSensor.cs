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

    [SerializeField] private LayerMask raycastMask;

    // TODO: remove if done testing - purely for debugging
    private RaycastHit hitInfo; 

    void OnEnable ()
	{
	    sunlightObject = GameObject.FindGameObjectWithTag("Sunlight");

	    if (sunlightObject == null)
	        Debug.LogError("No Sunlight object found");

	    isLit = false;

        InvokeRepeating("CheckLights", 0f, updateFrequency);
	}
	
	void CheckLights () {
		// raycast in opposite direction to sunlight direction for long distance
        // if another object is hit then this gameobject is in shadow
	    bool isInSunlight = !Physics.Raycast(transform.position, -sunlightObject.transform.forward, out hitInfo, raycastLength, raycastMask);

        // This is nowhere near as clear as the inspector method
        //Debug.DrawRay(transform.position, -sunlightObject.transform.forward * ((isLit) ? raycastLength : hitInfo.distance), ((isLit) ? Color.green : Color.red));

	    // TODO: also need a way of tracking and checking local light sources
	    bool isInLocalLight = false;

	    isLit = isInSunlight || isInLocalLight;

        // send notification of light status (for UI etc)
	    this.PostNotification(LightStatusNotification, new Info<GameObject, bool>(gameObject, isLit));
    }

    // TODO: debugging method for inspector
    public Info<Vector3, Vector3, bool> GetRaycastInfo()
    {
        if (sunlightObject == null)
            return null;

        return new Info<Vector3, Vector3, bool>(transform.position, transform.position - (sunlightObject.transform.forward * ((isLit) ? raycastLength : hitInfo.distance)), isLit);
    }

    public bool IsLit()
    {
        return isLit;
    }
}
