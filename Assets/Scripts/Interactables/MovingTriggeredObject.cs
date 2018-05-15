using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTriggeredObject : TriggeredObject
{

    [SerializeField] private const float transitionDuration = 1f;

    [SerializeField] private bool transitionPosition = true;

    [SerializeField]
    private Vector3 endPosition;
    private Vector3 startPosition;

    [SerializeField] private bool transitionRotation = true;

    [SerializeField]
    private Quaternion endRotation;
    private Quaternion startRotation;

    private Rigidbody rb;

    private bool active;

    private Coroutine moveObjectCoroutine;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    public override void Trigger()
    {
        active = !active;
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
     * Triggering while running will change active boolean and reverse direction of transition
     */
    private IEnumerator MoveObjectTransition()
    {
        float t = active ? 0f : 1f;

        while (active ? t < 1f : t > 0f)
        {
            t += (active ? Time.deltaTime : -Time.deltaTime) / transitionDuration;

            if (transitionPosition)
            {
                Vector3 newPos = Vector3.Lerp(startPosition, endPosition, t);
                Vector3 posDiff = newPos - transform.position;
                rb.MovePosition(newPos);
                Info<Transform, Vector3> info = new Info<Transform, Vector3>(transform, posDiff);
                // post relative movement notification
                this.PostNotification(RelativeMovementController.RelativeMovementNotification, info);
            }
            if (transitionRotation)
            {
                Quaternion newRot = Quaternion.Lerp(startRotation, endRotation, t);
                Quaternion rotDiff = Quaternion.Inverse(transform.rotation) * newRot;
                rb.MoveRotation(newRot);
                Info<Transform, Quaternion> info = new Info<Transform, Quaternion>(transform, rotDiff);
                // post relative rotation notification
                this.PostNotification(RelativeMovementController.RelativeRotationNotification, info);
            }

            yield return 0;
        }

        moveObjectCoroutine = null;
    }
}
