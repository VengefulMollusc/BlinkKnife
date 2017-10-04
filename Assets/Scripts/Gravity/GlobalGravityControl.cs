using UnityEngine;
using System.Collections;


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
    private static float currentGravityStrength = 35f;

    private static Vector3 tempGravDir;

    // gravity target direction and shift speed
    // used when shifting
    private static Vector3 targetGravDirection;
    private static float gravShiftSpeed = 2;
    private static float duration;

    // the player to update with the shift
    private GameObject player;
    private static PlayerMotor playerMotor;

    private static Coroutine shiftingCoroutine;

    private static RelativeRotationController[] rotationObjects;

    void Awake()
    {
        rotationObjects = (RelativeRotationController[])FindObjectsOfType(typeof(RelativeRotationController));
    }

    // Use this for initialization
    void Start () {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null) Debug.LogError("No player object found");

        playerMotor = player.GetComponent<PlayerMotor>();

        currentGravDirection = -player.transform.up;
        targetGravDirection = currentGravDirection;

        instance = this;    
	}

    public static float GetGravityStrength()
    {
        return currentGravityStrength;
    }

    public static Vector3 GetCurrentGravityVector()
    {
        return currentGravDirection;
    }

    public static Vector3 GetGravityTarget()
    {
        return targetGravDirection;
    }

    public static void TransitionGravity(Vector3 _newGravDirection, float _duration)
    {
        duration = _duration;
        targetGravDirection = _newGravDirection.normalized;
        tempGravDir = currentGravDirection;

        // trigger scene objects to rotate
        foreach (RelativeRotationController r in rotationObjects)
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
            currentGravDirection = Vector3.Slerp(tempGravDir, targetGravDirection, t);

            // update gravity-dependant objects here
            //playerMotor.UpdateGravityDirection(currentGravDirection);

            yield return 0;
        }
        shiftingCoroutine = null;
    }


    /*
     * ChangeGravity methods
     */
    public static void ChangeGravity(Vector3 _newGravDir, float _newStrength)
    {
        currentGravityStrength = _newStrength; // possibly need some smoothing here

        ChangeGravity(_newGravDir);
    }

    public static void ChangeGravity(Vector3 _newGravDir)
    {
        if (shiftingCoroutine != null)
            return;
        
        // interpolate for large differences in rotation
        //Vector3 transitionUp = Vector3.RotateTowards(currentGravDirection, _newGravDir, gravShiftSpeed * Mathf.Deg2Rad, 0f);

        //targetUpDirection = transitionUp;
        //currentGravDirection = targetUpDirection;
        currentGravDirection = _newGravDir;

        //if (_modifyPlayer)
        //{
        //    // set player gravity direction
        //    playerMotor.UpdateGravityDirection(currentGravDirection);
        //}

        // Set scene object rotation
        foreach (RelativeRotationController r in rotationObjects)
        {
            if (r.IsFollowGravity())
                r.SetRotation(targetGravDirection);
        }
    }

    //private static float CalculateDuration(Vector3 _currUp, Vector3 _newGravDir, float _speedMod)
    //{
    //    float angle = Vector3.Angle(_currUp, _newGravDir);
    //    float dur = angle / _speedMod;

    //    return dur;
    //}

    
}
