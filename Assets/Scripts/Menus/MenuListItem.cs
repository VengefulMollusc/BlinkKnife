using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class MenuListItem : MonoBehaviour
{
    public string itemText;

    private SubMenuListItem parentItem;

    public virtual void Start()
    {
        if (transform.parent != null)
            parentItem = transform.parent.GetComponent<SubMenuListItem>();
    }

    public SubMenuListItem ParentItem()
    {
        return parentItem;
    }

    public abstract void Select();
}
