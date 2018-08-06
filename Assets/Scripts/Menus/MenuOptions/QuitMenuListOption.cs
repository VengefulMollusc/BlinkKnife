using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitMenuListOption : MenuListItem
{
    public override void Select()
    {
        // save any game data here
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
