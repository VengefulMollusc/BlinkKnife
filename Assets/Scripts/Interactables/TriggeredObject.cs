using UnityEngine;

public abstract class TriggeredObject : MonoBehaviour
{
    [SerializeField]
    private bool invertActiveState;
    /*
     * Abstract trigger method.
     * Toggles active script behaviour
     */
    public abstract void ToggleTrigger();

    /*
     * Activates behaviour with a specific state. Inverting if required
     */
    public void Trigger(bool active)
    {
        SetTriggerState(invertActiveState ? !active : active);
    }

    protected abstract void SetTriggerState(bool active);
}
