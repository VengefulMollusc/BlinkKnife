using UnityEngine;

[RequireComponent(typeof(LightSensor))]
public class LightSensorInteractionTrigger : InteractionTrigger
{
    /*
     * InteractionTrigger that triggers object based on object lit status.
     */
    private bool isLit;

    [SerializeField] private const float lightThreshold = 0.2f;
    
    void OnEnable()
    {
        this.AddObserver(OnLightStatusNotification, LightSensor.LightStatusNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnLightStatusNotification, LightSensor.LightStatusNotification);
    }

    /*
     * Updates TriggeredObject based on LightSensor status
     */
    void OnLightStatusNotification(object sender, object args)
    {
        Info<GameObject, float> info = (Info<GameObject, float>) args;
        if (info.arg0 != gameObject)
            return;

        bool newLightState = info.arg1 > lightThreshold;
        if (newLightState != isLit)
        {
            // Updates TriggeredObjects if light status changes
            isLit = newLightState;
            ActivateTriggers(isLit);
        }
    }
}
