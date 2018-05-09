using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class BoomerangKnifeController : KnifeController
{
    [SerializeField] private float boomerangDuration = 2f;

    private float recallRange = 1f;

    private Vector3 startPos;
    private Vector3 tangentPoint;

    void Start()
    {
        recallRange = recallRange * recallRange;
    }

    public override void Throw(Vector3 _velocity)
    {
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

        float t = warpTimer / boomerangDuration;
        if (t <= 1f)
        {
            rb.MovePosition(Utilities.LerpBezier(startPos, tangentPoint, tangentPoint, ownerTransform.position, t));
        }
        else
        {
            this.PostNotification(ReturnKnifeNotification);
        }

        //// update boomerang physics
        //if (warpTimer > 0.5f)
        //{
        //    Vector3 dir = ownerTransform.position - transform.position;
        //    rb.AddForce(dir.normalized * boomerangForce, ForceMode.Acceleration);

        //    if (dir.sqrMagnitude < recallRange)
        //        this.PostNotification(ReturnKnifeNotification);
        //}
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
}
