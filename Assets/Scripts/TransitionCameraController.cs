using System;
using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class TransitionCameraController : MonoBehaviour
{

    private Vector3 startPos;
    private Vector3 endPos;
    private Quaternion startRot;
    private Quaternion endRot;

    private bool gravityShift;

    private Camera playerCam;
    private PlayerMotor playerMotor;

    [Header("Default Warp")]
    [SerializeField] private float baseTransDuration = 1f; // 0.5f
    [SerializeField] private float maxDistModifier = 1f;

    [Header("Gravity Warp")]
    [SerializeField] private float gravBaseTransDuration = 1f; // 0.5f
    [SerializeField] private float gravMaxDistModifier = 1f;

    private float duration;

    // Image effects variables
    private VignetteAndChromaticAberration chromAberration;

    [Header("Other Variables")]
    [SerializeField]
    private float chromaticAberrationMaxValue = 30f;
    private float chromDiff;

    private Camera cam;
    [SerializeField]
    private float fovMaxValue = 120f;
    [SerializeField]
    private float fovSpeedModMax = 50f;
    private float fovDiff;

    public void Setup(Camera _playerCam, PlayerMotor _playerMotor)
    {
        playerCam = _playerCam;
        playerMotor = _playerMotor;
    }

    public void Setup(Camera _playerCam, PlayerMotor _playerMotor, Vector3 _startPos, Vector3 _endPos, Quaternion _startRot, Quaternion _endRot, bool _gravityShift)
    {
        playerCam = _playerCam;
        playerMotor = _playerMotor;
        startPos = _startPos;
        endPos = _endPos;
        startRot = _startRot;
        endRot = _endRot;
        gravityShift = _gravityShift;

        chromAberration = GetComponent<VignetteAndChromaticAberration>();
        chromDiff = chromaticAberrationMaxValue - chromAberration.chromaticAberration;

        cam = GetComponent<Camera>();
        cam.fieldOfView = playerCam.fieldOfView;

        CalculateDuration();

        //baseLayerMask = cam.cullingMask;
    }

    private void CalculateDuration()
    {
        float dist = Vector3.Distance(startPos, endPos);

        if (gravityShift)
            CalculateDuration(dist, gravBaseTransDuration, gravMaxDistModifier);
        else
            CalculateDuration(dist, baseTransDuration, maxDistModifier);
    }

    private void CalculateDuration(float _dist, float _baseDuration, float _modifier)
    {
        // use the given duration
        float distModifier = Utilities.MapValues(_dist, 0f, 100f, 0f, _modifier, true);
        duration = _baseDuration + distModifier;

        // modify fovMax by speed
        float speed = _dist / duration;
        fovMaxValue = Utilities.MapValues(speed, 0f, fovSpeedModMax, cam.fieldOfView, fovMaxValue, true);
        fovDiff = fovMaxValue - cam.fieldOfView;
    }

    public float GetDuration()
    {
        return duration;
    }

    // Triggers the transition animation, unsure if this is needed, could put in setup
    public void StartTransition()
    {
        playerCam.enabled = false;
        playerMotor.Freeze();

        StartCoroutine(TransitionCamera());
    }

    IEnumerator TransitionCamera()
    {
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * (Time.timeScale / duration);

            float lerpPercent = t * t * t; // modify t value to allow non-linear transitions

            gameObject.transform.position = Vector3.Lerp(startPos, endPos, lerpPercent);
            
            // tAlt transitions from 0-1-0 over warp
            float tAlt = Mathf.Abs((lerpPercent * 2) - 1);

            if (gravityShift || duration > 0.4f)
            {
                // increase FOV while long/gravity shift
                cam.fieldOfView = Mathf.Lerp(fovMaxValue, fovMaxValue - (fovDiff * tAlt), tAlt);
            }

            if (gravityShift)
            {
                // lerp rotation as well
                gameObject.transform.rotation = Quaternion.Lerp(startRot, endRot, lerpPercent);
                // increase chromatic aberration during gravity shift
                //chromAberration.chromaticAberration = Mathf.Lerp(chromMaxValue, chromMinValue, tChrom);
                chromAberration.chromaticAberration = Mathf.Lerp(chromaticAberrationMaxValue, chromaticAberrationMaxValue - (chromDiff * tAlt), tAlt);
            }

            yield return 0;
        }

        playerCam.enabled = true;

        playerMotor.UnFreeze();

        Destroy(gameObject);
    }
}
