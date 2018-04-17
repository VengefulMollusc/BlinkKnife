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
            
            if (angle < slideThreshold)
            {
                frictionless = false;
                return;
            }
        }

        frictionless = true;

        // check for ledges
        jumpHeightToLedge = CheckVaultForwardSweep();
    }

    /*
     * Checks for a ledge in front of the player that could be vaulted
     */
    float CheckVaultForwardSweep()
    {
        // initial check for ledge surface
        Vector3 raycastOrigin = transform.position + (1.5f * transform.up) + transform.forward;
        RaycastHit hitInfo;

        if (Physics.Raycast(raycastOrigin, -transform.up, out hitInfo, 2.5f, raycastMask, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.Angle(hitInfo.normal, transform.up) > 5f)
                return 0f;

            // sweep collider to check if player can fit on ledge
            Vector3 offset = Vector3.Project((transform.position - transform.up) - hitInfo.point, transform.up) + (transform.up * 0.05f);
            ledgeCheckObject.transform.position = transform.position + offset;

            RaycastHit sweepHitInfo;

            if (ledgeCheckObject.GetComponent<Rigidbody>().SweepTest(transform.forward, out sweepHitInfo, 1f))
            {
                if (sweepHitInfo.distance < 0.5f)
                {
                    // No room
                    return 0f;
                }
            }

            //return offset.magnitude;
            return 2.5f - hitInfo.distance;
        }

        return 0f;
    }

    public static float GetVaultHeight()
    {
        return jumpHeightToLedge;
    }

    void OnCollisionExit()
    {
        frictionless = true;
        colliding = false;
        jumpHeightToLedge = 0f;
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
