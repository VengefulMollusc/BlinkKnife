using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativeMovementController : MonoBehaviour
{
    public const string RelativeMovementNotification = "RelativeMovementController.RelativeMovementNotification";
    public const string RelativeRotationNotification = "RelativeMovementController.RelativeRotationNotification";

    private Transform relativeMotionTransform;
    private Vector3 thisMovementVector = Vector3.zero;
    private Vector3 lastMovementVector = Vector3.zero;
    private ContactPoint contactPoint;

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

    void FixedUpdate()
    {
        if (relativeMotionTransform == null)
            return;

        // store movement vector for use during jumps/leaving contact with moving object
        lastMovementVector = thisMovementVector;
        thisMovementVector = Vector3.zero;

        if (landing)
        {
            Vector3 movingVelocity = lastMovementVector / Time.deltaTime;

            // modify velocity slightly towards matching platform movement
            float direction = Vector3.Dot(rb.velocity.normalized, movingVelocity.normalized);
            if (direction > 0)
            {
                // dampen velocity in direction of platform movement
                Vector3 component = Vector3.Project(rb.velocity, movingVelocity);
                //float brakeMagnitude = Mathf.Min(component.magnitude / movingVelocity.magnitude, 1f);
                //rb.velocity -= (movingVelocity * brakeMagnitude);

                // alt logic
                if (component.magnitude < movingVelocity.magnitude)
                {
                    rb.velocity -= component;
                }
                else
                {
                    rb.velocity -= movingVelocity;
                }
            }
            //else
            //{
            //    // increase velocity in direction of platform movement
            //    Vector3 component = Vector3.Project(rb.velocity, -movingVelocity);
            //    float boostMagnitude = Mathf.Min(component.magnitude / movingVelocity.magnitude, 1f);
            //    rb.velocity += (movingVelocity * boostMagnitude);
            //}

            landing = false;
        }
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
        if (relativeMotionTransform == null)
            return;

        Info<Transform, Vector3> info = (Info<Transform, Vector3>) args;

        if (!relativeMotionTransform.IsChildOf(info.arg0))
            return;

        // apply movement vector
        rb.MovePosition(rb.position + info.arg1);
        thisMovementVector += info.arg1;

        // store movement vector for use during jumps/leaving contact with moving object
        //lastMovementVector = info.arg1;
    }

    /*
     * Handles notifications of rotating objects and moves the object to match.
     */
    void OnRelativeRotationNotification(object sender, object args)
    {
        if (relativeMotionTransform == null)
            return;

        Info<Transform, Quaternion> info = (Info<Transform, Quaternion>)args;

        if (!relativeMotionTransform.IsChildOf(info.arg0))
            return;

        Vector3 rotationMovement = GetRotationMovement(info.arg0, info.arg1);

        rb.MovePosition(rb.position + rotationMovement);
        thisMovementVector += rotationMovement;
    }

    /*
     * Gets the relative vector movement of the contact point using the given rotation
     */
    Vector3 GetRotationMovement(Transform _transform, Quaternion _rotation)
    {
        Vector3 centerToContact = contactPoint.point - _transform.position;
        Vector3 newContactPoint = _rotation * centerToContact;

        return newContactPoint - centerToContact;
    }

    void OnCollisionStay(Collision col)
    {
        if (col.collider.isTrigger)
            return;

        Transform colTransform = col.transform;

        if (relativeMotionLayers == (relativeMotionLayers | (1 << col.gameObject.layer)))
        {
            if (colTransform != relativeMotionTransform)
            {
                relativeMotionTransform = colTransform;
                landing = true;
            }

            //if relativemotionobjects exists, check that a suitable contact point can be found
            if (relativeMotionTransform != null)
            {
                bool suitablePoint = false;
                Vector3 gravVector = GlobalGravityControl.GetCurrentGravityVector();

                foreach (ContactPoint point in col.contacts)
                {
                    Vector3 relative = point.point - transform.position;
                    if (Vector3.Angle(gravVector, relative) < 20f)
                    {
                        contactPoint = point;
                        suitablePoint = true;
                        break;
                    }
                }

                if (!suitablePoint)
                {
                    ExitRelativeMotion();
                    //relativeMotionTransform = null;
                }
            }
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (relativeMotionTransform == null || col.collider.isTrigger)
            return;

        Transform colTransform = col.transform;

        if (colTransform == relativeMotionTransform)
        {
            ExitRelativeMotion();
        }
    }

    void ExitRelativeMotion()
    {
        relativeMotionTransform = null;
        //Debug.Log(rb.velocity + " " + lastMovementVector / Time.deltaTime);
        rb.velocity += (lastMovementVector / Time.deltaTime);
        lastMovementVector = Vector3.zero;
    }
}
