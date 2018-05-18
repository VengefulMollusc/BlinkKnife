using UnityEngine;

[RequireComponent(typeof(SoftSurface))]
public class FibreOpticController : MonoBehaviour
{
    [SerializeField] private FibreOpticController otherEndFibreOpticController;
    public Vector3 bezierTargetPosition;

    private KnifeController attachedKnife;

    [SerializeField]
    private GameObject bezierMeshPrefab;

    void Start()
    {
        if (IsConnected() && !otherEndFibreOpticController.IsConnected())
            otherEndFibreOpticController.SetOtherEndController(this);
    }

    // Use this for initialization
    void OnEnable()
    {
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
    }

    // detach knife on warp end
    void OnWarpEnd(object sender, object args)
    {
        if (attachedKnife)
            attachedKnife = null;
    }

    // attaches knife to other end of fibreoptic
    public void WarpKnife(KnifeController _knifeController)
    {
        if (!IsConnected())
        {
            Debug.LogError("Not connected to other FibreOpticController");
            return;
        }

        otherEndFibreOpticController.AttachKnife(_knifeController);
    }

    /*
     * Sticks knife to this fibreoptic end.
     * Positions to allow easy warp transition
     */
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
     * Lerps along the bezier defined by both FibreOpticControllers and the bezier targets.
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
        Mesh fibreMesh = FibreOpticMeshCreator.CreateMeshForBezier(GetBezierPoints());
        meshObject.GetComponent<MeshFilter>().mesh = fibreMesh;
        meshObject.GetComponent<MeshCollider>().sharedMesh = fibreMesh;
        Debug.Log("Done Creating Mesh");
    }

    // returns warp duration based on estimated length of bezier
    public float GetDuration()
    {
        return Mathf.Max(GetLengthEstimate() * 0.008f, 1f);
    }

    public float GetLengthEstimate()
    {
        return Utilities.BezierLengthEstimate(GetBezierPoints());
    }

    // returns tangent/derivative of bezier at the given point
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
        return (attachedKnife) ? attachedKnife.GetPosition() : transform.position;
    }

    // Used to make sure that both ends are connected properly
    public void SetOtherEndController(FibreOpticController _other)
    {
        otherEndFibreOpticController = _other;
    }

    // returns the 'out' direction of this bezier end
    public Vector3 GetDirection()
    {
        return (transform.rotation * -bezierTargetPosition).normalized;
    }

    // returns the 'out' direction of the other bezier end
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
