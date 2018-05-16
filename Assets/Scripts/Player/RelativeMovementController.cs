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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    void OnEnable()
    {
        this.AddObserver(OnRelativeMovementNotification, RelativeMovementNotification);
        this.AddObserver(OnRelativeRotationNotification, RelativeRotationNotification);
    }

    void OnDisable()
    {
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

        ApplyRelativeRotation(info.arg0, info.arg1);
    }

    void ApplyRelativeRotation(Transform _transform, Quaternion _rotation)
    {
        // Gravity rotation and rotation from relative motion object axis to global
        Quaternion gravRotation = GlobalGravityControl.GetGravityRotation();
        Quaternion rotateToGlobalAxis = Quaternion.FromToRotation(_transform.up, Vector3.up);

        // rotate relative position vector of contact point to global axis
        Vector3 centerToContact = rotateToGlobalAxis * (contactPoint.point - _transform.position);

        // apply rotation to relative position vector
        Vector3 newContactPoint = _rotation * centerToContact;

        // rotate both old and new position vectors back to relative motion object axis
        centerToContact = Quaternion.Inverse(rotateToGlobalAxis) * centerToContact;
        newContactPoint = Quaternion.Inverse(rotateToGlobalAxis) * newContactPoint;

        // calculate movement vector due to rotation
        Vector3 rotationMovement = newContactPoint - centerToContact;

        // move the player to the new position
        rb.MovePosition(rb.position + rotationMovement);
        thisMovementVector += rotationMovement;

        // get axis of rotation for right angle check
        float tempAngle;
        Vector3 rotationAxis;
        _rotation.ToAngleAxis(out tempAngle, out rotationAxis);

        // Check if rotation axis is right angle
        if (Mathf.Abs(Vector3.Dot(rotateToGlobalAxis * rotationAxis, transform.up)) > 0.01f)
        {
            // project position vectors onto plane defined by player up direction and rotate to match gravity
            centerToContact = Quaternion.Inverse(gravRotation) * Vector3.ProjectOnPlane(centerToContact, transform.up);
            newContactPoint = Quaternion.Inverse(gravRotation) * Vector3.ProjectOnPlane(newContactPoint, transform.up);

            // get angle of view rotation from projected vectors
            Quaternion lookRotation = Quaternion.FromToRotation(centerToContact, newContactPoint);

            // apply rotation
            rb.rotation *= lookRotation;
        }
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
                    float angle = Vector3.Angle(-gravVector, point.normal);
                    if (angle < PlayerCollisionController.slideThreshold) // TODO: work on this to solve bug when being pushed by moving object
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
        rb.velocity += (lastMovementVector / Time.deltaTime);
        lastMovementVector = Vector3.zero;
    }
}
