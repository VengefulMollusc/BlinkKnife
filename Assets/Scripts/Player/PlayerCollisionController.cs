using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerCollisionController : MonoBehaviour
{

    public static float slideThreshold = 50f;

    [SerializeField] private LayerMask raycastMask;
    [SerializeField] private GameObject ledgeCheckObject;

    private static float jumpHeightToLedge;

    private PlayerMotor playerMotor;
    SphereCollider sphereCol;
    private PhysicMaterial colMaterial;
    private float staticFriction;
    private float dynamicFriction;

    private bool frictionless;
    private bool colliding;

    private float speedThreshold;

    // Use this for initialization
    void OnEnable ()
    {
        playerMotor = GetComponent<PlayerMotor>();
        sphereCol = GetComponent<SphereCollider>();
        colMaterial = sphereCol.material;
        staticFriction = colMaterial.staticFriction;
        dynamicFriction = colMaterial.dynamicFriction;

        Physics.IgnoreCollision(GetComponent<SphereCollider>(), GetComponentInChildren<CapsuleCollider>());

        frictionless = true;
        colliding = false;

        speedThreshold = PlayerController.Speed() * PlayerMotor.VelMod() * PlayerController.SprintModifier();
    }

    void FixedUpdate()
    {
        // also disable friction while moving above speed threshold
        bool frictionOverride = (GetComponent<Rigidbody>().velocity.magnitude > speedThreshold);
        UpdateCollisionState(frictionless, colliding, frictionOverride);
    }

    /*
     * Moving against a wall causes this to not detect the ground collision point.
     * Right now this isn't an issue as the PlayerMotor will override sliding on ground if needed
     */
    void OnCollisionStay(Collision col)
    {
        ContactPoint[] colContacts = col.contacts;

        foreach (ContactPoint point in colContacts)
        {
            if (!colliding && point.thisCollider == sphereCol)
                colliding = true;

            float angle = Vector3.Angle(point.normal, transform.up);

            if (Vector3.Dot(-transform.forward, point.normal) > 0.5f)
            {
                // check for ledges
                jumpHeightToLedge = CheckCanVault();
            }
            
            if (angle < slideThreshold)
            {
                frictionless = false;
                return;
            }
        }

        frictionless = true;
    }

    void OnCollisionExit()
    {
        frictionless = true;
        colliding = false;
        jumpHeightToLedge = 0f;
    }

    /*
     * Checks for a ledge in front of the player that could be vaulted
     */
    float CheckCanVault()
    {
        // initial check for ledge surface
        Vector3 ledgeDetectionCastOrigin = transform.position + (1.5f * transform.up);
        RaycastHit hitInfo;

        if (Physics.Raycast(ledgeDetectionCastOrigin + (0.75f * transform.forward), -transform.up, out hitInfo, 2.5f, raycastMask, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.Angle(hitInfo.normal, transform.up) > 15f)
                return 0f;

            // sweep collider to check if player can fit on ledge
            float ledgeHeightDiff = hitInfo.distance;

            // Check down with spherecast
            RaycastHit sphereHit;
            if (Physics.SphereCast(ledgeDetectionCastOrigin + (0.5f * transform.up) + (0.5f * transform.forward), 0.5f, -transform.up,
                out sphereHit, hitInfo.distance, raycastMask))
            {
                ledgeHeightDiff = sphereHit.distance;
            }

            // TODO: test whether capsulecast or sweeptest works better for detecting ceilings etc (apparently casting is more efficient?)
            // Sweep forward at height relative to raycast/spherecast hit
            Vector3 capsulePoint1 = ledgeDetectionCastOrigin - ((ledgeHeightDiff - 0.51f) * transform.up);
            Vector3 capsulePoint2 = capsulePoint1 + transform.up;
            if (Physics.CapsuleCast(capsulePoint1, capsulePoint2, 0.5f, transform.forward, 0.6f, raycastMask,
                QueryTriggerInteraction.Ignore))
            {
                return 0f;
            }


            //ledgeCheckObject.transform.position = ledgeDetectionCastOrigin - ((ledgeHeightDiff - 1.01f) * transform.up);
            //RaycastHit sweepHitInfo;
            //if (ledgeCheckObject.GetComponent<Rigidbody>().SweepTest(transform.forward, out sweepHitInfo, 0.6f))
            //{
            //    //Debug.Log("-- Sweep hit edge");
            //    // No room
            //    return 0f;
            //}


            // CapsuleCast down from position above player
            //Vector3 capsulePoint1 = ledgeDetectionRaycastOrigin;
            //Vector3 capsulePoint2 = capsulePoint1 + transform.up;

            //RaycastHit capsuleHitInfo;

            //if (Physics.CapsuleCast(capsulePoint1, capsulePoint2, 0.5f, -transform.up, out capsuleHitInfo,
            //    hitInfo.distance, raycastMask, QueryTriggerInteraction.Ignore))
            //{
            //    //Debug.Log((2.5f - hitInfo.distance) + " " + (2f - capsuleHitInfo.distance));
            //}


            //Debug.Log("-- Safe Sweep");
            //return offset.magnitude;
            return 2.5f - ledgeHeightDiff;
        }

        return 0f;
    }

    public static float GetVaultHeight()
    {
        return jumpHeightToLedge;
    }

    void UpdateCollisionState(bool _frictionless, bool _colliding, bool _frictionOverride)
    {
        if (_frictionless || _frictionOverride)
        {
            colMaterial.staticFriction = 0f;
            colMaterial.dynamicFriction = 0f;
            colMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
        }
        else
        {
            colMaterial.staticFriction = staticFriction;
            colMaterial.dynamicFriction = dynamicFriction;
            colMaterial.frictionCombine = PhysicMaterialCombine.Average;
        }

        playerMotor.SetCollisionState(_frictionless, _colliding);
    }
}
