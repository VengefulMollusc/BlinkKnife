using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class KnifeMarker : MonoBehaviour {

	private Transform target;

    [SerializeField]
    private GameObject onScreenMarker;
    private Image onScreenImage;

    [SerializeField]
    private GameObject offScreenMarker;
    private Image offScreenImage;

    [SerializeField]
	private float maxSize = 100f;
	private Vector2 maxSizeVector;
	private float currentSize = 0.0f;

	[SerializeField]
	private float onScreenPulseDuration = 0.5f;
    [SerializeField]
    private float offScreenPulseSpeed = 4f;

	[SerializeField]
	private GameObject playerCamera;

    private float lastAngle = 0f;

    private Vector3 center;
    
    private Color baseUiColor = new Color32(255, 100, 0, 255);
    private Color altUiColor = new Color32(0, 175, 255, 255);


    // Use this for initialization
    void Start () {
		maxSizeVector = new Vector2 (maxSize, maxSize);
        
        onScreenImage = onScreenMarker.GetComponent<Image>();
        offScreenImage = offScreenMarker.GetComponent<Image>();
        center = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        offScreenImage.transform.position = center + (Vector3.up * Screen.height / 2.1f);
    }

	// Update is called once per frame
	void Update () {

        if (target == null)
        {
            onScreenImage.enabled = false;
            offScreenImage.enabled = false;
            return;
        } else
        {
            // there is a target
            Vector3 targetPosOnScreen = Camera.main.WorldToScreenPoint(target.position);
            
            if (Utilities.IsOnScreen(targetPosOnScreen) && Utilities.IsInFront(playerCamera.transform, target))
            {
                // target is visible within the screen area
                onScreenImage.enabled = true;
                offScreenImage.enabled = false;

                onScreenImage.transform.position = targetPosOnScreen;
                PulseCircle();
            } else
            {
                // target is offscreen
                Vector3 center = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
                
                Vector3 targetRelative = target.position - playerCamera.transform.position;
                Quaternion toTarget = Quaternion.FromToRotation(playerCamera.transform.forward, targetRelative);

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
                offScreenImage.rectTransform.Rotate(new Vector3(0f, offScreenPulseSpeed, 0f));
            }
            
        }
	}

	private void PulseCircle(){
		currentSize += Time.deltaTime * (Time.timeScale / onScreenPulseDuration);
		if (currentSize > 1.0f){
			currentSize = 0.0f;
		}
        
        onScreenImage.rectTransform.sizeDelta = Vector2.Lerp(Vector2.zero, maxSizeVector, currentSize);
    }

	public void SetTarget(Transform _target, bool altColour){
		target = _target;
		currentSize = 0.0f;

        if (altColour)
        {
            onScreenImage.color = altUiColor;
            offScreenImage.color = altUiColor;
        } else
        {
            onScreenImage.color = baseUiColor;
            offScreenImage.color = baseUiColor;
        }
	}

    // Sets the base and alt ui colours
    public void SetColours(Color _base, Color _alt)
    {
        baseUiColor = _base;
        altUiColor = _alt;
    }
}
