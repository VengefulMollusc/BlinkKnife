using UnityEngine;

public class KnifeInteractionTrigger : InteractionTrigger
{
    /*
     * Controls logic for Interactions that are detemined by Knife contact.
     * 
     * Eg: door opens while knife is stuck to switch;
     * Platform position is toggled when knife hits it etc.
     */

    // only active while knife is attached. deactivate when knife is removed
    [SerializeField] private bool activeWhileKnifeAttached;

    // automatically return knife when triggered.
    // unsure if this will ever be useful in conjunction with activeWhileKnifeAttached
    [SerializeField] private bool autoReturnKnife;
    private bool knifeAttached;

    void OnEnable()
    {
        this.AddObserver(OnKnifeReturnTransition, KnifeController.ReturnKnifeTransitionNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnKnifeReturnTransition, KnifeController.ReturnKnifeTransitionNotification);
    }
    
    void OnKnifeReturnTransition(object sender, object args)
    {
        if (knifeAttached)
        {
            if (activeWhileKnifeAttached)
                TriggerActivation();

            knifeAttached = false;
        }
    }

    public void AttachKnife()
    {
        knifeAttached = true;
        TriggerActivation();

        if (autoReturnKnife)
            this.PostNotification(KnifeController.ReturnKnifeTransitionNotification);
    }
}
