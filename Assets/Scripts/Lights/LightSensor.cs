using System.Collections.Generic;
using UnityEngine;

public class LightSensor : MonoBehaviour
{
    /*
     * Should be attached to objects that need to know whether they are lit or not.
     * 
     * Handles logic for checking sunlight, and contains a list of points to be used for checking scene lights
     */
    private const float sunCheckRaycastLength = 200f;

    [SerializeField] private bool useCustomLightCheckPoints;
    [SerializeField] private bool rotateLightCheckHorOnly;
    [SerializeField] private bool rotateLightCheckAllAxis;
    [SerializeField] private List<Vector3> customLightCheckPoints;

    private GameObject sunlightObject;
    private Light sunLight;
    private bool checkSunlight = true;

    private float sunIntensity;
    private float localIntensity;
    private float overallIntensity;

    public const string LightStatusNotification = "LightSensor.LightStatusNotification";

    // TODO: remove - debugging thing
    private Info<List<Vector3>, List<Vector3>, List<bool>> testInfo;

    [SerializeField] private LayerMask raycastMask;

    void Start()
    {
        sunlightObject = GameObject.FindGameObjectWithTag("Sunlight");

        if (sunlightObject == null)
        {
            Debug.Log("No Sunlight object found");
            checkSunlight = false;
        }
        else
            sunLight = sunlightObject.GetComponent<Light>();

        sunIntensity = localIntensity = 0f;

        // If no custom points are defined, use non-custom logic by default
        if (customLightCheckPoints.Count == 0)
            useCustomLightCheckPoints = false;
    }

    void OnEnable()
    {
        InvokeRepeating("CheckLights", 0f, GlobalVariableController.LightCheckUpdateFrequency);
    }

    void OnDisable()
    {
        CancelInvoke("CheckLights");
    }

    /*
     * determines overall lit percentage of object relative to sunlight and local light
     * Sends notification with current lit value
     */
    void CheckLights()
    {
        // raycast in opposite direction to sunlight direction for long distance
        // if another object is hit then this gameobject is in shadow
        sunIntensity = GetSunIntensity();

        overallIntensity = Mathf.Max(sunIntensity, localIntensity);

        // reset local light variable
        localIntensity = 0f;

        // send notification of light status (for UI etc)
        this.PostNotification(LightStatusNotification, new Info<GameObject, float>(gameObject, overallIntensity));
    }

    /*
     * Checks for sunlight on the object
     * 
     * NOTE: Due to how this logic works, custom points list does not 
     *      need to include the base transform position
     */
    private float GetSunIntensity()
    {
        if (!checkSunlight)
            return 0f;

        //// TODO: THIS IS THE SIMPLIFIED LOGIC - comment out for testing
        Vector3 raycastDir = -sunlightObject.transform.forward;
        float currSunIntensity = sunLight.intensity;
        if (!Physics.Raycast(transform.position, raycastDir, sunCheckRaycastLength, raycastMask))
            // check position first to save time
            return currSunIntensity;

        // if no custom points given, go no further
        if (!UseCustomPoints())
            return 0f;

        List<Vector3> points = GetLightCheckPoints(-raycastDir);
        foreach (Vector3 point in points)
        {
            if (!Physics.Raycast(point, raycastDir, sunCheckRaycastLength, raycastMask))
                return currSunIntensity;
        }
        return 0f;


        // TODO: REMOVE - testing logic that collects raycast info
        // Same logic as above, but returns scene view test variables based on all performed raycasts
        //List<Vector3> testPoints = new List<Vector3>();
        //List<Vector3> rays = new List<Vector3>();
        //List<bool> hits = new List<bool>();
        //Vector3 raycastDir = -sunlightObject.transform.forward;
        //float currSunIntensity = sunLight.intensity;
        //RaycastHit hitInfo;
        //testPoints.Add(transform.position);
        //if (!Physics.Raycast(transform.position, raycastDir, out hitInfo, sunCheckRaycastLength, raycastMask))
        //{
        //    rays.Add(raycastDir * sunCheckRaycastLength);
        //    hits.Add(true);
        //    testInfo = new Info<List<Vector3>, List<Vector3>, List<bool>>(testPoints, rays, hits);
        //    // check position first to save time
        //    return currSunIntensity;
        //}
        //rays.Add(raycastDir * hitInfo.distance);
        //hits.Add(false);

        //// if no custom points given, go no further
        //if (!UseCustomPoints())
        //{
        //    testInfo = new Info<List<Vector3>, List<Vector3>, List<bool>>(testPoints, rays, hits);
        //    return 0f;
        //}

        //List<Vector3> points = GetLightCheckPoints(-raycastDir);
        //foreach (Vector3 point in points)
        //{
        //    testPoints.Add(point);
        //    if (!Physics.Raycast(point, raycastDir, out hitInfo, sunCheckRaycastLength, raycastMask))
        //    {
        //        rays.Add(raycastDir * sunCheckRaycastLength);
        //        hits.Add(true);
        //        testInfo = new Info<List<Vector3>, List<Vector3>, List<bool>>(testPoints, rays, hits);
        //        return currSunIntensity;
        //    }
        //    rays.Add(raycastDir * hitInfo.distance);
        //    hits.Add(false);
        //}
        //testInfo = new Info<List<Vector3>, List<Vector3>, List<bool>>(testPoints, rays, hits);
        //return 0f;
    }

