using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggeredObject : MonoBehaviour
{
    public virtual void Trigger()
    {
        // logic here
        Debug.Log("Triggered");
    }
}
