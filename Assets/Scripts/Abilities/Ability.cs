using UnityEngine;

public class Ability : MonoBehaviour
{
    /*
     * Base class for Abilities.
     * Abilities are small pieces of optional functionality that can be toggled on/off and aren't part of the default player code/features
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

    public virtual string GetDisplayName()
    {
        Debug.LogError("GetDisplayName must be overridden");
        return "undefined display name";
    }
}
