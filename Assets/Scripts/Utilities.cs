using UnityEngine;
using System.Collections;

public class Utilities : MonoBehaviour {

    /*
     * Ignores or allows collisions between multiple colliders at once
     */
    //public static void IgnoreCollisions(Collider[] _this, Collider[] _other, bool _ignore)
    //{
    //    foreach (Collider col1 in _this)
    //    {
    //        IgnoreCollisions(col1, _other, _ignore);
    //    }
    //}

    //public static void IgnoreCollisions(Collider _this, Collider[] _other, bool _ignore)
    //{
    //    foreach (Collider col in _other)
    //    {
    //        Physics.IgnoreCollision(_this, col, _ignore);
    //    }
    //}

	/*
     * Returns true if the second transform is in front of the first
     * (ie t1 is looking within 90 degrees of t2)
     */
	public static bool IsInFront (Transform t1, Transform t2){
		Vector3 relativePoint = t1.InverseTransformPoint (t2.position);
		return relativePoint.z > 0f;
	}

    /*
     * Returns true if the given point is on the screen
     */
    public static bool IsOnScreen(Vector2 _target)
    {
        return !(_target.x > Screen.width || _target.x < 0 || _target.y > Screen.height || _target.y < 0);
    }

    /*
     * A simple map function, maps a value from one range to another
     */
    public static float MapValues(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static float MapValues(float value, float from1, float to1, float from2, float to2, bool clampValues)
    {
        if (clampValues)
            value = Mathf.Clamp(value, from1, to1);
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    /*
     * Creates an explosion at the given position,
     * requires position, radius, damage to health and force applied to rigidbodies.
     * 
     * Checks all colliders in radius of blast, raycasts to see if collider is obscured/behind an object
     * if not obscured, applies force to rigidbodies relative to distance from blast
     * and applies health damage to anything with a HealthController component
     */
    public static void CreateExplosion(Vector3 _position, float _radius, float _damage, float _force)
    {
        Collider[] colliders = Physics.OverlapSphere(_position, _radius);
        foreach (Collider col in colliders)
        {
            RaycastHit hit;
            bool exposed = false;
            Vector3 colPos = col.transform.position;

            // check if col is behind cover
            if (Physics.Raycast(_position, (colPos - _position), out hit))
            {
                exposed = (hit.collider == col);
            }

            if (exposed)
            {
                // apply explosion force to rigidbodies
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(_force, _position, _radius, 0f, ForceMode.VelocityChange);
                }

                // check if can recieve damage then do damage relative to distance from explosion
                HealthController hc = col.GetComponent<HealthController>();
                if (hc != null)
                {
                    float dist = Vector3.Distance(_position, colPos);
                    float i = MapValues(dist, 0f, _radius, 1f, 0.2f);
                    hc.Damage(_damage * i);
                }
            }
        }
    }

    /*
     * Lerps with t along the bezier curve defined by p0-3
     */
    public static Vector3 LerpBezier(Info<Vector3, Vector3, Vector3, Vector3> _bezier, float _t)
    {
        return LerpBezier(_bezier.arg0, _bezier.arg1, _bezier.arg2, _bezier.arg3, _t);
    }

    public static Vector3 LerpBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float r = 1f - t;
        float f0 = r * r * r;
        float f1 = r * r * t * 3;
        float f2 = r * t * t * 3;
        float f3 = t * t * t;
        return f0 * p0 + f1 * p1 + f2 * p2 + f3 * p3;
    }

    public static Vector3 BezierDerivative(Info<Vector3, Vector3, Vector3, Vector3> _bezier, float _t)
    {
        return BezierDerivative(_bezier.arg0, _bezier.arg1, _bezier.arg2, _bezier.arg3, _t);
    }

