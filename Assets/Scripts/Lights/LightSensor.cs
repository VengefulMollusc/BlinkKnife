using System.Collections.Generic;
using UnityEngine;

public class LightSensor : MonoBehaviour
{
    private static float updateFrequency = 0.1f;
    private static float sunCheckRaycastLength = 200f;

    [SerializeField] private bool useLitPercent;
    [SerializeField] private bool useCustomLightCheckPoints;
    [SerializeField] private bool rotateLightCheckHorOnly;
    [SerializeField] private bool rotateLightCheckAllAxis;
    [SerializeField] private List<Vector3> customLightCheckPoints;

    private GameObject sunlightObject;
    private bool checkSunlight = true;

    private float sunLitPercent;
    private float localLitPercent;
    private float overallLitPercent;

    public const string LightStatusNotification = "LightSensor.LightStatusNotification";

    // TODO: remove - debugging thing
    private Info<List<Vector3>, List<Vector3>, List<bool>> testInfo;

    [SerializeField] private LayerMask raycastMask;

    void OnEnable()
    {
        sunlightObject = GameObject.FindGameObjectWithTag("Sunlight");

        if (sunlightObject == null)
        {
            Debug.LogError("No Sunlight object found");
            checkSunlight = false;
        }

        sunLitPercent = localLitPercent = 0f;

        // If no custom points are defined, use non-percent logic by default
        // Still checks transform.position, but less expensive
        if (!useCustomLightCheckPoints || customLightCheckPoints.Count == 0)
            useLitPercent = false;

        InvokeRepeating("CheckLights", 0f, updateFrequency);
    }

    /*
     * determines overall lit percentage of object relative to sunlight and local light
     * Sends notification with current lit value
     */
    /*
     * TODO: Consider alternate solutions re: raycast efficiency
     * one mention online of sweeptest being more efficient than ~3 or more raycasts.
     * Could add sweeptest or something similar before raycasts?
     * If nothing is hit we know we are fully lit.
     * Still have to perform raycasts if hits something though - worst case more expensive.
     */
    void CheckLights()
    {
        // raycast in opposite direction to sunlight direction for long distance
        // if another object is hit then this gameobject is in shadow
        if (checkSunlight)
            CheckSunlight();

        overallLitPercent = Mathf.Max(sunLitPercent, localLitPercent);

        // reset local light variable
        localLitPercent = 0f;

        // send notification of light status (for UI etc)
        this.PostNotification(LightStatusNotification, new Info<GameObject, float>(gameObject, overallLitPercent));
    }

    /*
     * Checks for sunlight at transform position and in a radius around there
     * 
     * NOTE: Due to how this logic works, a non-percent sensor with custom points does not 
     *      need to include the base transform position in the list of custom points
     *      (It's checked first anyway)
     */
    private void CheckSunlight()
    {
        // TODO: THIS IS THE SIMPLIFIED LOGIC - commenting out for testing
        //Vector3 sunLightDir = sunlightObject.transform.forward;
        //if (!useLitPercent)
        //{
        //    if (!Physics.Raycast(transform.position, -sunLightDir, sunCheckRaycastLength, raycastMask))
        //    {
        //        // If percent doesn't matter, check position first to save time
        //        sunLitPercent = 1f;
        //        return;
        //    }
        //    if (!UseCustomPoints())
        //        return;
        //}
        //int litCount = 0;
        //List<Vector3> points = GetLightCheckPoints(sunLightDir);
        //foreach (Vector3 point in points)
        //{
        //    if (!Physics.Raycast(point, -sunLightDir, sunCheckRaycastLength, raycastMask))
        //    {
        //        if (!useLitPercent)
        //        {
        //            sunLitPercent = 1f;
        //            return;
        //        }
        //        litCount++;
        //    }
        //}
        //sunLitPercent = (float)litCount / points.Count;


        // TODO: REMOVE
        // - Expanded logic to allow inspector script to draw/debug all lightCheckPoints and rays
        Vector3 sunLightDir = sunlightObject.transform.forward;
        List<Vector3> points = GetLightCheckPoints(sunLightDir);
        List<Vector3> rays = new List<Vector3>();
        List<bool> hits = new List<bool>();
        int litCount = 0;
        RaycastHit hitInfo;
        foreach (Vector3 point in points)
        {
            if (!Physics.Raycast(point, -sunLightDir, out hitInfo, sunCheckRaycastLength,
                raycastMask))
            {
                rays.Add(-sunLightDir * sunCheckRaycastLength);
                hits.Add(true);

                litCount++;
            }
            else
            {
                rays.Add(-sunLightDir * hitInfo.distance);
                hits.Add(false);
            }
        }
        testInfo = new Info<List<Vector3>, List<Vector3>, List<bool>>(points, rays, hits);
        sunLitPercent = (float)litCount / points.Count;
    }

