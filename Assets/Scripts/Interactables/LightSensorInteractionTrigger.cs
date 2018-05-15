using UnityEngine;

[RequireComponent(typeof(LightSensor))]
public class LightSensorInteractionTrigger : InteractionTrigger
{
    /*
     * InteractionTrigger that triggers object based on object lit status.
     */
    private bool isLit;
    
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

        bool newLightState = info.arg1 > 0f;
        if (newLightState != isLit)
        {
            isLit = newLightState;
            ActivateTriggers(isLit);
        }
    }
}
