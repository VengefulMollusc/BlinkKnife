using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    /*
     * Handles interaction with triggerable objects etc.
     * 
     * Extended to handle different triggers: player presence, knife stuck to object etc.
     */
    [SerializeField]
    private TriggeredObject triggeredObject;

    public virtual void Start()
    {
        if (triggeredObject == null)
            Debug.LogError("No TriggeredObject given");
    }

    public void TriggerActivation()
    {
        triggeredObject.Trigger();
    }
}
