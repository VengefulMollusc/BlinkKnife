using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class BoomerangKnifeController : KnifeController
{
    /*
     * 'Boomerang' knife option.
     * Useful for shorter-range (compared to BlinkKnife) accurate traversal,
     * or navigation to hard-to-reach areas through collisions on the return journey
     * 
     * Travels for a while before turning back and returning to the player
     * Note: this is NOT a recall as it can still collide with objects on the return journey
     * 
     * Sticks into surfaces and waits for player input to warp
     */

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

        warpTimer += Time.fixedDeltaTime;

        if (HasStuck())
            return;

        transition = warpTimer / boomerangDuration;
        Vector3 ownerPos = ownerTransform.position;
        Vector3 secondTangent = ownerPos +
                                (ownerTransform.forward * initialVelMagnitude * (boomerangDuration * 0.5f));
        if (transition <= 1f)
        {
            //rb.velocity = Utilities.BezierDerivative(startPos, tangentPoint, tangentPoint, ownerTransform.position, t);
            rb.MovePosition(Utilities.LerpBezier(startPos, tangentPoint, secondTangent, ownerPos, transition));
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
