using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    /*
     * Base class for Abilities.
     * Abilities are small pieces of optional functionality that can be toggled on/off and aren't part of the default player code/features
     * 
     * Depending on individual logic, multiple abilities can activate on one button. 
     * I'll eventually need a system for restricting them to 'slots' or something
     */
    public virtual void Enable()
    {
        enabled = true;
        Debug.Log(GetDisplayName() + " Activated");
    }

    public virtual void Disable()
    {
        enabled = false;
        Debug.Log(GetDisplayName() + " Deactivated");
    }

    public bool IsActive()
    {
        return enabled;
    }

    public abstract string GetDisplayName();
}
