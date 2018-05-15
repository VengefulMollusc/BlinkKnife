using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightToggleTriggeredObject : TriggeredObject
{
    /*
     * Simply TriggeredObject script to toggle a light on/off
     */
    private Light lightComponent;

    void Start()
    {
        lightComponent = GetComponent<Light>();
    }

    /*
     * Toggles light on/off
     */
    public override void Trigger()
    {
        lightComponent.enabled = !lightComponent.enabled;
    }
}
