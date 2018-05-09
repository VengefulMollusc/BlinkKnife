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
    private bool disableControl;

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

    /*
     * Handles the movement logic of the knife.
     * Transitions along bezier curve to give boomerang motion
     */
    void FixedUpdate()
    {
        if (returning)
            return;

        warpTimer += Time.fixedDeltaTime;

        if (HasStuck())
            return;

        // calculate 0-1 transition variable
        transition = warpTimer / boomerangDuration;

        if (transition <= 1f)
        {
            // Calculate current bezier points based on current owner view direction
            Vector3 ownerPos = ownerTransform.position;
            if (!disableControl)
            {
                // calculate second tangent if manual control is not disabled
                Vector3 viewDirection = ownerTransform.forward * tangentMagnitude;
                tangentTwo = Vector3.RotateTowards(tangentTwo, viewDirection, 0.5f * Mathf.Deg2Rad, 0f);
            }

            // apply position based on current bezier
            rb.MovePosition(Utilities.LerpBezier(startPos, tangentOnePos, (disableControl) ? tangentOnePos : ownerPos + tangentTwo, ownerPos, transition));
        }
        else
        {
            // Full duration reached. Return knife.
            // return transition not needed as knife should be at owner position already
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
            Vector3 newVel = Vector3.Reflect(GetVelocity(), collide.normal);
            transform.position = collide.point;
            ResetBezier(newVel, true);
        }
    }

    /*
     * returns the derivative of the bezier at the current time.
     * Needed as rigidbody velocity should be zero (as movement is handled by bezier lerping)
     */
    public override Vector3 GetVelocity()
    {
        Vector3 ownerPos = ownerTransform.position;
        return Utilities.BezierDerivative(startPos, tangentOnePos, ownerPos + tangentTwo, ownerPos, transition);
    }

    /*
     * Recalculates the bezier if knife is boosted
     */
    public override void OnBoostNotification(object sender, object args)
    {
        Info<GameObject, Vector3> info = (Info<GameObject, Vector3>)args;
        if (info.arg0 != gameObject)
            return;

        // boosted object is this knife
        ResetBezier(info.arg1);
    }

    /*
     * Recalculates the bezier to start from the current position and new given direction.
     * Used when knife is boosted or bounces off a surface
     * 
     * _useNewMagnitude determines if velocity/duration is inherited or reset
     */
    private void ResetBezier(Vector3 _newVelocity, bool _useNewMagnitude = false)
    {
        Vector3 newVel = (_useNewMagnitude) ? _newVelocity * throwStrengthMod : _newVelocity.normalized * initialVelMagnitude;
        startPos = transform.position;

        if (_useNewMagnitude)
        {
            // resets duration according to time left in original duration
            boomerangDuration -= warpTimer;
        }

        tangentOnePos = startPos + newVel * (boomerangDuration * 0.5f);
        warpTimer = 0f;

        // Disables manual control after a reset
        disableControl = true;
    }
}