    /*
     * returns a list of points to check for this LightSensor
     */
    /*
     * TODO: try this with code to order points front-to-back along lightDirection?
     * This may not actually affect anything enough to justify the cost
     */
    public List<Vector3> GetLightCheckPoints(Vector3 _lightDirection)
    {
        // If no custom points are specified, return just the object position
        // - mainly for small/simple objects
        // TODO: TEST - THIS SHOULD NEVER TRIGGER
        if (!useCustomLightCheckPoints || customLightCheckPoints.Count == 0)
        {
            Debug.LogError("GetLightCheckPoints called for object that has no custom points");
            return new List<Vector3>() { transform.position };
            //return GetDefaultLightCheckPoints(_lightDirection);
        }

        // return custom points rotated according to the light direction
        // - for cylindrical/symmetrical objects
        if (rotateLightCheckAllAxis || rotateLightCheckHorOnly)
            return GetRotatedPoints(_lightDirection);

        // return unmodified points (still relative to transform though)
        return GetUnmodifiedPoints();
    }

    public List<Vector3> GetUnmodifiedPoints()
    {
        // return custom points ignoring direction
        // - for more complex objects, or ones that have a distinct shape
        // Also for inspector testing
        List<Vector3> points = new List<Vector3>();
        foreach (Vector3 point in customLightCheckPoints)
            points.Add(transform.TransformPoint(point));
        return points;
    }

    /*
     * Returns a list of points rotated around all or just the vertical axes
     */
    private List<Vector3> GetRotatedPoints(Vector3 _lightDirection)
    {
        List<Vector3> rotatedPoints = new List<Vector3>();
        if (rotateLightCheckAllAxis)
        {
            // Rotate around all axes
            Quaternion rot = Quaternion.LookRotation(-_lightDirection, transform.up);
            Vector3 basePos = transform.position;
            foreach (Vector3 point in customLightCheckPoints)
                rotatedPoints.Add(basePos + (rot * point));
        }
        else
        {
            // Rotate only around vertical axis
            Vector3 flatDir = Vector3.ProjectOnPlane(-_lightDirection, transform.up);
            Quaternion horRot = Quaternion.FromToRotation(transform.forward, flatDir);
            foreach (Vector3 point in customLightCheckPoints)
                rotatedPoints.Add(transform.TransformPoint(horRot * point));
        }
        return rotatedPoints;
    }

    // TODO: REMOVE. debugging method for inspector
    public Info<List<Vector3>, List<Vector3>, List<bool>> GetRaycastInfo()
    {
        if (sunlightObject == null)
            return null;

        return testInfo;
    }

    /*
     * returns whether to use custom check points
     * also used by lightSources to simplify logic and reduce raycasts if this is false
     */
    public bool UseCustomPoints()
    {
        return useCustomLightCheckPoints;
    }

    // called by LightSource objects when sensor is lit by the object
    public void LightObject(float _intensity)
    {
        if (_intensity > localIntensity)
            localIntensity = _intensity;
    }

    // used to enable/disable the sunlight check (for mainly dark environments with no global lighting)
    public void EnableSunlightCheck(bool _checkSunlight)
    {
        checkSunlight = _checkSunlight;
        if (!checkSunlight)
            sunIntensity = 0f;
    }
}
