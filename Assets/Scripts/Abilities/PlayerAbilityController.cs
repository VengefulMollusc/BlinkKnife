using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityController : MonoBehaviour
{

    [SerializeField]
    private KeyCode useAbility = KeyCode.E;

    // TODO: Next/prev just for testing. switch to scroll wheel if can be bothered
    [SerializeField] private KeyCode nextAbility = KeyCode.Tab;
    [SerializeField] private KeyCode prevAbility = KeyCode.Q;

    [SerializeField]
    private AbilityType startingAbility;

    private AbilityType currentType;
    private List<Ability> playerAbilities;

    private Ability currentAbility;

    //private int numberOfAbilities;

    enum AbilityType
    {
        None,
        MissileRedirect,
        SuperJump
    };

    void Start()
    {
        SetupAbilities();
        SetAbility(startingAbility);
    }

    void Update()
    {
        if (currentType != AbilityType.None)
        {
            if (Input.GetKeyDown(useAbility))
            {
                // Do the thing
                playerAbilities[(int)currentType].Activate();
            }
            if (Input.GetKeyUp(useAbility))
            {
                // Stop doing the thing
                playerAbilities[(int)currentType].EndActivation();
            }
        }

        if (Input.GetKeyDown(nextAbility))
        {
            NextAbility();
        }
        if (Input.GetKeyDown(prevAbility))
        {
            PreviousAbility();
        }
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

    private void SetAbility(AbilityType type)
    {
        currentType = type;

        Debug.Log("Switched to " + ((currentAbility != null) ? currentAbility.GetDisplayName() : type.ToString()));
    }

    private void NextAbility()
    {
        AbilityType newType = currentType + 1;
        if ((int)newType >= playerAbilities.Count)
            newType = 0;
        SetAbility(newType);
    }

    private void PreviousAbility()
    {
        AbilityType newType = currentType - 1;
        if ((int)newType < 0)
            newType = (AbilityType)(playerAbilities.Count - 1);
        SetAbility(newType);
    }
}
