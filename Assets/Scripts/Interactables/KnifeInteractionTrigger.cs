using UnityEngine;

[RequireComponent(typeof(SoftSurface))]
public class KnifeInteractionTrigger : InteractionTrigger
{
    /*
     * Controls logic for Interactions that are detemined by Knife contact.
     * 
     * Eg: door opens while knife is stuck to switch;
     * Platform position is toggled when knife hits it etc.
     */

    // only activate when player warps to this object
    [SerializeField] private bool activateOnWarp;

    // only active while knife is attached. deactivate when knife is removed
    [SerializeField] private bool activeWhileKnifeAttached;

    private bool knifeAttached;

    void OnEnable()
    {
        this.AddObserver(OnKnifeReturnTransition, KnifeController.ReturnKnifeTransitionNotification);
        this.AddObserver(OnWarpEndNotification, TransitionCameraController.WarpEndNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnKnifeReturnTransition, KnifeController.ReturnKnifeTransitionNotification);
        this.RemoveObserver(OnWarpEndNotification, TransitionCameraController.WarpEndNotification);
    }

    // De-attaches knife on return and Triggers objects again if active only while knife attached
    void OnKnifeReturnTransition(object sender, object args)
    {
        if (knifeAttached)
        {
            if (activeWhileKnifeAttached)
                ActivateTriggers(false);

            knifeAttached = false;
        }
    }

    // calls knife return logic on warp end. Also toggles triggers if activateOnWarp
    void OnWarpEndNotification(object sender, object args)
    {
        if (!knifeAttached)
            return;

        if (activateOnWarp)
        {
            knifeAttached = false;
            ToggleTriggers();
        }
        else
        {
            OnKnifeReturnTransition(null, null);
        }
    }

    /*
     * Attaches knife and triggers objects
     */
    public void AttachKnife()
    {
        knifeAttached = true;

        if (activateOnWarp)
            return;

        if (activeWhileKnifeAttached)
            ActivateTriggers(true);
        else
            ToggleTriggers();

        // if autoReturn, return knife
        if (!activeWhileKnifeAttached)
            this.PostNotification(KnifeController.ReturnKnifeTransitionNotification);
    }
}
