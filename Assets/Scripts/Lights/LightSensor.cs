using System.Collections.Generic;
using UnityEngine;

public class LightSensor : MonoBehaviour
{
    private static float updateFrequency = 0.1f;
    private static float sunCheckRaycastLength = 200f;

    [SerializeField] private bool useLitPercent = false;
    public float lightCheckRadius = 0.5f;
    public bool useCustomLightCheckPoints;
    [SerializeField] private bool rotateLightCheckHorOnly;
    [SerializeField] private bool rotateLightCheckAllAxis;
    public List<Vector3> customLightCheckPoints;

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

        InvokeRepeating("CheckLights", 0f, updateFrequency);
    }

    void CheckLights()
    {
        // raycast in opposite direction to sunlight direction for long distance
        // if another object is hit then this gameobject is in shadow
        if (checkSunlight)
            CheckSunlight();

        //isLit = isInSunlight || isInLocalLight;
        overallLitPercent = Mathf.Max(sunLitPercent, localLitPercent);

        // reset local light variable
        //isInLocalLight = false;
        localLitPercent = 0f;

        // send notification of light status (for UI etc)
        this.PostNotification(LightStatusNotification, new Info<GameObject, float>(gameObject, overallLitPercent));
    }

    /*
     * Checks for sunlight at transform position and in a radius around there
     */
    private void CheckSunlight()
    {
        // TODO: THIS IS THE SIMPLIFIED LOGIC - commenting out for testing
        //int litCount = 0;
        //Vector3 sunLightDir = sunlightObject.transform.forward;
        //foreach (Vector3 point in GetLightCheckPoints(sunLightDir))
        //{
        //    if (!Physics.Raycast(point, -sunLightDir, sunCheckRaycastLength,
        //        raycastMask))
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


        // TODO: remove this
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
        return overallLitPercent > 0f;
    }

    /*
     * returns the percentage of the object that is lit.
     * Defined by the fraction of lightcheckpoints that are illuminated
     */
    public float GetLitPercent()
    {
        return overallLitPercent;
    }

    public bool UseLitPercent()
    {
        return useLitPercent;
    }

    // called by LightSource objects when sensor is within range
    public void LightObject(float _litPercent)
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