    public static Vector3 BezierDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = Mathf.Clamp01(t);
        float r = 1f - t;
        return
            3f * r * r * (p1 - p0) +
            6f * r * t * (p2 - p1) +
            3f * t * t * (p3 - p2);
    }

    /*
     * Gives a rough estimate of the length of a cubic bezier curve
     * 
     * used for duration calculations in fibre optics
     */
    public static float BezierLengthEstimate(Info<Vector3, Vector3, Vector3, Vector3> _bezier)
    {
        return BezierLengthEstimate(_bezier.arg0, _bezier.arg1, _bezier.arg2, _bezier.arg3);
    }

    public static float BezierLengthEstimate(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float chord = (p3 - p0).magnitude;
        float controlNet = (p0 - p1).magnitude + (p2 - p1).magnitude + (p3 - p2).magnitude;

        return (controlNet + chord) / 2;
    }

    //public static Color ChangeColorBrightness(Color color, float correctionFactor)
    //{
    //    float red = color.r;
    //    float green = color.g;
    //    float blue =color.b;

    //    if (correctionFactor < 0)
    //    {
    //        correctionFactor = 1 + correctionFactor;
    //        red *= correctionFactor;
    //        green *= correctionFactor;
    //        blue *= correctionFactor;
    //    }
    //    else
    //    {
    //        red = (255 - red) * correctionFactor + red;
    //        green = (255 - green) * correctionFactor + green;
    //        blue = (255 - blue) * correctionFactor + blue;
    //    }

    //    return new Color((int)red, (int)green, (int)blue, color.a);
    //}







    /*
     * Methods for projectile intercept of moving target.
     * Also takes into account velocity of shooter 
     * (unsure if necessary - should only be needed if shots inherit velocity).
     * 
     * Needs position and velocity of both source and target objects.
     * Also needs shot speed;
     */
    public static Vector3 InterceptPosition(Vector3 _sourcePos, Vector3 _sourceVel, Vector3 _targetPos, Vector3 _targetVel, float _shotSpeed)
    {
        Vector3 targetRelPos = _targetPos - _sourcePos;
        Vector3 targetRelVel = _targetVel - _sourceVel;

        float t = InterceptTime(targetRelPos, targetRelVel, _shotSpeed);

        return _targetPos + (t * targetRelVel);
    }

    /*
     * Calculates the lead time needed to intercept a moving object with a shot
     * 
     * Needs the relative position and velocity of the target, and the speed of the shot
     */
    public static float InterceptTime(Vector3 _targetRelPos, Vector3 _targetRelVel, float _shotSpeed)
    {
        float velSquared = _targetRelVel.sqrMagnitude;
        if (velSquared < 0.001f)
        {
            return 0f;
        }

        // Maths here
        float a = velSquared - (_shotSpeed * _shotSpeed);

        // handle similar velocities
        if (Mathf.Abs(a) < 0.001f)
        {
            float t = -_targetRelPos.sqrMagnitude /
                (2f*Vector3.Dot(_targetRelVel, _targetRelPos));
            return Mathf.Max(t, 0f); // dont shoot back in time

        }

        float b = 2f * Vector3.Dot(_targetRelVel, _targetRelPos);
        float c = _targetRelPos.sqrMagnitude;
        float determinant = (b * b) - (4f * a * c);

        if (determinant > 0f) 
        { 
            // two intercept paths
            float sqrtDeterminant = Mathf.Sqrt(determinant);
            float t1 = (-b + sqrtDeterminant) / (2f * a);
            float t2 = (-b - sqrtDeterminant) / (2f * a);

            if (t1 > 0f)
            {
                if (t2 > 0f)
                {
                    // both are positive
                    return Mathf.Min(t1, t2);
                } else
                {
                    // only t1 is positive
                    return t1;
                }
            } else
            {
                return Mathf.Max(t2, 0f); // dont shoot back in time
            }
        } else if (determinant < 0f)
        {
            // no intercept path
            return 0f;
        } else
        {
            // determinant == 0, one intercept path
            return Mathf.Max(-b/(2f*a), 0f);
        }
    }

    /*
     * Calculates the angle needed to land a projectile on a given point.
     * 
     * Needs the origin and target positions, as well as the speed of the shot.
     * Optionally needs gravity value (if different from global Physics gravity amount).
     * 
     * Currently returns only positive firing angle,
     * but negative might be useful for closer targets.
     */
    public static float CalculateProjectileAngle(Vector3 _origin, Vector3 _target, float _shotSpeed)
    {
        float dist = (_target - _origin).magnitude;
        float yDiff = _target.y - _origin.y;
        float grav = Physics.gravity.y; // can replace this with parameter value if needed

        float sqrt = (_shotSpeed * _shotSpeed * _shotSpeed * _shotSpeed) - (grav * (grav * (dist * dist) + 2 * yDiff * (_shotSpeed * _shotSpeed)));

        if (sqrt < 0)
        {
            Debug.Log("No solution");
            return 0;
        }

        sqrt = Mathf.Sqrt(sqrt);

        // could use either of these, depending on if you need to aim down to hit something
        // NOTE: Positive angle will always work if there is a solution
        var anglePos = Mathf.Atan(((_shotSpeed * _shotSpeed) + sqrt) / (grav * dist));
        // var angleNeg = Mathf.Atan(((_shotSpeed * _shotSpeed) - sqrt) / (grav * dist));

        return anglePos * Mathf.Rad2Deg;
    }
}
