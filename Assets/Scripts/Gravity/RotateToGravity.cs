using UnityEngine;
using System.Collections;

public class RotateToGravity : MonoBehaviour
{

    [SerializeField]
    private bool followGravityDirection = true;
    [SerializeField]
    private bool followPlayerPosition = false;

    [Header("Ambient Rotation")]
    [SerializeField]
    private bool ambientRot = false;
    [SerializeField]
    private Vector3 ambientRotAxis;
    [SerializeField]
    private float ambientRotSpeed = 1f;

    [Header("Rotation Modifiers")]
    [SerializeField]
    private bool modifyRotation = false;
    [SerializeField]
    private float xMod = 1f;
    [SerializeField]
    private float yMod = 1f;
    [SerializeField]
    private float zMod = 1f;

    private Quaternion startRot;
    private Quaternion endRot;
    private float duration;

    private GameObject player;

    private Coroutine transitionCoroutine;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null) Debug.Log("No player object found");

        /*
         * Need to update for any changes to player rotation
         * 
         * could get player object and transition if it changes?
         * This would need to be as well as the warp transitions
         * as the player 'doesn't exist' when thats happening
         */

    }

    public void StartRotation(Vector3 _newGravDir, float _duration)
        //public void StartRotation(Quaternion _endRot, float _duration)
    {
        // cancel if not following gravity changes
        if (!followGravityDirection) return;

        startRot = transform.rotation;
        endRot = CalculateEndRotation(_newGravDir);
        duration = _duration; // could multiply here to allow for lengthened shift effect

        if (modifyRotation)
            ApplyModifiers();

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(RotateTransition());
    }

    public void SetRotation(Vector3 _newGravDir)
    {
        // cancel if not following gravity changes
        if (!followGravityDirection) return;

        endRot = CalculateEndRotation(_newGravDir);

        if (modifyRotation)
            ApplyModifiers();

        transform.rotation = endRot;
    }

    void FixedUpdate()
    {
        // perform ambient rotation
        if (ambientRot)
        {
            Quaternion initialRotation = transform.rotation;
            transform.Rotate(ambientRotAxis, ambientRotSpeed * Time.fixedDeltaTime);
            Quaternion relative = Quaternion.Inverse(initialRotation) * transform.rotation;

            Info<Transform, Quaternion> info = new Info<Transform, Quaternion>(transform, relative);
            this.PostNotification(RelativeMovementController.RelativeRotationNotification, info);
        }

        // TODO: Not sure why this is here....
        // Might be for skybox...
        if (followPlayerPosition)
            transform.position = Camera.main.transform.position;
    }

    // Modifies rotations in each axes by multiplying by field value
    // Can be reversed by using negative values
    private void ApplyModifiers()
    {
        Vector3 eulerAngles = endRot.eulerAngles;
        if (xMod != 1f)
        {
            eulerAngles.x = eulerAngles.x * xMod;
        }
        if (yMod != 1f)
        {
            eulerAngles.y = eulerAngles.y * yMod;
        }
        if (zMod != 1f)
        {
            eulerAngles.z = eulerAngles.z * zMod;
        }
        endRot = Quaternion.Euler(eulerAngles);
    }

    /*
     * Goes a bit mental, but seems to get to the right direction consistently, so thats ok
     * 
     *  - converting back to quaternions for smoother wrapping (360-0 etc)
     */
    IEnumerator RotateTransition()
    {
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * (Time.timeScale / duration); // may need to be fixedDeltaTime
            transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return 0;
        }
        transitionCoroutine = null;
    }

    /*
     * Calculates a new end rotation that aims the target rotation
     * at either 'up' or 'forward' on the world axis depending
     * on the new 'up' direction of the player
     * 
     * Keeps local y rotation consistent regardless of which direction
     * the player is facing
     */
    //private Quaternion CalculateEndRotation(Vector3 _up)
    //{

    //    float angleToUp = Vector3.Angle(_up, Vector3.up);
    //    Vector3 projectedForward;
    //    if (angleToUp < 45f || angleToUp > 135f)
    //    {
    //        // if new up direction is within 45 degrees of world up (or down)
    //        // use world forward
    //        projectedForward = Vector3.ProjectOnPlane(Vector3.forward, _up);
    //    } else
    //    {
    //        // use world up
    //        projectedForward = Vector3.ProjectOnPlane(Vector3.up, _up);
    //    }

    //    // end rotation based on player up vector and new forward direction
    //    Quaternion newRot = Quaternion.LookRotation(projectedForward, _up);

    //    return newRot;
    //}

    private Quaternion CalculateEndRotation(Vector3 _newGravDir)
    {
        Vector3 _up = -_newGravDir;
        float angleToUp = Vector3.Angle(_up, Vector3.up);
        //float angleToForward = Vector3.Angle(_up, Vector3.forward);
        float angleToRight = Vector3.Angle(_up, Vector3.right);

        /*
         * This is much better.
         * Smoother again, still the odd flick but overall success
         * Caused by Gimbal lock?
         */

        float tUp = Mathf.Abs(angleToUp - 90f);
        float tRight = Mathf.Abs(angleToRight - 90f);

        // Lerp between world up and forward vectors
        // should produce a much smoother transition
        // Testing lerping to right vector as well
        tUp = Utilities.MapValues(tUp, 0f, 90f, 0f, 1f);
        tRight = Utilities.MapValues(tRight, 0f, 90f, 0f, 1f);

        Vector3 newForward = Vector3.Lerp(Vector3.up, Vector3.forward, tUp);
        newForward = Vector3.Lerp(Vector3.right, newForward, tRight);

        Vector3 projectedForward = Vector3.ProjectOnPlane(newForward, _up); // may not be nessecary
        // end rotation based on player up vector and new forward direction
        Quaternion newRot = Quaternion.LookRotation(projectedForward, _up);

        return newRot;
    }

    public bool IsFollowGravity()
    {
        return followGravityDirection;
    }
}
