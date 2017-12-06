using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativeMovementController : MonoBehaviour
{
    public const string RelativeMovementNotification = "RelativeMovementController.RelativeMovementNotification";

    private GameObject relativeMotionObject;

    /*
     * TODO: this needs to be refactored to NOT use JumpCollider notifications. 
     * Object should be selected by this script, and by direct collisions.
     * This should be able to be used for anything to which relative movement might apply.
     */
    void OnEnable()
    {
        this.AddObserver(OnMovementObjectNotification, JumpCollider.MovementObjectNotification);
    }

    void OnDisable()
    {
        this.AddObserver(OnMovementObjectNotification, JumpCollider.MovementObjectNotification);
    }

    /*
     * Handler for RelativeMotionNotification sent from JumpCollider
     *  - Allows player to move with moving platforms etc without parenting
     */
    void OnMovementObjectNotification(object sender, object args)
    {
        relativeMotionObject = (GameObject)args;
    }

    //void Update()
    //{
    //    //if (!frozen && UseGroundMovement() && jumpTimer <= 0)
    //    RelativeMovement();
    //}

    /*
     * Handles repositioning of the player to match movement of object the player is standing on
     */
    //void RelativeMovement()
    //{
    //    // TODO: Try using FixedJoint to lock movement

    //    if (relativeMotionTransform == null)
    //    {
    //        return;
    //    }

    //    Vector3 newRelativeMotionPosition = relativeMotionTransform.position;
    //    Quaternion newRelativeMotionRotation = relativeMotionTransform.rotation;

    //    if (newRelativeMotionPosition == relativeMotionPosition && newRelativeMotionRotation == relativeMotionRotation)
    //    {
    //        return;
    //    }

    //    // get initial foot position relative to moving object
    //    Vector3 relativeFootPos = GetFootPosition() - relativeMotionPosition;

    //    // if rotation of object has changed
    //    if (newRelativeMotionRotation != relativeMotionRotation)
    //    {
    //        // calculate Quaternion rotation difference between rotations
    //        Quaternion rotDiff = Quaternion.Inverse(relativeMotionRotation) * newRelativeMotionRotation;

    //        // apply rotation difference to relative foot position
    //        relativeFootPos = rotDiff * relativeFootPos;
    //    }

    //    // calculate new position
    //    Vector3 newPos = newRelativeMotionPosition + relativeFootPos - currentGravVector;

    //    //Vector3 newPos = transform.position + (newRelativeMotionPosition - relativeMotionPosition);

    //    // store relative movement between old/new positions for adding to jump vector off moving platforms
    //    relativeMovementVector = newPos - transform.position;

    //    // move player to new position
    //    // TODO: MAYBE add relative velocity?
    //    //rb.position = newPos;
    //    rb.MovePosition(newPos);
    //    //rb.velocity += relativeMovementVector;

    //    // update relative motion variables
    //    relativeMotionPosition = newRelativeMotionPosition;
    //    relativeMotionRotation = newRelativeMotionRotation;
    //}
}
