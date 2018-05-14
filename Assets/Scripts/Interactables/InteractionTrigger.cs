using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    private bool active;

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