    /*
     * returns a list of points to check for this LightSensor
     */
    public List<Vector3> GetLightCheckPoints(Vector3 _lightDirection)
    {
        // If no custom points are specified, return just the object position
        // - mainly for small/simple objects
        // TODO: TEST - THIS SHOULD NEVER TRIGGER (OnEnable checks for this)
        if (!useCustomLightCheckPoints || customLightCheckPoints.Count == 0)
        {
            Debug.LogError("default LightCheckPoint generated for object that should not have any custom points");
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
            foreach (Vector3 point in customLightCheckPoints)
                rotatedPoints.Add(transform.position + (rot * point));
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

    /*
     * returns a list of points arranged in a circle and rotated to face the light.
     * (was) used by default when no custom points are defined
     */
    //private List<Vector3> GetDefaultLightCheckPoints(Vector3 _lightDirection)
    //{
    //    // return default points defined by radius
    //    Vector3 up;
    //    if (Vector3.Angle(transform.up, _lightDirection) > 45f)
    //        up = Vector3.Cross(transform.up, _lightDirection).normalized * lightCheckRadius;
    //    else
    //        up = Vector3.Cross(transform.right, _lightDirection).normalized * lightCheckRadius;

    //    Vector3 right = Vector3.Cross(up, _lightDirection).normalized * lightCheckRadius;

    //    return new List<Vector3>()
    //    {
    //        transform.position,
    //        transform.position + up,
    //        transform.position - up,
    //        transform.position + right,
    //        transform.position - right
    //    };
    //}

    // TODO: REMOVE. debugging method for inspector
    public Info<List<Vector3>, List<Vector3>, List<bool>> GetRaycastInfo()
    {
        if (sunlightObject == null)
            return null;

        return testInfo;
    }

    /*
     * returns true if the object is lit at all, regardless of percentage
     */
    //public bool IsLit()
    //{
    //    return overallLitPercent > 0f;
    //}

    /*
     * returns the percentage of the object that is lit.
     * Defined by the fraction of lightcheckpoints that are illuminated
     */
    public float GetLitPercent()
    {
        return overallLitPercent;
    }

    /*
     * returns whether to use full lit percentage calculations
     * used by lightSources to simplify logic and reduce raycasts if this is false
     */
    public bool UseLitPercent()
    {
        return useLitPercent;
    }

    /*
     * returns whether to use custom check points
     * also used by lightSources to simplify logic and reduce raycasts if this is false
     */
    public bool UseCustomPoints()
    {
        return useCustomLightCheckPoints && customLightCheckPoints.Count > 0;
    }

    // called by LightSource objects when sensor is lit by the object
    public void LightObject(float _litPercent = 1f)
    {
        if (_litPercent > localLitPercent)
            localLitPercent = _litPercent;
    }

    // used to enable/disable the sunlight check (for mainly dark environments with no global lighting)
    public void EnableSunlightCheck(bool _checkSunlight)
    {
        checkSunlight = _checkSunlight;
        if (!checkSunlight)
            sunLitPercent = 0f;
    }
}
