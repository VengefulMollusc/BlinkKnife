using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResumeMenuListItem : MenuListItem
{
    private MenuController menuController;

    private void Start()
    {
        menuController = GetComponentInParent<MenuController>();
    }

    public override void Select()
    {
        menuController.Pause(false);
    }
}
