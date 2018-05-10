using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileRedirectAbility : Ability
{
    private string displayName = "Missile Retarget";

    public override void Activate()
    {
        // Do the thing
    }

    public override string GetDisplayName()
    {
        return displayName;
    }
}
