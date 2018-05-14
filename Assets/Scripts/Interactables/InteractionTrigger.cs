using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    /*
     * Handles interaction with triggerable objects etc.
     * 
     * Extended to handle different triggers: player presence, knife stuck to object etc.
     */
    private bool active;

    // Used to set the initial state to active.
    [SerializeField]
    private bool initialActiveState;

    public virtual void Start()
    {
        active = initialActiveState;
    }

    public virtual void ToggleActivation()
    {
        if (Active())
            Deactivate();
        else
            Activate();
    }

    public virtual void Activate()
    {
        active = true;
    }

    public virtual void Deactivate()
    {
        active = false;
    }

    public bool Active()
    {
        return active;
    }
}
