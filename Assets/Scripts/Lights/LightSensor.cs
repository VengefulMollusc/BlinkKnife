using System.Collections.Generic;
using UnityEngine;

public class LightSensor : MonoBehaviour
{
    private static float updateFrequency = 0.1f;
    private static float sunCheckRaycastLength = 200f;

    public float lightCheckRadius = 0.5f;
    public bool useCustomLightCheckPoints;
    public bool rotateLightCheckHorOnly;
    public bool rotateLightCheckAllAxis;
    public Vector3[] customLightCheckPoints;
    private List<Vector3> lightCheckPoints;

    private GameObject sunlightObject;
    private bool checkSunlight = true;

    private bool isLit;
    private bool isInLocalLight;

    public const string LightStatusNotification = "LightSensor.LightStatusNotification";

    [SerializeField] private LayerMask raycastMask;

    // TODO: remove if done testing - purely for debugging
    private RaycastHit hitInfo; 

    void OnEnable ()
	{
	    sunlightObject = GameObject.FindGameObjectWithTag("Sunlight");

	    if (sunlightObject == null)
	    {
	        Debug.LogError("No Sunlight object found");
	        checkSunlight = false;
	    }

	    isLit = false;

        // add custom points to list
        // TODO: remove if can just use list
	    lightCheckPoints = new List<Vector3>();
	    foreach (Vector3 p in customLightCheckPoints)
	        lightCheckPoints.Add(p);

        InvokeRepeating("CheckLights", 0f, updateFrequency);
	}
	
	void CheckLights () {
		// raycast in opposite direction to sunlight direction for long distance
        // if another object is hit then this gameobject is in shadow
	    bool isInSunlight = checkSunlight && IsInSunlight();

        // This is nowhere near as clear as the inspector method
        //Debug.DrawRay(transform.position, -sunlightObject.transform.forward * ((isLit) ? sunCheckRaycastLength : hitInfo.distance), ((isLit) ? Color.green : Color.red));

	    isLit = isInSunlight || isInLocalLight;

        // reset local light variable
	    isInLocalLight = false;

        // send notification of light status (for UI etc)
        this.PostNotification(LightStatusNotification, new Info<GameObject, bool>(gameObject, isLit));
    }

    /*
     * Checks for sunlight at transform position and in a radius around there
     */
    private bool IsInSunlight()
    {
        foreach (Vector3 point in GetLightCheckPoints(sunlightObject.transform.forward))
        {
            if (!Physics.Raycast(point, -sunlightObject.transform.forward, out hitInfo, sunCheckRaycastLength,
                raycastMask))
                return true;
        }

        return false;
    }

    /*
     * returns a list of points to check for this LightSensor
     */
    public List<Vector3> GetLightCheckPoints(Vector3 _dir)
    {
        if (!useCustomLightCheckPoints || customLightCheckPoints.Length == 0)
        {
            // return default points defined by radius
            Vector3 up;
            if (Vector3.Angle(transform.up, _dir) > 45f)
                up = Vector3.Cross(transform.up, _dir).normalized * lightCheckRadius;
            else
                up = Vector3.Cross(transform.right, _dir).normalized * lightCheckRadius;

            Vector3 right = Vector3.Cross(up, _dir).normalized * lightCheckRadius;

            return new List<Vector3>(){transform.position,
                transform.position + up,
                transform.position - up,
                transform.position + right,
                transform.position - right
            };
        }

        if (rotateLightCheckHorOnly)
        {
            Vector3 flattenedDir = Vector3.ProjectOnPlane(_dir, transform.up);
            Quaternion rot = Quaternion.FromToRotation(transform.forward, flattenedDir);
            List<Vector3> rotatedPoints = new List<Vector3>();
            foreach (Vector3 point in lightCheckPoints)
                rotatedPoints.Add(transform.position + (rot * point));
            return rotatedPoints;
        }

        if (rotateLightCheckAllAxis)
        {
            Quaternion rot = Quaternion.FromToRotation(transform.forward, _dir);
            List<Vector3> rotatedPoints = new List<Vector3>();
            foreach (Vector3 point in lightCheckPoints)
                rotatedPoints.Add(transform.position + (rot * point));
            return rotatedPoints;
        }

        List<Vector3> points = new List<Vector3>();
        foreach (Vector3 point in lightCheckPoints)
            points.Add(transform.position + point);
        return points;
    }

    // TODO: REMOVE? debugging method for inspector
    public Info<Vector3, Vector3, bool> GetRaycastInfo()
    {
        if (sunlightObject == null)
            return null;

        return new Info<Vector3, Vector3, bool>(transform.position, transform.position - (sunlightObject.transform.forward * sunCheckRaycastLength), isLit);
    }

    public bool IsLit()
    {
        return isLit;
    }

    // called by LightSource objects when sensor is within range
    public void LightObject()
    {
        isInLocalLight = true;
    }

    // used to enable/disable the sunlight check (for mainly dark environments with no global lighting)
    public void EnableSunlightCheck(bool _checkSunlight)
    {
        checkSunlight = _checkSunlight;
    }
}
