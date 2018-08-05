using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using UnityEngine;

public class SubMenuListItem : MenuListItem
{
    private MenuListItem[] subMenuItems;
    private int currentIndex;

    public override void Start()
    {
        base.Start();
        subMenuItems = GetComponentsInChildren<MenuListItem>();
        subMenuItems = subMenuItems.RemoveAt(0);
    }

    public override void Select()
    {
        Debug.Log("Open Submenu: " + ItemText());
    }

    public MenuListItem CurrentItem()
    {
        return subMenuItems[currentIndex];
    }

    public void Next()
    {
        currentIndex++;
        currentIndex = Mathf.Clamp(currentIndex, 0, subMenuItems.Length - 1);

        Debug.Log("Currently Selected: " + CurrentItem().ItemText());
    }

    public void Previous()
    {
        currentIndex--;
        currentIndex = Mathf.Clamp(currentIndex, 0, subMenuItems.Length - 1);

        Debug.Log("Currently Selected: " + CurrentItem().ItemText());
    }

    public void DisplayMenu(bool visible)
    {
        currentIndex = 0;
    }
}
