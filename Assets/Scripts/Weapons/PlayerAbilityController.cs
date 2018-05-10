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
    private Ability currentAbility;

    private int numberOfAbilities;

    enum AbilityType
    {
        None,
        MissileRedirect,
        SuperJump
    };

    void Start()
    {
        SetAbility(startingAbility);
        numberOfAbilities = System.Enum.GetValues(typeof(AbilityType)).Length;
    }

    private void SetAbility(AbilityType type)
    {
        if (type == currentType)
            return;

        Destroy(currentAbility);
        currentType = type;
        switch (type)
        {
            case AbilityType.MissileRedirect:
                currentAbility = gameObject.AddComponent<MissileRedirectAbility>();
                break;
            case AbilityType.SuperJump:
                currentAbility = gameObject.AddComponent<SuperJumpAbility>();
                break;
            default: // Case for None
                currentAbility = null;
                break;
        }

        Debug.Log("Switched to " + ((currentAbility != null) ? currentAbility.GetDisplayName() : type.ToString()));
    }

    private void NextAbility()
    {
        AbilityType newType = currentType + 1;
        if ((int)newType >= numberOfAbilities)
            newType = 0;
        SetAbility(newType);
    }

    private void PreviousAbility()
    {
        AbilityType newType = currentType - 1;
        if ((int)newType < 0)
            newType = (AbilityType)(numberOfAbilities - 1);
        SetAbility(newType);
    }

    void Update()
    {
        if (currentAbility != null)
        {
            if (Input.GetKeyDown(useAbility))
            {
                // Do the thing
                currentAbility.Activate();
            }
            if (Input.GetKeyUp(useAbility))
            {
                // Stop doing the thing
                currentAbility.EndActivation();
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
}
