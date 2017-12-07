using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativeMovementController : MonoBehaviour
{
    public const string RelativeMovementNotification = "RelativeMovementController.RelativeMovementNotification";
    public const string RelativeRotationNotification = "RelativeMovementController.RelativeRotationNotification";

    private GameObject relativeMotionObject;
    private Vector3 lastMovementVector = Vector3.zero;

    private Rigidbody rb;

    private bool landing = false;

    [SerializeField] private LayerMask relativeMotionLayers;

    /*
     * TODO: this needs to be refactored to NOT use JumpCollider notifications. 
     * Object should be selected by this script, and by direct collisions.
     * This should be able to be used for anything to which relative movement might apply.
     */
    void OnEnable()
    {
        rb = GetComponent<Rigidbody>();

        //this.AddObserver(OnMovementObjectNotification, JumpCollider.MovementObjectNotification);
        this.AddObserver(OnRelativeMovementNotification, RelativeMovementNotification);
        this.AddObserver(OnRelativeRotationNotification, RelativeRotationNotification);
    }

    void OnDisable()
    {
        //this.RemoveObserver(OnMovementObjectNotification, JumpCollider.MovementObjectNotification);
        this.RemoveObserver(OnRelativeMovementNotification, RelativeMovementNotification);
        this.RemoveObserver(OnRelativeRotationNotification, RelativeRotationNotification);
    }

    /*
     * Handler for RelativeMotionNotification sent from JumpCollider
     *  - Allows player to move with moving platforms etc without parenting
     */
    //void OnMovementObjectNotification(object sender, object args)
    //{
    //    GameObject newObject = (GameObject)args;

    //    if (newObject == null && relativeMotionObject != null)
    //    {
    //        rb.AddForce(lastMovementVector, ForceMode.VelocityChange);
    //        lastMovementVector = Vector3.zero;
    //    }

    //    relativeMotionObject = newObject;
    //}

    /*
     * Handles notifications of moving objects and moves the object to match.
     */
    void OnRelativeMovementNotification(object sender, object args)
    {
        if (relativeMotionObject == null)
            return;

        Info<GameObject, Vector3> info = (Info<GameObject, Vector3>) args;

        if (info.arg0 != relativeMotionObject)
            return;

        // apply movement vector
        rb.MovePosition(rb.position + info.arg1);

        // store movement vector for use during jumps/leaving contact with moving object
        lastMovementVector = info.arg1;

        if (landing)
        {
            Vector3 movingVelocity = lastMovementVector / Time.deltaTime;

            // modify velocity slightly towards matching platform movement
            float direction = Vector3.Dot(rb.velocity.normalized, movingVelocity.normalized);
            if (direction > 0)
            {
                // dampen velocity in direction of platform movement
                Vector3 component = Vector3.Project(rb.velocity, movingVelocity);
                float brakeMagnitude = Mathf.Min(component.magnitude / movingVelocity.magnitude, 1f);
                rb.velocity -= (movingVelocity * brakeMagnitude);
            }
            else
            {
                // increase velocity in direction of platform movement
                Vector3 component = Vector3.Project(rb.velocity, -movingVelocity);
                float boostMagnitude = Mathf.Min(component.magnitude / movingVelocity.magnitude, 1f);
                rb.velocity += (movingVelocity * boostMagnitude);
            }

            landing = false;
        }
    }

    /*
     * Handles notifications of rotating objects and moves the object to match.
     */
    void OnRelativeRotationNotification(object sender, object args)
    {
        if (relativeMotionObject == null)
            return;

        Info<GameObject, Quaternion> info = (Info<GameObject, Quaternion>)args;

        if (info.arg0 != relativeMotionObject)
            return;

        // Relative rotation logic here
    }

    void OnCollisionStay(Collision col)
    {
        if (col.collider.isTrigger)
            return;

        GameObject colObject = col.gameObject;

        if (colObject != relativeMotionObject && relativeMotionLayers == (relativeMotionLayers | (1 << col.gameObject.layer)))
        {
            relativeMotionObject = colObject;
            landing = true;
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.collider.isTrigger)
            return;

        GameObject colObject = col.gameObject;

        if (colObject == relativeMotionObject)
        {
            relativeMotionObject = null;
            //rb.AddForce(lastMovementVector, ForceMode.VelocityChange);
            rb.velocity += (lastMovementVector / Time.deltaTime);
            lastMovementVector = Vector3.zero;
        }
    }
}
