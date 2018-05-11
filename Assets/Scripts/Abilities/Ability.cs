using UnityEngine;

public class Ability : MonoBehaviour
{
    /*
     * Base class for Abilities.
     * Controls activating and deactivating of the ability
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
