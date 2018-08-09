using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public InputSettings inputSettings;
    public bool activeOnStart;

    [Header("Components to mirror active state")]
    public List<Behaviour> relatedComponents;

    // Use this for initialization
    public virtual void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SetActiveState(activeOnStart);
    }

    public virtual void SetActiveState(bool active)
    {
        this.enabled = active;

        foreach (Behaviour comp in relatedComponents)
        {
            comp.enabled = active;
        }
    }
}
