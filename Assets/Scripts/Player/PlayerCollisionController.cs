using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionController : MonoBehaviour
{

    public static float slideThreshold = 50f;

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
    }

    void OnCollisionExit()
    {
        frictionless = true;
        colliding = false;
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

        //Debug.Log(_frictionless);
    }
}
