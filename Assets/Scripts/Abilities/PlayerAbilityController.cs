using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityController : MonoBehaviour
{
    /*
     * Sets up and controls which player abilities are active
     */
    // TODO: just for testing. switch to scroll wheel if can be bothered
    private KeyCode ability1 = KeyCode.Alpha1;
    private KeyCode ability2 = KeyCode.Alpha2;
    private KeyCode ability3 = KeyCode.Alpha3;
    
    private List<Ability> playerAbilities;

    public enum AbilityType
    {
        DoubleJump,
        SuperJump,
        MissileRedirect
    };

    void Start()
    {
        SetupAbilities();
    }

    void Update()
    {
        if (Input.GetKeyDown(ability1))
        {
            ToggleAbility(AbilityType.DoubleJump);
        }

        if (Input.GetKeyDown(ability2))
        {
            ToggleAbility(AbilityType.SuperJump);
        }

        if (Input.GetKeyDown(ability3))
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
                case AbilityType.DoubleJump:
                    ability = gameObject.AddComponent<DoubleJumpAbility>();
                    ability.enabled = false;
                    playerAbilities.Add(ability);
                    break;
                case AbilityType.SuperJump:
                    ability = gameObject.AddComponent<SuperJumpAbility>();
                    ability.enabled = false;
                    playerAbilities.Add(ability);
                    break;
                case AbilityType.MissileRedirect:
                    ability = gameObject.AddComponent<MissileRedirectAbility>();
                    ability.enabled = false;
                    playerAbilities.Add(ability);
                    break;
            }
        }
    }

    public void EnableAbility(AbilityType type)
    {
        playerAbilities[(int)type].Enable();
    }

    public void DisableAbility(AbilityType type)
    {
        playerAbilities[(int)type].Disable();
    }

    private void ToggleAbility(AbilityType type)
    {
        Ability ability = playerAbilities[(int) type];
        if (ability.IsActive())
            ability.Disable();
        else
            ability.Enable();
    }
}
