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

    //private Vector3 lastMovementVector = Vector3.zero;

    //private List<Rigidbody> attachedObjects = new List<Rigidbody>();

    void OnEnable()
    {
        if (startPos == endPos)
            Debug.LogError("Start and end positions are the same");

        transform.position = startPos;
        targetPos = endPos;

        rb = GetComponent<Rigidbody>();
    }

	// Update is called once per frame
	void Update () {
        //Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeed);
	    //lastMovementVector = newPos - rb.position;
	    //rb.position = newPos;

        rb.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeed);

        if (transform.position == targetPos)
	    {
	        if (targetPos == startPos)
	            targetPos = endPos;
	        else
	            targetPos = startPos;
	    }

        // move attached objects
	    //foreach (Rigidbody obj in attachedObjects)
	    //{
	    //    obj.position += lastMovementVector;
	    //}
	}

    //void OnCollisionStay(Collision col)
    //{
    //    bool isOnTop = false;

    //    // check if has a RigidBody
    //    Rigidbody colRb = col.gameObject.GetComponent<Rigidbody>();
    //    if (colRb == null)
    //        return;

    //    foreach (ContactPoint item in col.contacts)
    //    {
    //        isOnTop = item.point.y > transform.position.y;
    //        if (isOnTop)
    //            break;
    //    }
    //    if (isOnTop)
    //    {
    //        bool unique = true;
    //        foreach (Rigidbody attached in attachedObjects)
    //        {
    //            unique = attached != colRb;

    //            if (!unique)
    //                break;
    //        }
    //        if (unique)
    //        {
    //            attachedObjects.Add(colRb);
    //        }
    //    }
    //}

    //void OnCollisionExit(Collision col)
    //{
    //    Rigidbody colRb = col.gameObject.GetComponent<Rigidbody>();
    //    if (colRb == null)
    //        return;

    //    if (attachedObjects.Contains(colRb))
    //    {
    //        // add velocity/force in direction of last movement

    //        attachedObjects.Remove(colRb);
    //    }
    //}
}
