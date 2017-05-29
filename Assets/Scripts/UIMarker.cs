using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIMarker : MonoBehaviour {

    /*
     * Handles visual ui markers 
     */

    [SerializeField]
    private bool useOnScreenMarker = true;
    [SerializeField]
    private GameObject onScreenMarkerPrefab;
    private GameObject onScreenMarker;
    private Image onScreenImage;

    [SerializeField]
    private bool useOffScreenMarker = true;
    [SerializeField]
    private GameObject offScreenMarkerPrefab;
    private GameObject offScreenMarker;
    private Image offScreenImage;
    private float lastAngle = 0f;
    
    private GameObject UiParent;

    private Vector3 center;

    [SerializeField]
    private Color UiColor = new Color32(255, 0, 0, 255);

    [SerializeField]
    private bool markerEnabled = false;

    // Use this for initialization
    void Start () {

        center = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        UiParent = GameObject.FindGameObjectWithTag("UIParent");

        if (useOnScreenMarker)
        {
            onScreenMarker = (GameObject)Instantiate(onScreenMarkerPrefab);
            onScreenMarker.transform.SetParent(UiParent.transform, false);
            onScreenImage = onScreenMarker.GetComponent<Image>();
            onScreenImage.color = UiColor;
            onScreenImage.enabled = false;
        }

        if (useOffScreenMarker)
        {
            offScreenMarker = (GameObject)Instantiate(offScreenMarkerPrefab);
            offScreenMarker.transform.SetParent(UiParent.transform, false);
            offScreenImage = offScreenMarker.GetComponent<Image>();
            offScreenImage.color = UiColor;
            offScreenImage.enabled = false;
            offScreenImage.transform.position = center + (Vector3.up * Screen.height / 2.1f);
        }
    }
	
	// Update is called once per frame
	void Update () {

        if (!markerEnabled) return;

        // there is a target
        Vector3 targetPosOnScreen = Camera.main.WorldToScreenPoint(transform.position);

        if (useOnScreenMarker && Utilities.IsOnScreen(targetPosOnScreen) && Utilities.IsInFront(Camera.main.transform, transform))
        {
            // target is visible within the screen area
            onScreenImage.enabled = true;
            offScreenImage.enabled = false;

            onScreenImage.transform.position = targetPosOnScreen;
        }
        else if (useOffScreenMarker) {

            Vector3 targetRelative = transform.position - Camera.main.transform.position;
            Quaternion toTarget = Quaternion.FromToRotation(Camera.main.transform.forward, targetRelative);

            // THIS WORKS
            // OMG.
            Vector3 relativePos = targetPosOnScreen - center;

            if (relativePos.z < 0) relativePos.y = -relativePos.y;

            float angle = -Vector3.Angle(Vector3.up, relativePos);
            if (relativePos.x < 0) angle = -angle;
            if (relativePos.z < 0) angle = -angle;

            float angleDiff = angle - lastAngle;
            lastAngle = angle;

            onScreenImage.enabled = false;
            offScreenImage.enabled = true;

            offScreenImage.transform.RotateAround(center, Vector3.forward, angleDiff);
        }
    }

    public void EnableMarker(bool _markerEnabled)
    {
        markerEnabled = _markerEnabled;
    }

    public void DestroyMarker()
    {
        markerEnabled = false;
        Destroy(onScreenMarker);
        Destroy(offScreenMarker);
    }

    public Image GetOnScreenImage()
    {
        return onScreenImage;
    }

    public Image GetOffScreenImage()
    {
        return onScreenImage;
    }

    private void UpdateColour()
    {
        onScreenImage.color = UiColor;
        offScreenImage.color = UiColor;
    }

    public void SetAlpha(float _newAlpha)
    {
        UiColor.a = _newAlpha;
        UpdateColour();
    }
}
