using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class BlinkKnifeController : KnifeController
{
    /*
     * 'Default' knife option.
     * Useful for accurate traversal of longer distances
     * 
     * Travels in a straight line until it hits something or until it reaches the time limit.
     * 
     * Sticks into surfaces and waits for player input to warp
     */

    [SerializeField]
    private float timeToAutoRecall = 1.5f;

    void Update()
    {
        if (returning)
            return;

        warpTimer += Time.deltaTime;

        if (!CanWarp() && warpTimer > timeToAutoRecall)
            // Return knife
            ReturnKnifeTransition();
    }

    void OnCollisionEnter(Collision _col)
    {
        if (HasStuck())
            return;

        ContactPoint collide = _col.contacts[0];
        GameObject other = collide.otherCollider.gameObject;

        // If collided surface is not a HardSurface, stick knife into it
        // else bounce and reset timer
        if (other.GetComponent<HardSurface>() == null)
            StickToSurface(collide.point, collide.normal, other);
        else
            warpTimer = 0f;
    }
}
