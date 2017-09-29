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
    [SerializeField]
    private bool useDuration = true;

    [SerializeField]
    private float transDuration = 1f; // 0.2f

    [SerializeField]
    [Range(0.0f, 500.0f)]
    private float transSpeed = 200f;

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

    private int baseLayerMask;

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

        CalculateDuration();

        baseLayerMask = cam.cullingMask;
    }

    private void CalculateDuration()
    {
        float dist = Vector3.Distance(startPos, endPos);

        if (gravityShift)
        {
            // use the given duration
            float distModifier = Utilities.MapValues(dist, 0f, 100f, 0f, gravMaxDistModifier, true);
            duration = gravBaseTransDuration + distModifier;

            // modify fovMax by speed
            float speed = dist / duration;
            fovMaxValue = Utilities.MapValues(speed, 0f, fovSpeedModMax, cam.fieldOfView, fovMaxValue, true);
            fovDiff = fovMaxValue - cam.fieldOfView;
        }
        else
        {
            if (!useDuration)
            {
                // calculate duration based on distance
                duration = dist / transSpeed;
            }
            else
            {
                // use the given duration
                duration = transDuration;
            }
        }
    }

    public float GetDuration()
    {
        return duration;
    }

    // Triggers the transition animation, unsure if this is needed, could put in setup
    public void StartTransition()
    {
        //        if (startPosition == null || endPosition == null)
        //        {
        //            Debug.LogError("Transition camera missing start or end position");
        //            return;
        //        }

        playerCam.enabled = false;
        playerMotor.Freeze();

        StartCoroutine(TransitionCamera());
    }

    IEnumerator TransitionCamera()
    {
        float t = 0.0f;
        while (t < 1.0f)
        {
            // TODO: 
            // Physics.CheckSphere(transform.position, 0.1f);
            // if ^ then inside mesh and should render black
            //if (Physics.CheckSphere(transform.position, 0.1f))
            //{
            //    // disable this camera and enable 'black' camera
            //    // with: Clear flags - Solid Color
            //    // background - Black
            //    // culling mask - nothing
            //    cam.cullingMask = (1 << LayerMask.NameToLayer("Particles"));
            //    // or use image effects/distortion to 'warp' image while inside?
            //} else
            //{
            //    cam.cullingMask = baseLayerMask;
            //}

            // TODO: Need to decide lerp or slerp
            // Lerp is more accurate/less bugs
            // but slerp gives a cool curve effect to the position movement
            t += Time.deltaTime * (Time.timeScale / duration);

            float lerpPercent = t * t * t; // modify t value to allow non-linear transitions

            gameObject.transform.position = Vector3.Lerp(startPos, endPos, lerpPercent);

            if (gravityShift)
            {
                // lerp rotation as well
                gameObject.transform.rotation = Quaternion.Lerp(startRot, endRot, lerpPercent);

                float tAlt = Mathf.Abs((lerpPercent * 2) - 1);
                // increase FOV while gravity shift
                cam.fieldOfView = Mathf.Lerp(fovMaxValue, fovMaxValue - (fovDiff * tAlt), tAlt);
                // increase chromatic aberration during gravity shift
                //chromAberration.chromaticAberration = Mathf.Lerp(chromMaxValue, chromMinValue, tChrom);
                chromAberration.chromaticAberration = Mathf.Lerp(chromaticAberrationMaxValue, chromaticAberrationMaxValue - (chromDiff * tAlt), tAlt);
            }
            yield return 0;
        }

        playerCam.enabled = true;

        // wallhang if crouching
        //if (playerMotor.WallHang () && toBeParent != null) {
        //	playerMotor.transform.SetParent(toBeParent.transform);
        //} else {
        //	playerMotor.UnFreeze ();
        //}

        playerMotor.UnFreeze();

        Destroy(gameObject);
    }
}
