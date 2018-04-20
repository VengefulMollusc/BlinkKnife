using UnityEngine;
using System.Collections;
using TriangleNet.Tools;


/**
 * May need modifications to allow 'planetary' gravity
 * ie: multiple spheres contributing gravity based on distance to sphere
 * track multiple gravity sources? combine all gravity vectors into resulting gravity
 **/
public class GlobalGravityControl : MonoBehaviour {

    // instance needed to start coroutine in static method
    public static GlobalGravityControl instance;

    // current gravity DOWN direction and strength
    private static Vector3 currentGravDirection;
    private static float currentGravStrength = 35f;
    private static float baseGravStrength;

    private static Vector3 gravTransitionFromDir;

    // gravity target direction and shift speed
    // used when shifting
    private static Vector3 targetGravDirection;
    private static float duration;

    // the player to update with the shift
    private GameObject player;
    private static PlayerMotor playerMotor;

    private static Coroutine shiftingCoroutine;

    private static RotateToGravity[] rotationObjects;

    public const string GravityChangeNotification = "GlobalGravityControl.GravityChangeNotification";

    void OnEnable()
    {
        rotationObjects = (RotateToGravity[])FindObjectsOfType(typeof(RotateToGravity));

        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null) Debug.LogError("No player object found");

        playerMotor = player.GetComponent<PlayerMotor>();

        currentGravDirection = -player.transform.up;
        targetGravDirection = currentGravDirection;

        baseGravStrength = currentGravStrength;

        instance = this;
    }

    public static float GetGravityStrength()
    {
        return currentGravStrength;
    }

    public static Vector3 GetCurrentGravityVector()
    {
        return currentGravDirection;
    }

    public static Vector3 GetGravityTarget()
    {
        return targetGravDirection;
    }

    public static Quaternion GetGravityRotation()
    {
        return Quaternion.FromToRotation(Vector3.down, currentGravDirection);
    }

    public static Quaternion GetRotationToDefaultAxis()
    {
        return Quaternion.FromToRotation(currentGravDirection, Vector3.down);
    }

    public static Quaternion GetRotationToGravity(Vector3 _up)
    {
        return Quaternion.FromToRotation(-_up, currentGravDirection);
    }

    public static Quaternion GetRotationToDir(Vector3 _direction)
    {
        return Quaternion.LookRotation(_direction, -currentGravDirection);
    }

    // Begins a smooth transition from the current gravity direction back to the default(down) direction
    public static void TransitionToDefault(float _duration = 1f)
    {
        // TODO: test logic here to map duration to size of gravity change
        TransitionGravity(Vector3.down, _duration);
    }

    public static void TransitionGravity(Vector3 _newGravDirection, float _duration)
    {
        duration = _duration;
        targetGravDirection = _newGravDirection.normalized;
        gravTransitionFromDir = currentGravDirection;

        // trigger scene objects to rotate
        foreach (RotateToGravity r in rotationObjects)
        {
            if (r.IsFollowGravity())
                r.StartRotation(targetGravDirection, duration);
        }

        if (shiftingCoroutine != null)
        {
            instance.StopCoroutine(shiftingCoroutine);
        }
        shiftingCoroutine = instance.StartCoroutine("TimedGravShift");
    }

    // should allow targetUpDirection to be updated mid-execution and it will continue to new target
    IEnumerator TimedGravShift()
    {
        float t = 0.0f;
        while (t < 1f)
        {
            t += Time.deltaTime * (Time.timeScale / duration);

            PerformGravTransition(t);

            this.PostNotification(GravityChangeNotification);

            yield return 0;
        }
        shiftingCoroutine = null;
    }

    // Apply gravity transition values relative to t
    private void PerformGravTransition(float t)
    {
        // transition direction
        float dirT = t * t;
        currentGravDirection = Vector3.Slerp(gravTransitionFromDir, targetGravDirection, dirT);

        // transition strength
        float strT = Mathf.Abs((t * 2) - 1);
        currentGravStrength = Mathf.Lerp(0f, baseGravStrength, strT);
    }

    /*
     * ChangeGravity methods
     */
    public static void ChangeGravity(Vector3 _newGravDir, float _newStrength, bool _dontAllowSmoothing)
    {
        currentGravStrength = _newStrength; // possibly need some smoothing here
        baseGravStrength = currentGravStrength;

        ChangeGravity(_newGravDir, _dontAllowSmoothing);
    }

    public static void ChangeGravity(Vector3 _newGravDir, bool _dontAllowSmoothing)
    {
        if (shiftingCoroutine != null)
            return;
        
        // interpolate for large differences in rotation
        //Vector3 transitionUp = Vector3.RotateTowards(currentGravDirection, _newGravDir, gravShiftSpeed * Mathf.Deg2Rad, 0f);

        //targetUpDirection = transitionUp;
        //currentGravDirection = targetUpDirection;
        currentGravDirection = _newGravDir;

        if (_dontAllowSmoothing)
        {
            // set player gravity direction instantly
            playerMotor.UpdateGravityDirection(currentGravDirection);
        }

        // Set scene object rotation
        foreach (RotateToGravity r in rotationObjects)
        {
            if (r.IsFollowGravity())
                r.SetRotation(targetGravDirection);
        }

        instance.StartCoroutine("SendGravityChangeNotification");
    }

    private IEnumerator SendGravityChangeNotification()
    {
        this.PostNotification(GravityChangeNotification);
        yield return null;
    }

    //private static float CalculateDuration(Vector3 _currUp, Vector3 _newGravDir, float _speedMod)
    //{
    //    float angle = Vector3.Angle(_currUp, _newGravDir);
    //    float dur = angle / _speedMod;

    //    return dur;
    //}

    
}
