using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public const string ControllerChangeNotification = "Controller.ControllerChangeNotification";

    public InputSettings inputSettings;
    public bool activeOnStart;

    [Header("Components to mirror active state")]
    public List<Behaviour> relatedComponents;

    // Use this for initialization
    public virtual void Start()
    {
        this.AddObserver(OnControllerChangeNotification, ControllerChangeNotification);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SetActiveState(activeOnStart);
    }

    private void OnControllerChangeNotification(object sender, object args)
    {
        Controller newActiveController = (Controller) args;
        SetActiveState(newActiveController == this);
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
