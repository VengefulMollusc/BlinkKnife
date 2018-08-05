using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    private KeyCode pauseKey = KeyCode.P;
    private bool paused;

    private SubMenuListItem rootMenu;
    private SubMenuListItem currentMenu;

    void Start()
    {
        rootMenu = GetComponent<SubMenuListItem>();
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
            Pause(!paused);

        if (!paused || rootMenu == null)
            return;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMenu.Next();

        if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMenu.Previous();

        if (Input.GetKeyDown(KeyCode.RightArrow))
            Select();

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            Back();

    }

    private void Select()
    {
        MenuListItem selectedItem = currentMenu.CurrentItem();
        Debug.Log("Select " + selectedItem.ItemText());
        if (selectedItem is SubMenuListItem)
        {
            currentMenu.DisplayMenu(false);
            currentMenu = (SubMenuListItem)selectedItem;
            currentMenu.DisplayMenu(true);
        }
        else
        {
            selectedItem.Select();
        }
    }

    private void Back()
    {
        SubMenuListItem parentMenu = currentMenu.ParentItem();
        if (parentMenu == null)
        {
            Pause(false);
            return;
        }

        currentMenu.DisplayMenu(false);
        currentMenu = parentMenu;
        currentMenu.DisplayMenu(true);
    }

    public void Pause(bool _paused)
    {
        paused = _paused;
        TimeController.Pause(paused);

        if (rootMenu == null)
            return;

        if (paused)
        {
            currentMenu = rootMenu;
            currentMenu.DisplayMenu(true);
        }
        else
        {
            currentMenu.DisplayMenu(false);
        }
    }
}
