using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionInteractionTrigger : InteractionTrigger
{
    /*
     * Logic for interactions that are defined by player collision.
     * 
     * Eg: pressure plate - player must stand on switch to open door etc
     */
    [SerializeField] private bool triggerOnExit;
    private bool active;

    void OnCollisionStay(Collision col)
    {
        if (col.transform.CompareTag("Player") && !active)
        {
            ActivateTriggers();
            active = true;
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.transform.CompareTag("Player") && active)
        {
            if (triggerOnExit)
                ActivateTriggers();

            active = false;
        }
    }
}
