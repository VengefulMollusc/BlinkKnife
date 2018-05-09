using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class BoomerangKnifeController : KnifeController
{
    [SerializeField] private float boomerangDuration = 2f;

    private Vector3 startPos;
    private Vector3 tangentPoint;
    private float transition;
    private float initialVelMagnitude;

    public override void Throw(Vector3 _velocity)
    {
        //base.Throw(_velocity);
        initialVelMagnitude = _velocity.magnitude;
        this.PostNotification(AttachLookAheadColliderNotification, this);

        startPos = transform.position;
        tangentPoint = startPos + (_velocity * throwStrengthMod * (boomerangDuration * 0.5f));
    }

    void FixedUpdate()
    {
        if (returning)
            return;

        //if (rb.velocity != Vector3.zero)
        //{
        //    // something has affected velocity - boostRing etc.
        //    Vector3 tempVel = rb.velocity;
        //    rb.velocity = Vector3.zero;
        //    ResetBezier(tempVel);
        //}

        warpTimer += Time.fixedDeltaTime;

        if (HasStuck())
            return;

        transition = warpTimer / boomerangDuration;
        if (transition <= 1f)
        {
            //rb.velocity = Utilities.BezierDerivative(startPos, tangentPoint, tangentPoint, ownerTransform.position, t);
            rb.MovePosition(Utilities.LerpBezier(startPos, tangentPoint, tangentPoint, ownerTransform.position, transition));
        }
        else
        {
            this.PostNotification(ReturnKnifeNotification);
        }
    }

    void OnCollisionEnter(Collision _col)
    {
        if (HasStuck())
            return;

        ContactPoint collide = _col.contacts[0];
        GameObject other = collide.otherCollider.gameObject;

        // If collided surface is not a HardSurface, stick knife into it
        if (other.GetComponent<HardSurface>() == null)
            StickToSurface(collide.point, collide.normal, other);
    }

    public Vector3 GetEffectiveVelocity()
    {
        return Utilities.BezierDerivative(startPos, tangentPoint, tangentPoint, ownerTransform.position, transition);
    }

    public override void OnBoostNotification(object sender, object args)
    {
        Info<GameObject, Vector3> info = (Info<GameObject, Vector3>)args;
        if (info.arg0 != gameObject)
            return;

        // boosted object is this knife
        ResetBezier(info.arg1.normalized * initialVelMagnitude);
    }

    private void ResetBezier(Vector3 _newVelocity)
    {
        startPos = transform.position;
        tangentPoint = startPos + (_newVelocity * throwStrengthMod * (boomerangDuration * 0.5f));
        warpTimer = 0f;
    }
}
