using UnityEngine;
using System.Collections;

public class GlobalGravityControl : MonoBehaviour {

    // instance needed to start coroutine in static method
    public static GlobalGravityControl instance;

    // current gravity direction and strength
    // TODO: figure out whether this needs to be an up or down vector
    private static Vector3 currentUpDirection;
    private static float currentGravityStrength = 35f;

    private static Vector3 tempUp;

    // gravity target direction and shift speed
    // used when shifting
    private static Vector3 targetUpDirection;
    private static float gravShiftSpeed = 2;
    private static float duration;

    // the player to update with the shift
    [SerializeField]
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
        if (player == null) Debug.LogError("No player object found in GlobalGravityControl");

        playerMotor = player.GetComponent<PlayerMotor>();

        currentUpDirection = Vector3.up;
        targetUpDirection = currentUpDirection;

        instance = this;    
	}

    public static float GetGravityStrength()
    {
        return currentGravityStrength;
    }

    public static Vector3 GetCurrentGravityUpVector()
    {
        return currentUpDirection;
    }

    public static Vector3 GetCurrentGravityDownVector()
    {
        return -currentUpDirection;
    }

    public static Vector3 GetGravityTarget()
    {
        return targetUpDirection;
    }

    public static void TransitionGravity(Vector3 _newUp, float _duration)
    {
        duration = _duration;
        targetUpDirection = _newUp.normalized;
        tempUp = currentUpDirection;

        // trigger scene objects to rotate
        foreach (RelativeRotationController r in rotationObjects)
        {
            if (r.IsFollowGravity())
                r.StartRotation(targetUpDirection, duration);
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
            currentUpDirection = Vector3.Slerp(tempUp, targetUpDirection, t);

            // update gravity-dependant objects here
            //playerMotor.UpdateGravityDirection(currentUpDirection);

            yield return 0;
        }
        shiftingCoroutine = null;
    }

    public static void ChangeGravity(Vector3 _newUp, bool _modifyPlayer)
    {
        if (shiftingCoroutine != null)
            return;
        
        // interpolate for large differences in rotation
        Vector3 transitionUp = Vector3.RotateTowards(currentUpDirection, _newUp, gravShiftSpeed * Mathf.Deg2Rad, 0f);

        targetUpDirection = transitionUp;
        currentUpDirection = targetUpDirection;

        if (_modifyPlayer)
        {
            // set player gravity direction
            playerMotor.UpdateGravityDirection(currentUpDirection);
        }

        // Set scene object rotation
        foreach (RelativeRotationController r in rotationObjects)
        {
            if (r.IsFollowGravity())
                r.SetRotation(targetUpDirection);
        }
    }

    //private static float CalculateDuration(Vector3 _currUp, Vector3 _newUp, float _speedMod)
    //{
    //    float angle = Vector3.Angle(_currUp, _newUp);
    //    float dur = angle / _speedMod;

    //    return dur;
    //}

    
}
