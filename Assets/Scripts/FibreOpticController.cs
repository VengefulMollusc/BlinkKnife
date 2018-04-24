using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEditor;
using UnityEngine;

public class FibreOpticController : MonoBehaviour
{
    [SerializeField] private FibreOpticController otherEndFibreOpticController;
    public Vector3 bezierTargetPosition;

    private KnifeController attachedKnife;

    [SerializeField]
    private GameObject bezierMeshPrefab;

    [SerializeField] private int meshSegmentCount = 10;

    // Use this for initialization
    void OnEnable () {
        if (IsConnected() && !otherEndFibreOpticController.IsConnected())
            otherEndFibreOpticController.SetOtherEndController(this);

        this.AddObserver(OnFibreOpticWarp, KnifeController.FibreOpticWarpNotification);
        this.AddObserver(OnWarpEnd, TransitionCameraController.WarpEndNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnFibreOpticWarp, KnifeController.FibreOpticWarpNotification);
        this.RemoveObserver(OnWarpEnd, TransitionCameraController.WarpEndNotification);
    }

    /*
     * Handles start of fibre optic warp and triggers animation transition of knife along bezier
     */
    void OnFibreOpticWarp(object sender, object args)
    {
        FibreOpticController fibreOpticController = (FibreOpticController)args;
        if (fibreOpticController != this)
            return;

        if (!IsConnected())
        {
            Debug.LogError("Not connected to other FibreOpticController");
            return;
        }

        //Debug.Log("Fibre Optic warp effect here");


        //Info<GameObject, KnifeController> info = (Info<GameObject, KnifeController>)args;
        //KnifeController knifeController = info.arg1;

        // stick knife into other end. Should allow lookahead colliders to do their thing
        //knifeController.StickToSurface(otherEndFibreOpticController.transform.position, -otherEndFibreOpticController.transform.forward, otherEndFibreOpticController.gameObject, true);


        //StartCoroutine(TransitionKnife(knifeTransform));
    }

    void OnWarpEnd(object sender, object args)
    {
        if (attachedKnife)
            attachedKnife = null;
    }

    public void WarpKnife(KnifeController _knifeController)
    {
        if (!IsConnected())
        {
            Debug.LogError("Not connected to other FibreOpticController");
            return;
        }

        otherEndFibreOpticController.AttachKnife(_knifeController);
    }

    // Transitions the knife along the bezier
    //private IEnumerator TransitionKnife(Transform _knifeTransform)
    //{
    //    float t = 1f;
    //    while (t > 0)
    //    {
    //        t -= Time.deltaTime * (Time.timeScale / transitionTime);
    //        _knifeTransform.position = GetBezierPosition(t);

    //        yield return 0;
    //    }
    //}

    public void AttachKnife(KnifeController _knifeController)
    {
        attachedKnife = _knifeController;

        // stick knife into this end. Should allow lookahead colliders to do their thing
        // places knife out from surface to give lookahead more room
        Vector3 normalVector = GetDirection();
        Vector3 knifePosition = transform.position + normalVector; // gravity offset to align camera
        _knifeController.StickToSurface(knifePosition, normalVector, gameObject, true);
    }

    /*
     * Lerps along the bezier defined by both FibreOpticControllers and the bezier targets
     * 
     * allows bezier control point movement during transition
     */
    public Vector3 GetBezierPosition(float _t)
    {
        return Utilities.LerpBezier(GetPosition(), 
            GetBezierTargetPosition(),
            otherEndFibreOpticController.GetBezierTargetPosition(), 
            otherEndFibreOpticController.GetPosition(),
            _t);
    }

    /*
     * Handles creation of fibre-optic cable mesh following bezier curve.
     * Only run through inspector button.
     * 
     * I think running this every frame/fixedupdate might be a bit much, 
     * so no moving fibre-optics for the moment
     */
    public void CreateBezierMesh()
    {
        if (bezierMeshPrefab == null)
        {
            Debug.LogError("No BezierMeshPrefab given");
            return;
        }

        Debug.Log("Creating Bezier Mesh...");
        GameObject meshObject = Instantiate(bezierMeshPrefab, transform.parent);
        meshObject.transform.position = Vector3.zero;
        meshObject.transform.rotation = Quaternion.identity;
        meshObject.GetComponent<MeshFilter>().mesh = FibreOpticMeshCreator.CreateMeshForBezier(GetBezierPoints(), meshSegmentCount);
    }

    // returns mesh segment count - used by inspector script to create wireframe
    public int GetSegmentCount()
    {
        return meshSegmentCount;
    }

    //public Quaternion GetInitialRotation()
    //{
    //    Vector3 forwardRelToGravity =
    //        Vector3.ProjectOnPlane(transform.forward, GlobalGravityControl.GetCurrentGravityVector());

    //    return Quaternion.LookRotation(forwardRelToGravity.normalized, -GlobalGravityControl.GetCurrentGravityVector());
    //}

    // used so player can warp at the right speed
    public float GetDuration()  // TODO: replace with exponential increase
    {
        return Mathf.Max(GetLengthEstimate() * 0.008f, 1f); // was 1f
    }

    public float GetLengthEstimate()
    {
        return Utilities.BezierLengthEstimate(GetBezierPoints());
    }

    public Vector3 GetBezierTangent(float _t)
    {
        return Utilities.BezierDerivative(GetBezierPoints(), _t);
    }

    // Used so that other end can get target position for bezier calculation
    public Vector3 GetBezierTargetPosition()
    {
        return transform.position + (transform.rotation * bezierTargetPosition);
    }

    public Vector3 GetPosition()
    {
        //return transform.position;
        return (attachedKnife) ? attachedKnife.GetPosition() : transform.position;
    }

    // Used to make sure that both ends are connected properly
    public void SetOtherEndController(FibreOpticController _other)
    {
        otherEndFibreOpticController = _other;
    }

    //public Vector3 GetEndPosition()
    //{
    //    return otherEndFibreOpticController.transform.position;
    //}

    public Vector3 GetDirection()
    {
        return (transform.rotation * -bezierTargetPosition).normalized;
    }

    public Vector3 GetExitDirection()
    {
        return otherEndFibreOpticController.GetDirection();
    }

    // Get rotations for aligning transition camera
    public Quaternion GetStartRotation()
    {
        return GlobalGravityControl.GetRotationToDir(-GetDirection());
    }
    
    public Quaternion GetEndRotation()
    {
        //return Quaternion.AngleAxis(180, otherEndFibreOpticController.transform.up) * otherEndFibreOpticController.transform.rotation;
        return GlobalGravityControl.GetRotationToDir(otherEndFibreOpticController.GetDirection());
    }

    // returns true if this controller has a reference to the controller at the other end
    public bool IsConnected()
    {
        return otherEndFibreOpticController != null;
    }


    // TODO: Currently only a debug method used for drawing in editor
    public Info<Vector3, Vector3, Vector3, Vector3> GetBezierPoints()
    {
        return (otherEndFibreOpticController != null) ? new Info<Vector3, Vector3, Vector3, Vector3>(transform.position,
            GetBezierTargetPosition(),
            otherEndFibreOpticController.GetBezierTargetPosition(), 
            otherEndFibreOpticController.GetPosition()) : null;
    }
}
