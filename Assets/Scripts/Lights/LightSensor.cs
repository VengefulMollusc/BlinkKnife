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

        //   // add custom points to list
        //   // TODO: remove if can just use list
        //customLightCheckPoints = new List<Vector3>();
        //foreach (Vector3 p in customLightCheckPoints)
        //    customLightCheckPoints.Add(p);

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
        // TODO: THIS IS THE CORRECT LOGIC - commenting out for testing
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
        bool inSunLight = false;
        RaycastHit hitInfo;
        foreach (Vector3 point in points)
        {
            if (!Physics.Raycast(point, -sunlightObject.transform.forward, out hitInfo, sunCheckRaycastLength,
                raycastMask))
            {
                rays.Add(-sunlightObject.transform.forward * sunCheckRaycastLength);
                hits.Add(true);
                inSunLight = true;
            }
            else
            {
                rays.Add(-sunlightObject.transform.forward * hitInfo.distance);
                hits.Add(false);
            }
        }
        testInfo = new Info<List<Vector3>, List<Vector3>, List<bool>>(points, rays, hits);
        return inSunLight;
    }

    /*
     * returns a list of points to check for this LightSensor
     */
    public List<Vector3> GetLightCheckPoints(Vector3 _lightDirection)
    {
        if (!useCustomLightCheckPoints || customLightCheckPoints.Count == 0)
            return GetDefaultLightCheckPoints(_lightDirection);

        if (rotateLightCheckAllAxis || rotateLightCheckHorOnly)
        {
            // return rotated custom points
            Vector3 dir = (rotateLightCheckHorOnly)
                ? Vector3.ProjectOnPlane(_lightDirection, transform.up)
                : _lightDirection;
            Quaternion rot = Quaternion.FromToRotation(transform.forward, dir);
            List<Vector3> rotatedPoints = new List<Vector3>();
            foreach (Vector3 point in customLightCheckPoints)
                rotatedPoints.Add(transform.TransformPoint(rot * point));
            return rotatedPoints;
        }

        // return custom points ignoring direction
        List<Vector3> points = new List<Vector3>();
        foreach (Vector3 point in customLightCheckPoints)
            points.Add(transform.TransformPoint(point));
        return points;
    }

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
