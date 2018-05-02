using System.Collections.Generic;
using UnityEngine;

public class LightSensor : MonoBehaviour
{
    private static float updateFrequency = 0.1f;
    private static float sunCheckRaycastLength = 200f;

    public float lightCheckRadius = 0.5f;
    public bool useCustomLightCheckPoints;
    [SerializeField] private bool rotateLightCheckHorOnly;
    [SerializeField] private bool rotateLightCheckAllAxis;
    //public Vector3[] customLightCheckPoints;
    public List<Vector3> customLightCheckPoints;

    private GameObject sunlightObject;
    private bool checkSunlight = true;

    private bool isLit;
    private float litPercent;
    private bool isInLocalLight;

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

        isLit = false;

        InvokeRepeating("CheckLights", 0f, updateFrequency);
    }

    void CheckLights()
    {
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
        // TODO: THIS IS THE SIMPLIFIED LOGIC - commenting out for testing
        //foreach (Vector3 point in GetLightCheckPoints(sunlightObject.transform.forward))
        //{
        //    if (!Physics.Raycast(point, -sunlightObject.transform.forward, sunCheckRaycastLength,
        //        raycastMask))
        //        return true;
        //}

        //return false;

        // TODO: remove this
        List<Vector3> points = GetLightCheckPoints(sunlightObject.transform.forward);
        List<Vector3> rays = new List<Vector3>();
        List<bool> hits = new List<bool>();
        int litCount = 0;
        RaycastHit hitInfo;
        foreach (Vector3 point in points)
        {
            if (!Physics.Raycast(point, -sunlightObject.transform.forward, out hitInfo, sunCheckRaycastLength,
                raycastMask))
            {
                rays.Add(-sunlightObject.transform.forward * sunCheckRaycastLength);
                hits.Add(true);

                litCount++;
            }
            else
            {
                rays.Add(-sunlightObject.transform.forward * hitInfo.distance);
                hits.Add(false);
            }
        }
        testInfo = new Info<List<Vector3>, List<Vector3>, List<bool>>(points, rays, hits);

        litPercent = (float)litCount / points.Count;

        return litCount > 0;
    }

    /*
     * returns a list of points to check for this LightSensor
     */
    public List<Vector3> GetLightCheckPoints(Vector3 _lightDirection)
    {
        if (!useCustomLightCheckPoints || customLightCheckPoints.Count == 0)
            return GetDefaultLightCheckPoints(_lightDirection);

        if (rotateLightCheckAllAxis || rotateLightCheckHorOnly)
            return GetRotatedPoints(_lightDirection);

        // return custom points ignoring direction
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
        // return rotated custom points
        List<Vector3> rotatedPoints = new List<Vector3>();
        if (rotateLightCheckAllAxis)
        {
            Quaternion rot = Quaternion.LookRotation(-_lightDirection, transform.up);
            foreach (Vector3 point in customLightCheckPoints)
                rotatedPoints.Add(transform.position + (rot * point));
        }
        else
        {
            Vector3 flatDir = Vector3.ProjectOnPlane(-_lightDirection, transform.up);
            Quaternion horRot = Quaternion.FromToRotation(transform.forward, flatDir);
            foreach (Vector3 point in customLightCheckPoints)
                rotatedPoints.Add(transform.TransformPoint(horRot * point));
        }
        return rotatedPoints;
    }

    /*
     * returns a list of points arranged in a circle and rotated to face the light.
     * Used by default when no custom points are defined
     */
    private List<Vector3> GetDefaultLightCheckPoints(Vector3 _lightDirection)
    {
        // return default points defined by radius
        Vector3 up;
        if (Vector3.Angle(transform.up, _lightDirection) > 45f)
            up = Vector3.Cross(transform.up, _lightDirection).normalized * lightCheckRadius;
        else
            up = Vector3.Cross(transform.right, _lightDirection).normalized * lightCheckRadius;

        Vector3 right = Vector3.Cross(up, _lightDirection).normalized * lightCheckRadius;

        return new List<Vector3>()
        {
            transform.position,
            transform.position + up,
            transform.position - up,
            transform.position + right,
            transform.position - right
        };
    }

    // TODO: REMOVE? debugging method for inspector
    public Info<List<Vector3>, List<Vector3>, List<bool>> GetRaycastInfo()
    {
        if (sunlightObject == null)
            return null;

        return testInfo;
    }

    /*
     * returns true if the object is lit at all, regardless of percentage
     */
    public bool IsLit()
    {
        return litPercent > 0f;
    }

    /*
     * returns the percentage of the object that is lit.
     * Defined by the fraction of lightcheckpoints that are illuminated
     */
    public float GetLitPercent()
    {
        return litPercent;
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
