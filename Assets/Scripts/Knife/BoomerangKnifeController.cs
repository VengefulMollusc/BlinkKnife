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
     * Can be 'aimed' mid-flight by turning player camera
     * 
     * Sticks into surfaces and waits for player input to warp
     */

    // Defines the duration of the entire loop./
    // Altering this and ThrowStrengthMod will change the distance and speed of the throw
    [SerializeField] private float boomerangDuration = 2f;

    private Vector3 startPos;
    private float tangentMagnitude;
    private Vector3 tangentOnePos;
    private Vector3 tangentTwo;
    private float transition;
    private float initialVelMagnitude;

    // True if the knife has been boosted or bounced etc. 
    // Disables manual control of knife
    private bool blockControl;

    public override void Throw(Vector3 _velocity)
    {
        // record velocity magnitude (used when realigning after boosts etc)
        initialVelMagnitude = _velocity.magnitude * throwStrengthMod;
        this.PostNotification(AttachLookAheadColliderNotification, this);

        // Setup initial bezier points
        startPos = transform.position;
        tangentTwo = _velocity * throwStrengthMod * (boomerangDuration * 0.5f);
        tangentOnePos = startPos + tangentTwo;
        tangentMagnitude = tangentTwo.magnitude;
    }

    void FixedUpdate()
    {
        if (returning)
            return;

        warpTimer += Time.fixedDeltaTime;

        if (HasStuck())
            return;

        // calculate transition variables, bezier points etc
        transition = warpTimer / boomerangDuration;

        if (transition <= 1f)
        {
            Vector3 ownerPos = ownerTransform.position;
            Vector3 viewDirection = ownerTransform.forward * tangentMagnitude;
            tangentTwo = Vector3.RotateTowards(tangentTwo, viewDirection, 0.5f * Mathf.Deg2Rad, 0f);

            rb.MovePosition(Utilities.LerpBezier(startPos, tangentOnePos, (blockControl) ? tangentOnePos : ownerPos + tangentTwo, ownerPos, transition));
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
        else
        {
            // Reflect knife off surface
            Vector3 newVel = Vector3.Reflect(GetEffectiveVelocity(), collide.normal);
            transform.position = collide.point;
            ResetBezier(newVel, true);
        }
    }

    public Vector3 GetEffectiveVelocity()
    {
        Vector3 ownerPos = ownerTransform.position;
        return Utilities.BezierDerivative(startPos, tangentOnePos, ownerPos + tangentTwo, ownerPos, transition);
    }

    public override void OnBoostNotification(object sender, object args)
    {
        Info<GameObject, Vector3> info = (Info<GameObject, Vector3>)args;
        if (info.arg0 != gameObject)
            return;

        // boosted object is this knife
        ResetBezier(info.arg1);
    }

    private void ResetBezier(Vector3 _newVelocity, bool _useNewMagnitude = false)
    {
        Vector3 newVel = (_useNewMagnitude) ? _newVelocity * throwStrengthMod : _newVelocity.normalized * initialVelMagnitude;
        startPos = transform.position;

        if (_useNewMagnitude)
        {
            boomerangDuration -= warpTimer;
        }

        tangentOnePos = startPos + newVel * (boomerangDuration * 0.5f);
        warpTimer = 0f;

        blockControl = true;
    }
}
