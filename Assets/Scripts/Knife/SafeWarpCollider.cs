using UnityEngine;

public class SafeWarpCollider : MonoBehaviour
{
    [SerializeField]
    private LayerMask collisionOffsetLayerMask;

    private KnifeController knifeController;

    private Collider collider;

    private bool safeToWarp;

    private Vector3 lastKnifePosition;

    private bool fibreOpticWarp;

    private Vector3 currentGravVector;
    private Quaternion currentGravRotation;

    public const string UpdateLookAheadColliderNotification = "SafeWarpCollider.UpdateLookAheadColliderNotification";

    void Start()
    {
        knifeController = transform.parent.GetComponent<KnifeController>();
        collider = GetComponent<Collider>();
        safeToWarp = true;
        lastKnifePosition = knifeController.GetPosition();
    }

    // Use this for initialization
    void OnEnable ()
	{
        this.AddObserver(OnFibreOpticWarp, KnifeController.FibreOpticWarpNotification);
        OnGravityChangeNotification(null, null);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnFibreOpticWarp, KnifeController.FibreOpticWarpNotification);
    }

    void FixedUpdate ()
	{
	    if (knifeController.GetPosition() == lastKnifePosition)
        {
            this.PostNotification(UpdateLookAheadColliderNotification, transform);
            return;
        }

	    UpdatePosition();
	}

    private void UpdatePosition()
    {
        lastKnifePosition = knifeController.GetPosition();

        // reset to knife position and gravity rotation
        if (transform.position != lastKnifePosition)
            transform.position = lastKnifePosition;

        // if knife has stuck, get position offset from wall
        if (knifeController.HasStuck())
        {
            // Check if gravity warp
            if (knifeController.ShiftGravity() && !fibreOpticWarp)
            {
                if (transform.up != -knifeController.GetGravVector())
                {
                    transform.rotation = Quaternion.FromToRotation(Vector3.down, knifeController.GetGravVector());
                }
            }
            else if (transform.up != -currentGravVector)
            {
                transform.rotation = currentGravRotation;
            }

            Vector3 newPosition = CollisionOffset();
            newPosition += SurfaceRaycastOffset(newPosition);
            transform.position = newPosition;
        }
        else if (!safeToWarp)
        {
            transform.position = lastKnifePosition + OmniRaycastOffset(knifeController.transform.position);
        }

        this.PostNotification(UpdateLookAheadColliderNotification, transform);
    }

    // Offsets collider position when knife has stuck to a surface
    // Places collider so that it is touching the point where the knife has collided
    private Vector3 CollisionOffset()
    {
        Vector3 collisionNormal = knifeController.GetCollisionNormal();

        if (collisionNormal == Vector3.zero)
        {
            return transform.position;
        }

        Vector3 position = transform.position;
        Vector3 up = transform.up;
        Vector3 closestPointBase = position;
        float dot = Vector3.Dot(up, collisionNormal);

        // offset base position to use upper/lower hemispheres of collider
        if (dot >= -0.001) // was > 0.001
            closestPointBase -= (up * 0.5f);
        else if (dot < -0.001)
            closestPointBase += (up * 0.5f);

        Vector3 closestPointOnCollider = collider.ClosestPoint(closestPointBase - collisionNormal);

        Vector3 pointDiff = closestPointOnCollider - position;

        return knifeController.GetPosition() - pointDiff;
    }

    // raycasts in 4 cardinal directions parallel to the surface the knife collided with.
    // shifts the collider along the surface according to the raycast result
    private Vector3 SurfaceRaycastOffset(Vector3 basePosition)
    {
        Vector3 position = transform.position;
        Vector3 collisionNormal = knifeController.GetCollisionNormal();

        if (collisionNormal == Vector3.zero)
            return Vector3.zero;

        Vector3 offset = Vector3.zero;

        // Check directions at right angles from collision normal
        Quaternion rotateToNormal = Quaternion.FromToRotation(Vector3.up, collisionNormal);
        Vector3 forward = rotateToNormal * Vector3.forward;
        Vector3 right = rotateToNormal * Vector3.right;
        
        float forwardDist = Vector3.Distance(position, collider.ClosestPoint(position + forward));
        float rightDist = Vector3.Distance(position, collider.ClosestPoint(position + right));

        RaycastHit hitInfo;

        if (Physics.Raycast(basePosition, forward, out hitInfo, forwardDist))
            offset -= forward * (forwardDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, -forward, out hitInfo, forwardDist))
            offset += forward * (forwardDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, right, out hitInfo, rightDist))
            offset -= right * (rightDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, -right, out hitInfo, rightDist))
            offset += right * (rightDist - hitInfo.distance);

        return offset;
    }

    // raycasts in 6 cardinal directions while the knife is airborne
    // shifts the collider to avoid obstacles
    private Vector3 OmniRaycastOffset(Vector3 basePosition)
    {
        Vector3 offset = Vector3.zero;

        // Check distance at cardinal directions
        Vector3 up = transform.up;
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        
        float verDist = 1f;
        float horDist = 0.5f;

        RaycastHit hitInfo;

        // Up/Down raycasts
        if (Physics.Raycast(basePosition, up, out hitInfo, verDist, collisionOffsetLayerMask))
            offset -= up * (verDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, -up, out hitInfo, verDist, collisionOffsetLayerMask))
            offset += up * (verDist - hitInfo.distance);


        // forward/back raycasts
        if (Physics.Raycast(basePosition, forward, out hitInfo, horDist, collisionOffsetLayerMask))
            offset -= forward * (horDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, -forward, out hitInfo, horDist, collisionOffsetLayerMask))
            offset += forward * (horDist - hitInfo.distance);


        // right/left raycasts
        if (Physics.Raycast(basePosition, right, out hitInfo, horDist, collisionOffsetLayerMask))
            offset -= right * (horDist - hitInfo.distance);

        if (Physics.Raycast(basePosition, -right, out hitInfo, horDist, collisionOffsetLayerMask))
            offset += right * (horDist - hitInfo.distance);

        return offset;
    }

    private void OnFibreOpticWarp(object sender, object args)
    {
        fibreOpticWarp = true;
    }

    private void OnGravityChangeNotification(object sender, object args)
    {
        currentGravVector = GlobalGravityControl.GetCurrentGravityVector();
        currentGravRotation = GlobalGravityControl.GetGravityRotation();
    }

    public bool IsSafeToWarp()
    {
        return safeToWarp;
    }

    void OnTriggerStay(Collider col)
    {
        if (col.isTrigger)
            return;

        safeToWarp = false;
    }

    void OnTriggerExit(Collider col)
    {
        if (col.isTrigger)
            return;

        safeToWarp = true;
    }
}
