using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class LayoutAnchor : MonoBehaviour {

    RectTransform myRT;
    RectTransform parentRT;

    private void Awake()
    {
        myRT = transform as RectTransform;
        parentRT = transform.parent as RectTransform;
        if (parentRT == null)
            Debug.LogError("This component requires a parent RectTransform to work", gameObject);
    }

    /*When positioning our RectTransform, we will need to know the general
     * offsets to use based on the location of the anchor we want and the 
     * size of the RectTransform’s rect. Let’s make a method which allows 
     * us to get this information from either of the RectTransforms we might want to pass to it*/
    Vector2 GetPosition (RectTransform rt, TextAnchor anchor)
    {
        Vector2 retValue = Vector2.zero;

        switch (anchor)
        {
            case TextAnchor.LowerCenter:
            case TextAnchor.MiddleCenter:
            case TextAnchor.UpperCenter:
                retValue.x += rt.rect.width * 0.5f;
                break;
            case TextAnchor.LowerRight:
            case TextAnchor.MiddleRight:
            case TextAnchor.UpperRight:
                retValue.x += rt.rect.width;
                break;
        }

        switch (anchor)
        {
            case TextAnchor.MiddleLeft:
            case TextAnchor.MiddleCenter:
            case TextAnchor.MiddleRight:
                retValue.y += rt.rect.height * 0.5f;
                break;
            case TextAnchor.UpperLeft:
            case TextAnchor.UpperCenter:
            case TextAnchor.UpperRight:
                retValue.y += rt.rect.height;
                break;
        }

        return retValue;
    }

    /*purpose is to find the value you would use to make a RectTransform appear in the correct
     * place based on the anchor points you specify. I wanted this to work regardless of the 
     * RectTransform’s own pivot and anchor settings, which is why this method is more complex 
     * than it could be. For example, if you could assume that both the parent and current 
     * RectTrasnform had values of zero for all anchor and pivot settings then the calcuations 
     * would have been very easy to determine. However such an assumption doesnt allow for 
     * things like anchors which stretch a UI element based on the parent canvas, screen aspect 
     * ratio, etc.*/
    public Vector2 AnchorPosition(TextAnchor myAnchor, TextAnchor parentAnchor, Vector2 offset)
    {
        Vector2 myOffset = GetPosition(myRT, myAnchor);
        Vector2 parentOffset = GetPosition(parentRT, parentAnchor);
        Vector2 anchorCenter = new Vector2(Mathf.Lerp(myRT.anchorMin.x, myRT.anchorMax.x, myRT.pivot.x), Mathf.Lerp(myRT.anchorMin.y, myRT.anchorMax.y, myRT.pivot.y));
        Vector2 myAnchorOffset = new Vector2(parentRT.rect.width * anchorCenter.x, parentRT.rect.height * anchorCenter.y);
        Vector2 myPivotOffset = new Vector2(myRT.rect.width * myRT.pivot.x, myRT.rect.height * myRT.pivot.y);
        Vector2 pos = parentOffset - myAnchorOffset - myOffset + myPivotOffset + offset;
        pos.x = Mathf.RoundToInt(pos.x);
        pos.y = Mathf.RoundToInt(pos.y);
        return pos;
    }

    public void SnapToAnchorPosition(TextAnchor myAnchor, TextAnchor parentAnchor, Vector2 offset)
    {
        myRT.anchoredPosition = AnchorPosition(myAnchor, parentAnchor, offset);
    }

    // animate moving recttransform into position
    public Tweener MoveToAnchorPosition(TextAnchor myAnchor, TextAnchor parentAnchor, Vector2 offset)
    {
        return myRT.AnchorTo(AnchorPosition(myAnchor, parentAnchor, offset));
    }
}
