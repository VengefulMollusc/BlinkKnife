using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityController : MonoBehaviour
{
    // TODO: just for testing. switch to scroll wheel if can be bothered
    private KeyCode nextAbility = KeyCode.Tab;
    private KeyCode ability1 = KeyCode.Alpha1;
    private KeyCode ability2 = KeyCode.Alpha2;

    //[SerializeField]
    //private AbilityType startingAbility;

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
        //SetAbility(startingAbility);
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
        //if (currentType != AbilityType.None)
        //{
        //    if (Input.GetKeyDown(useAbility))
        //    {
        //        // Do the thing
        //        playerAbilities[(int)currentType].Activate();
        //    }
        //    if (Input.GetKeyUp(useAbility))
        //    {
        //        // Stop doing the thing
        //        playerAbilities[(int)currentType].EndActivation();
        //    }
        //}

        //if (Input.GetKeyDown(nextAbility))
        //{
        //    NextAbility();
        //}
        //if (Input.GetKeyDown(prevAbility))
        //{
        //    PreviousAbility();
        //}
    }

    private void SetupAbilities()
    {
        playerAbilities = new List<Ability>();

        foreach (AbilityType type in System.Enum.GetValues(typeof(AbilityType)))
        {
            switch (type)
            {
                case AbilityType.MissileRedirect:
                    playerAbilities.Add(gameObject.AddComponent<MissileRedirectAbility>());
                    break;
                case AbilityType.SuperJump:
                    playerAbilities.Add(gameObject.AddComponent<SuperJumpAbility>());
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
        if (ability.IsActive())
            ability.Deactivate();
        else
            ability.Activate();

    }

    //private void SetAbility(AbilityType type)
    //{
    //    playerAbilities[(int)currentType].Deactivate();
    //    currentType = type;
    //    playerAbilities[(int)currentType].Activate();

    //    Debug.Log("Switched to " + ((currentType != AbilityType.None) ? playerAbilities[(int)currentType].GetDisplayName() : type.ToString()));
    //}

    //private void NextAbility()
    //{
    //    AbilityType newType = currentType + 1;
    //    if ((int)newType >= playerAbilities.Count)
    //        newType = 0;
    //    SetAbility(newType);
    //}

    //private void PreviousAbility()
    //{
    //    AbilityType newType = currentType - 1;
    //    if ((int)newType < 0)
    //        newType = (AbilityType)(playerAbilities.Count - 1);
    //    SetAbility(newType);
    //}
}
