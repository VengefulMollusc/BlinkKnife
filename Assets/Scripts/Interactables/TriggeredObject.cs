using UnityEngine;

public abstract class TriggeredObject : MonoBehaviour
{
    /*
     * Abstract trigger method.
     * Activates script behaviour
     */
    public abstract void Trigger();

    /*
     * Activates behaviour with a specific state
     */
    public abstract void Trigger(bool active);
}
