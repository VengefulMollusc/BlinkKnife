using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20f;

    [SerializeField] private Vector3 startPos = Vector3.zero;
    [SerializeField] private Vector3 endPos = Vector3.zero;

    private Vector3 targetPos = Vector3.zero;

    private Rigidbody rb;

    void OnEnable()
    {
        if (startPos == endPos)
            Debug.LogError("Start and end positions are the same");

        transform.position = startPos;
        targetPos = endPos;

        rb = GetComponent<Rigidbody>();
    }
    
	void FixedUpdate () {
        Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, Time.fixedDeltaTime * moveSpeed);
        Info<Transform, Vector3> info = new Info<Transform, Vector3>(transform, newPos - rb.position);
        rb.position = newPos;

        // Send notification with movementVector here
        this.PostNotification(RelativeMovementController.RelativeMovementNotification, info);

        if (transform.position == targetPos)
	    {
	        if (targetPos == startPos)
	            targetPos = endPos;
	        else
	            targetPos = startPos;
	    }
	}
}
