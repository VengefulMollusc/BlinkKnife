using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionController : MonoBehaviour
{

    [SerializeField] private float slideThreshold = 45f;

    private PlayerMotor playerMotor;
    private PhysicMaterial colMaterial;
    private float staticFriction;
    private float dynamicFriction;

    private bool frictionless;

    // Use this for initialization
    void OnEnable ()
    {
        playerMotor = GetComponent<PlayerMotor>();
        colMaterial = GetComponent<SphereCollider>().material;
        staticFriction = colMaterial.staticFriction;
        dynamicFriction = colMaterial.dynamicFriction;

        Physics.IgnoreCollision(GetComponent<SphereCollider>(), GetComponent<CapsuleCollider>());

        frictionless = false;
    }

    void FixedUpdate()
    {
        FrictionlessState(frictionless);

        frictionless = true;
    }

    void OnCollisionStay(Collision col)
    {
        ContactPoint[] colContacts = col.contacts;

        foreach (ContactPoint point in colContacts)
        {
            float angle = Vector3.Angle(point.normal, transform.up);
            if (angle < slideThreshold)
            {
                frictionless = false;
                return;
            }
        }
    }

    void FrictionlessState(bool _frictionless)
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

        playerMotor.SetSliding(_frictionless);
    }
}
