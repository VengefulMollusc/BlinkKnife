﻿using UnityEngine;
using System.Collections;
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
    [SerializeField]
    private float baseTransDuration = 1f; // 0.5f
    [SerializeField] private float maxDistModifier = 1f;

    [Header("Gravity Warp")]
    [SerializeField]
    private float gravBaseTransDuration = 1f; // 0.5f
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

    [SerializeField]
    private LayerMask raycastLayermask;

    private FibreOpticController fibreOpticController;
    private bool fibreOpticWarp;

    private Rigidbody rb;
    

    void Awake()
    {
        if (blackoutCamera == null)
            Debug.LogError("No BlackoutCamera assigned");

        cam = GetComponent<Camera>();
        //cam.enabled = false;
        //blackoutCamera.enabled = false;
        chromAberration = GetComponent<VignetteAndChromaticAberration>();

        rb = GetComponent<Rigidbody>();
        //rb.detectCollisions = false;

        Disable();
    }

    public void Setup(float _fov, Vector3 _startPos, KnifeController _knifeController, Vector3 _camRelativePos, Quaternion _startRot, Quaternion _endRot, bool _gravityShift, FibreOpticController _fibreController)
    {
        startPos = _startPos;
        startRot = _startRot;
        transform.position = startPos;
        transform.rotation = startRot;

        knifeController = _knifeController;
        endRot = _endRot;
        camRelativePos = endRot * _camRelativePos;
        gravityShift = _gravityShift;

        chromDiff = chromaticAberrationMaxValue - chromAberration.chromaticAberration;
        
        cam.fieldOfView = _fov;
        rb.detectCollisions = true;
        Blackout(false);

        if (_fibreController)
        {
            fibreOpticController = _fibreController;
            fibreOpticWarp = true;

            fibreOpticController.WarpKnife(knifeController);

            float speed = fibreOpticController.GetLengthEstimate() / fibreOpticController.GetDuration();
            fovMaxValue = Utilities.MapValues(speed, 0f, fovSpeedModMax, cam.fieldOfView, fovMaxValue, true) * 1.2f;
            fovDiff = fovMaxValue - cam.fieldOfView;
        }

        CalculateDuration();
    }

    private void CalculateDuration()
    {
        float dist = Vector3.Distance(startPos, knifeController.GetWarpPosition() + camRelativePos);

        if (gravityShift)
            CalculateDuration(dist, gravBaseTransDuration, gravMaxDistModifier);
        else
            CalculateDuration(dist, baseTransDuration, maxDistModifier);

        if (fibreOpticWarp)
            duration *= 1.5f;
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
        if (fibreOpticWarp)
            StartCoroutine(FibreTransitionCamera());
        else
            StartCoroutine(TransitionCamera());
    }

    IEnumerator FibreTransitionCamera()
    {
        float fibreOpticDuration = fibreOpticController.GetDuration();

        float totalDuration = duration + fibreOpticDuration;
        bool transitionFov = totalDuration > 0.5f;

        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * (Time.timeScale / duration);
            t = Mathf.Clamp01(t);

            float lerpPercent = t * t * t; // modify t value to allow non-linear transitions

            // lerp position and rotation
            transform.position = Vector3.Lerp(startPos, fibreOpticController.GetPosition(), lerpPercent);
            transform.rotation = Quaternion.Lerp(startRot, fibreOpticController.GetStartRotation(), lerpPercent);

            // Warp camera effects
            float totalT = (t * duration) / totalDuration;
            WarpCameraEffects(totalT, transitionFov);

            yield return 0;
        }

        // Fibre optic warp transition here
        float t2 = 0f;
        while (t2 < 1f)
        {
            t2 += Time.deltaTime * (Time.timeScale / fibreOpticDuration);
            t2 = Mathf.Clamp01(t2);

            transform.position = fibreOpticController.GetBezierPosition(t2);

            Vector3 tangent = fibreOpticController.GetBezierTangent(t2);
            Quaternion newRotation = GlobalGravityControl.GetRotationToDir(tangent);

            // rotate camera to face bezier tangent and lean slightly depending on angle of turn
            // Lots of calculations. may be a bit much for such a subtle effect :P
            float tAlt = Mathf.Abs((2f * t2) - 1f);
            tAlt = 1f - (tAlt * tAlt);
            Vector3 flattened = Vector3.ProjectOnPlane(tangent, transform.up).normalized;
            float angle = Vector3.Angle(transform.forward, flattened) * 10f * tAlt;
            float dot = Vector3.Dot(tangent, transform.right);

            Quaternion lean = Quaternion.AngleAxis((dot < 0) ? angle : -angle, tangent);
            newRotation = Quaternion.RotateTowards(newRotation, lean * newRotation, 10f);

            transform.rotation = newRotation;

            // Warp camera effects
            float totalT = (duration + (t2 * fibreOpticDuration)) / totalDuration;
            WarpCameraEffects(totalT, transitionFov);

            yield return 0;
        }
        
        Info<Vector3, Vector3, bool, FibreOpticController> info = new Info<Vector3, Vector3, bool, FibreOpticController>(knifeController.GetWarpPosition(),
            knifeController.transform.up,
            knifeController.IsBounceKnife(), fibreOpticController);
        this.PostNotification(WarpEndNotification, info);

        //Destroy(gameObject);
        Disable();
    }

    IEnumerator TransitionCamera()
    {
        bool transitionFov = gravityShift || duration > 0.4f;

        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * (Time.timeScale / duration);

            float lerpPercent = t * t * t; // modify t value to allow non-linear transitions

            transform.position = Vector3.Lerp(startPos, knifeController.GetWarpPosition() + camRelativePos,
                lerpPercent);

            // camera effects
            WarpCameraEffects(lerpPercent, transitionFov);

            if (gravityShift)
            {
                // lerp rotation as well
                transform.rotation = Quaternion.Lerp(startRot, endRot, lerpPercent);
            }

            yield return 0;
        }
        
        Info<Vector3, Vector3, bool, FibreOpticController> info = new Info<Vector3, Vector3, bool, FibreOpticController>(knifeController.GetWarpPosition(),
            knifeController.GetVelocity(),
            knifeController.IsBounceKnife(), null);
        this.PostNotification(WarpEndNotification, info);

        //Destroy(gameObject);
        Disable();
    }

    private void Disable()
    {
        blackoutCamera.enabled = false;
        cam.enabled = false;
        rb.detectCollisions = false;
    }

    /*
     * Activates camera effects based on warp progression
     */
    private void WarpCameraEffects(float _t, bool _fov)
    {
        float lerpPercent = Mathf.Abs((2f * _t) - 1f);
        // use chromatic aberration durign warp
        chromAberration.chromaticAberration = Mathf.Lerp(chromaticAberrationMaxValue, chromaticAberrationMaxValue - (chromDiff * lerpPercent), lerpPercent);

        if (_fov)
            // increase FOV
            cam.fieldOfView = Mathf.Lerp(fovMaxValue, fovMaxValue - (fovDiff * lerpPercent), lerpPercent);
    }

    // Performs a raycast check from the start position (guaranteed to be outside mesh)
    // to the current one to activate blackout camera accordingly
    void BlackoutCheckFromOrigin()
    {
        float dist = Vector3.Distance(startPos, transform.position);
        Vector3 dir = (transform.position - startPos).normalized;
        int hitCount = 0;

        RaycastHit[] hits = Physics.RaycastAll(startPos, dir, dist, raycastLayermask,
            QueryTriggerInteraction.Ignore);

        hitCount += hits.Length;

        hits = Physics.RaycastAll(transform.position, -dir, dist, raycastLayermask,
            QueryTriggerInteraction.Ignore);

        hitCount += hits.Length;

        // if odd number of hits, is inside a mesh
        Blackout(hitCount % 2 == 1);
    }

    void Blackout(bool blackout)
    {
        cam.enabled = !blackout;
        blackoutCamera.enabled = blackout;
    }

    // These alone are unreliable with large meshes (possibly something to do with Probuilder)
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
        
        BlackoutCheckFromOrigin();
    }
}
