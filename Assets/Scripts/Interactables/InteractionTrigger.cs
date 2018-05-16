using System.Collections.Generic;
using UnityEngine;

public abstract class InteractionTrigger : MonoBehaviour
{
    /*
     * Handles interaction with triggerable objects etc.
     * 
     * Extended to handle different triggers: player presence, knife stuck to object etc.
     */

    [SerializeField]
    private List<TriggeredObject> triggeredObjects;

    // controls which boolean state is triggered as the 'active' state.
    // setting to false will invert the effects of this trigger on triggered objects
    [SerializeField] private bool invertStates;

    void Start()
    {
        if (triggeredObjects == null || triggeredObjects.Count == 0)
            Debug.LogError("No TriggeredObjects given");
    }

    /*
     * Activates each attached TriggeredObject
     */
    protected void ToggleTriggers()
    {
        foreach (TriggeredObject obj in triggeredObjects)
        {
            obj.ToggleTrigger();
        }
    }

    /*
     * Activates each attached TriggeredObject with a specific active state
     */
    protected void ActivateTriggers(bool active)
    {
        bool state = invertStates ? !active : active;
        foreach (TriggeredObject obj in triggeredObjects)
        {
            obj.TriggerState(state);
        }
    }
}
