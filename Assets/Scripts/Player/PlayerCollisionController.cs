using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionController : MonoBehaviour
{

    public static float slideThreshold = 46f;

    private PlayerMotor playerMotor;
    SphereCollider sphereCol;
    private PhysicMaterial colMaterial;
    private float staticFriction;
    private float dynamicFriction;

    private bool frictionless;
    private bool colliding;

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
    }

    void FixedUpdate()
    {
        UpdateCollisionState(frictionless, colliding);

        frictionless = true;
        colliding = false;
    }

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
    }

    void UpdateCollisionState(bool _frictionless, bool _colliding)
    {
        if (_frictionless)
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
