using UnityEngine;

public abstract class TriggeredObject : MonoBehaviour
{
    /*
     * Abstract trigger method.
     * Toggles active script behaviour
     */
    public abstract void ToggleTrigger();

    /*
     * Activates behaviour with a specific state
     */
    public abstract void Trigger(bool active);
}
