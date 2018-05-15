using UnityEngine;

public class PlayerCollisionInteractionTrigger : InteractionTrigger
{
    /*
     * Logic for interactions that are defined by player collision.
     * 
     * Eg: pressure plate - player must stand on switch to open door etc
     */
    [SerializeField] private bool activeWhileInContact;
    private bool active;

    void OnCollisionStay(Collision col)
    {
        if (col.transform.CompareTag("Player") && !active)
        {
            active = true;
            if (activeWhileInContact)
                ActivateTriggers(true);
            else 
                ToggleTriggers();
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.transform.CompareTag("Player") && active)
        {
            active = false;
            if (activeWhileInContact)
                ActivateTriggers(false);
        }
    }
}
