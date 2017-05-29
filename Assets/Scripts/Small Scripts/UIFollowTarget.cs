using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIFollowTarget : MonoBehaviour {

    private RectTransform myRectTransform;

    private Camera mainCamera;

    public Transform currentTarget;

    public bool clampToScreen = true;

    [SerializeField]
    Vector2 clampBorderSize;

    public Vector3 offset;

    void Awake()
    {
        myRectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (currentTarget == null) return;
        Vector3 noClampPosition = mainCamera.WorldToScreenPoint(currentTarget.position + offset);
        Vector3 clampedPosition = new Vector3(Mathf.Clamp(noClampPosition.x, 0 + clampBorderSize.x, Screen.width - clampBorderSize.x),
                                                                Mathf.Clamp(noClampPosition.y, 0 + clampBorderSize.y, Screen.height - clampBorderSize.y),
                                                                  noClampPosition.z);

        myRectTransform.position = clampToScreen ? clampedPosition : noClampPosition;
    }
}