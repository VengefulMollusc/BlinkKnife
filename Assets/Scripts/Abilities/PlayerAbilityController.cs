using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityController : MonoBehaviour
{
    // TODO: just for testing. switch to scroll wheel if can be bothered
    private KeyCode nextAbility = KeyCode.Tab;
    private KeyCode ability1 = KeyCode.Alpha1;
    private KeyCode ability2 = KeyCode.Alpha2;

    private AbilityType currentType;
    private List<Ability> playerAbilities;

    enum AbilityType
    {
        None,
        MissileRedirect,
        SuperJump
    };

    void Start()
    {
        SetupAbilities();
    }

    void Update()
    {
        if (Input.GetKeyDown(ability1))
        {
            ToggleAbility(AbilityType.SuperJump);
        }

        if (Input.GetKeyDown(ability2))
        {
            ToggleAbility(AbilityType.MissileRedirect);
        }
    }

    private void SetupAbilities()
    {
        playerAbilities = new List<Ability>();
        Ability ability;

        foreach (AbilityType type in System.Enum.GetValues(typeof(AbilityType)))
        {
            switch (type)
            {
                case AbilityType.MissileRedirect:
                    ability = gameObject.AddComponent<MissileRedirectAbility>();
                    ability.enabled = false;
                    playerAbilities.Add(ability);
                    break;
                case AbilityType.SuperJump:
                    ability = gameObject.AddComponent<SuperJumpAbility>();
                    ability.enabled = false;
                    playerAbilities.Add(ability);
                    break;
                default: // Case for None
                    playerAbilities.Add(null);
                    break;
            }
        }
    }

    private void ToggleAbility(AbilityType type)
    {
        Ability ability = playerAbilities[(int) type];
        if (ability == null)
            return;

        if (ability.IsActive())
            ability.Deactivate();
        else
            ability.Activate();
    }
}
