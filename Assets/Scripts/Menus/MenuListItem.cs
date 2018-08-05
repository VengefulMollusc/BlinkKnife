using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class MenuListItem : MonoBehaviour
{
    [SerializeField]
    private string itemText;

    private SubMenuListItem parentItem;

    public virtual void Start()
    {
        if (transform.parent != null)
            parentItem = transform.parent.GetComponent<SubMenuListItem>();
    }

    public string ItemText()
    {
        return itemText;
    }

    public SubMenuListItem ParentItem()
    {
        return parentItem;
    }

    public abstract void Select();
}
