using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour {

    public virtual void Activate()
    {
        // Perform ability logic here
        Debug.LogError("Activate() must be overridden");
    }

    public virtual void EndActivation()
    {
        // do the thing here
    }
}
