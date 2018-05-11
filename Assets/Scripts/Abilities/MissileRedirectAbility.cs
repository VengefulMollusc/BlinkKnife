using System.Collections.Generic;
using UnityEngine;

public class MissileRedirectAbility : Ability
{
    /*
     * retargets all currently active missiles to the current knife object in the scene
     */
    private const string displayName = "Missile Retarget";
    private const KeyCode useAbility = KeyCode.Q;

    private PlayerKnifeController playerKnifeController;

    void Start()
    {
        playerKnifeController = GetComponent<PlayerKnifeController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(useAbility))
        {
            RedirectMissiles();
        }
    }

    void RedirectMissiles()
    {
        KnifeController knife = playerKnifeController.GetActiveKnifeController();
        if (knife == null)
            return;

        // Get all currently active missiles
        List<MissileController> missiles = new List<MissileController>();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Missile"))
        {
            MissileController controller = obj.GetComponent<MissileController>();
            if (controller != null)
                missiles.Add(controller);
        }

        if (missiles.Count <= 0)
            return;

        // apply new target
        foreach (MissileController missile in missiles)
        {
            // set target here!
            missile.SetTarget(knife.transform);
        }
    }

    public override string GetDisplayName()
    {
        return displayName;
    }
}
