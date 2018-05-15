using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingTriggeredObject : TriggeredObject
{

    [SerializeField] private const float transitionDuration = 1f;

    // determines if position should be moved during transition
    [SerializeField] private bool transitionPosition = true;

    [SerializeField]
    private Vector3 endPosition;
    private Vector3 startPosition;

    // determines if rotation should be changed during transition
    [SerializeField] private bool transitionRotation = true;

    [SerializeField]
    private Quaternion endRotation;
    private Quaternion startRotation;

    [SerializeField] private bool useRelativeMotion = false;

    private Rigidbody rb;
    private bool transitionToEnd;

    private Coroutine moveObjectCoroutine;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    public override void Trigger()
    {
        Trigger(!transitionToEnd);
    }

    public override void Trigger(bool active)
    {
        transitionToEnd = active;
        StartMoveObject();
    }

    private void StartMoveObject()
    {
        if (moveObjectCoroutine != null)
        {
            moveObjectCoroutine = StartCoroutine("MoveObjectTransition");
        }
    }

    /*
     * Transitions between start and end positions/rotations
     * 
     * Triggering while running will change transitionToEnd boolean and reverse direction of transition
     */
    private IEnumerator MoveObjectTransition()
    {
        // loop direction determined by state of transitionToEnd variable.
        // if true, t = 0->1, false t = 1->0 
        // Should allow simple reversing if state changes mid-transition
        float t = transitionToEnd ? 0f : 1f;
        while (transitionToEnd ? t < 1f : t > 0f)
        {
            t += (transitionToEnd ? Time.deltaTime : -Time.deltaTime) / transitionDuration;

            Transition(t);

            yield return 0;
        }

        moveObjectCoroutine = null;
    }

    /*
     * Perform transition on position/rotation 
     */
    private void Transition(float t)
    {
        if (!useRelativeMotion)
        {
            // position transition logic
            if (transitionPosition)
            {
                rb.MovePosition(Vector3.Lerp(startPosition, endPosition, t));
            }

            // rotation transition logic
            if (transitionRotation)
            {
                rb.MoveRotation(Quaternion.Lerp(startRotation, endRotation, t));
            }
        }
        else
        {
            // position transition logic
            if (transitionPosition)
            {
                Vector3 newPos = Vector3.Lerp(startPosition, endPosition, t);
                Vector3 posDiff = newPos - transform.position;
                rb.MovePosition(newPos);
                Info<Transform, Vector3> info = new Info<Transform, Vector3>(transform, posDiff);
                // post relative movement notification
                this.PostNotification(RelativeMovementController.RelativeMovementNotification, info);
            }

            // rotation transition logic
            if (transitionRotation)
            {
                Quaternion newRot = Quaternion.Lerp(startRotation, endRotation, t);
                Quaternion rotDiff = Quaternion.Inverse(transform.rotation) * newRot;
                rb.MoveRotation(newRot);
                Info<Transform, Quaternion> info = new Info<Transform, Quaternion>(transform, rotDiff);
                // post relative rotation notification
                this.PostNotification(RelativeMovementController.RelativeRotationNotification, info);
            }
        }
    }
}
