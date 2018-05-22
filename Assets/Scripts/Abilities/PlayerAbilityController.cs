using System.Collections.Generic;
using UnityEngine;

/*
 * An Enum that catalogues all possible abilities for use by the player
 */
public enum AbilityType
{
    DoubleJump,
    SuperJump,
    Hover,
    JumpDash,
    MissileRedirect,
    WallRun
};

public class PlayerAbilityController : MonoBehaviour
{
    /*
     * Sets up and controls which player abilities are active
     */
    // TODO: Make ingame method for switching
    private KeyCode ability1 = KeyCode.Alpha1;
    private KeyCode ability2 = KeyCode.Alpha2;
    private KeyCode ability3 = KeyCode.Alpha3;
    private KeyCode ability4 = KeyCode.Alpha4;
    private KeyCode ability5 = KeyCode.Alpha5;
    private KeyCode ability6 = KeyCode.Alpha6;

    // list of available abilities
    private List<Ability> playerAbilities;

    void Awake()
    {
        SetupAbilities();
    }

    /*
     * TODO: For testing
     * 
     * Allows easy toggling of abilities for testing
     */
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            string abilityInfo = "";
            foreach (AbilityType type in System.Enum.GetValues(typeof(AbilityType)))
            {
                abilityInfo += ((int)type + 1) + ": " + type + "  ";
            }
            Debug.Log(abilityInfo);
        }

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
            ToggleAbility(AbilityType.Hover);
        }

        if (Input.GetKeyDown(ability4))
        {
            ToggleAbility(AbilityType.JumpDash);
        }

        if (Input.GetKeyDown(ability5))
        {
            ToggleAbility(AbilityType.MissileRedirect);
        }

        if (Input.GetKeyDown(ability6))
        {
            ToggleAbility(AbilityType.WallRun);
        }
    }

    /*
     * Creates a set of ability components and attaches them to the player object (if they do not already exist)
     */
    private void SetupAbilities()
    {
        playerAbilities = new List<Ability>();

        foreach (AbilityType type in System.Enum.GetValues(typeof(AbilityType)))
        {
            Ability ability;
            switch (type)
            {
                case AbilityType.DoubleJump:
                    ability = GetComponent<DoubleJumpAbility>();
                    if (ability == null)
                        ability = gameObject.AddComponent<DoubleJumpAbility>();
                    ability.enabled = false;
                    playerAbilities.Add(ability);
                    break;
                case AbilityType.SuperJump:
                    ability = GetComponent<SuperJumpAbility>();
                    if (ability == null)
                        ability = gameObject.AddComponent<SuperJumpAbility>();
                    ability.enabled = false;
                    playerAbilities.Add(ability);
                    break;
                case AbilityType.Hover:
                    ability = GetComponent<HoverAbility>();
                    if (ability == null)
                        ability = gameObject.AddComponent<HoverAbility>();
                    ability.enabled = false;
                    playerAbilities.Add(ability);
                    break;
                case AbilityType.JumpDash:
                    ability = GetComponent<JumpDashAbility>();
                    if (ability == null)
                        ability = gameObject.AddComponent<JumpDashAbility>();
                    ability.enabled = false;
                    playerAbilities.Add(ability);
                    break;
                case AbilityType.MissileRedirect:
                    ability = GetComponent<MissileRedirectAbility>();
                    if (ability == null)
                        ability = gameObject.AddComponent<MissileRedirectAbility>();
                    ability.enabled = false;
                    playerAbilities.Add(ability);
                    break;
                case AbilityType.WallRun:
                    ability = GetComponent<WallRunAbility>();
                    if (ability == null)
                        ability = gameObject.AddComponent<WallRunAbility>();
                    ability.enabled = false;
                    playerAbilities.Add(ability);
                    break;
            }
        }
    }

    /*
     * Enables the given ability
     */
    public void EnableAbility(AbilityType type)
    {
        playerAbilities[(int)type].Enable();
    }

    /*
     * Disables the given ability
     */
    public void DisableAbility(AbilityType type)
    {
        playerAbilities[(int)type].Disable();
    }

    /*
     * Toggles an ability between enabled/disabled
     */
    private void ToggleAbility(AbilityType type)
    {
        Ability ability = playerAbilities[(int)type];
        if (ability.IsActive())
            ability.Disable();
        else
            ability.Enable();
    }
}
