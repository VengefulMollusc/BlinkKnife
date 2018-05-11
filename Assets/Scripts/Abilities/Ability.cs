using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    public virtual void Activate()
    {
        enabled = true;
        Debug.Log(GetDisplayName() + " Activated");
    }

    public virtual void Deactivate()
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
