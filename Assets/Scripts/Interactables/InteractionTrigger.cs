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

    public void Start()
    {
        if (triggeredObjects == null)
            Debug.LogError("No TriggeredObjects given");
    }

    /*
     * Activates each attached TriggeredObject
     */
    public void ActivateTriggers()
    {
        foreach (TriggeredObject obj in triggeredObjects)
        {
            obj.Trigger();
        }
    }
}
