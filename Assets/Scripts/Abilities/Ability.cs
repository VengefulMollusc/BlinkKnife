using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    public virtual void Activate()
    {
        // Perform ability logic here
        Debug.LogError("Activate must be overridden");
    }

    public virtual void EndActivation()
    {
        // stop doing the thing here
    }

    public virtual string GetDisplayName()
    {
        Debug.LogError("GetDisplayName needs to be overridden");
        return "No display name given";
    }
}
