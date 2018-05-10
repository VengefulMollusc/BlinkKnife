using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Ability
{
    void Activate();

    void EndActivation();

    string GetDisplayName();
}
