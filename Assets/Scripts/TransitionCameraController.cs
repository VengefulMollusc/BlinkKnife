﻿using System;
using UnityEngine;
using System.Collections;
using AssemblyCSharp;
using ProBuilder2.Common;
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

        Blackout(false);

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

    // Activates extended warp transition that covers bezier curve of fibre optic objects
    public void FibreOpticWarp(FibreOpticController _fibreOpticController)
    {
        fibreOpticController = _fibreOpticController;
        fibreOpticWarp = true;

        if (knifeController == null)
        {
            Debug.LogError("Null knifeController");
            return;
        }

        _fibreOpticController.WarpKnife(knifeController);

        duration *= 1.5f;
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
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * (Time.timeScale / duration);

            float lerpPercent = t * t * t; // modify t value to allow non-linear transitions

            transform.position = Vector3.Lerp(startPos, fibreOpticController.GetPosition(), lerpPercent);

            // tAlt transitions from 0-1-0 over warp
            float tAlt = Mathf.Abs((lerpPercent * 2) - 1);

            // use chromatic aberration durign warp
            chromAberration.chromaticAberration = Mathf.Lerp(chromaticAberrationMaxValue, chromaticAberrationMaxValue - (chromDiff * tAlt), tAlt);

            if (duration > 0.4f)
            {
                // increase FOV while long/gravity shift
                cam.fieldOfView = Mathf.Lerp(fovMaxValue, fovMaxValue - (fovDiff * tAlt), tAlt);
            }
            // lerp rotation as well
            transform.rotation = Quaternion.Lerp(startRot, fibreOpticController.GetStartRotation(), lerpPercent);

            yield return 0;
        }

        // Fibre optic warp transition here
        float fibreOpticDuration = fibreOpticController.GetDuration();
        float t2 = 0f;
        while (t2 < 1f)
        {
            t2 += Time.deltaTime * (Time.timeScale / fibreOpticDuration);

            transform.position = fibreOpticController.LerpBezierPosition(t2);

            // rotate camera to face bezier tangent and lean slightly depending on angle of turn
            // Lots of calculations. may be a bit pointless :P
            Vector3 tangent = fibreOpticController.GetBezierTangent(t2);
            Vector3 flattened = Vector3.ProjectOnPlane(tangent, transform.up).normalized;
            float angle = Vector3.Angle(transform.forward, flattened) * 10f;
            float dot = Vector3.Dot(tangent, transform.right);

            Quaternion lean = Quaternion.AngleAxis((dot < 0) ? angle : -angle, tangent);

            Quaternion newRotation = GlobalGravityControl.GetRotationToDir(tangent);
            newRotation = Quaternion.RotateTowards(newRotation, lean * newRotation, 10f);

            transform.rotation = newRotation;

            yield return 0;
        }

        // TODO: modify this to give velocity out of fibre optic warp. AND add rotation variable
        Info<Vector3, Vector3, bool, bool> info = new Info<Vector3, Vector3, bool, bool>(knifeController.GetWarpPosition(),
            knifeController.transform.up,
            knifeController.IsBounceKnife(), true);
        this.PostNotification(WarpEndNotification, info);

        Destroy(gameObject);
    }

    IEnumerator TransitionCamera()
    {
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * (Time.timeScale / duration);

            float lerpPercent = t * t * t; // modify t value to allow non-linear transitions

            transform.position = Vector3.Lerp(startPos, knifeController.GetWarpPosition() + camRelativePos,
                lerpPercent);

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

        // TODO: modify this to give velocity out of fibre optic warp. AND add rotation variable
        Info<Vector3, Vector3, bool, bool> info = new Info<Vector3, Vector3, bool, bool>(knifeController.GetWarpPosition(),
            knifeController.GetVelocity(),
            knifeController.IsBounceKnife(), false);
        this.PostNotification(WarpEndNotification, info);

        Destroy(gameObject);
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
