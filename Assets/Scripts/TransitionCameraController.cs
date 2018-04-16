using System;
using UnityEngine;
using System.Collections;
using AssemblyCSharp;
using UnityStandardAssets.ImageEffects;

public class TransitionCameraController : MonoBehaviour
{
    public const string WarpEndNotification = "TransitionCameraController.WarpEndNotification";

    private Vector3 startPos;
    private KnifeController knifeController;
    private Vector3 camRelativePos;
    private Quaternion startRot;
    private Quaternion endRot;

    private bool gravityShift;

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

    [SerializeField]
    private Camera blackoutCamera;

    void OnEnable()
    {
        if (blackoutCamera == null)
            Debug.LogError("No BlackoutCamera assigned");
    }

    public void Setup(float _fov, Vector3 _startPos, KnifeController _knifeController, Vector3 _camRelativePos, Quaternion _startRot, Quaternion _endRot, bool _gravityShift)
    {
        startPos = _startPos;
        knifeController = _knifeController;
        startRot = _startRot;
        endRot = _endRot;
        camRelativePos = endRot * _camRelativePos;
        gravityShift = _gravityShift;

        chromAberration = GetComponent<VignetteAndChromaticAberration>();
        chromDiff = chromaticAberrationMaxValue - chromAberration.chromaticAberration;

        cam = GetComponent<Camera>();
        cam.fieldOfView = _fov;

        CalculateDuration();
    }

    private void CalculateDuration()
    {
        float dist = Vector3.Distance(startPos, knifeController.GetWarpPosition() + camRelativePos);

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
        StartCoroutine(TransitionCamera());
    }

    IEnumerator TransitionCamera()
    {
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * (Time.timeScale / duration);

            float lerpPercent = t * t * t; // modify t value to allow non-linear transitions

            transform.position = Vector3.Lerp(startPos, knifeController.GetWarpPosition() + camRelativePos, lerpPercent);
            
            // tAlt transitions from 0-1-0 over warp
            float tAlt = Mathf.Abs((lerpPercent * 2) - 1);

            // use chromatic aberration durign warp
            chromAberration.chromaticAberration = Mathf.Lerp(chromaticAberrationMaxValue, chromaticAberrationMaxValue - (chromDiff * tAlt), tAlt);

            if (gravityShift || duration > 0.4f)
            {
                // increase FOV while long/gravity shift
                cam.fieldOfView = Mathf.Lerp(fovMaxValue, fovMaxValue - (fovDiff * tAlt), tAlt);
            }

            if (gravityShift)
            {
                // lerp rotation as well
                transform.rotation = Quaternion.Lerp(startRot, endRot, lerpPercent);
                // increase chromatic aberration during gravity shift
                //chromAberration.chromaticAberration = Mathf.Lerp(chromaticAberrationMaxValue, chromaticAberrationMaxValue - (chromDiff * tAlt), tAlt);
            }

            yield return 0;
        }

        Info<Vector3, Vector3, bool> info = new Info<Vector3, Vector3, bool>(knifeController.GetWarpPosition(), knifeController.GetVelocity(), knifeController.IsBounceKnife());
        this.PostNotification(WarpEndNotification, info);

        Destroy(gameObject);
    }

    void Blackout(bool blackout)
    {
        cam.enabled = !blackout;
        blackoutCamera.enabled = blackout;
    }

    void OnTriggerStay(Collider col)
    {
        if (col.isTrigger)
            return;

        Blackout(true);
    }

    void OnTriggerExit(Collider col)
    {
        if (col.isTrigger)
            return;

        Blackout(false);
    }
}
