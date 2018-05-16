using UnityEngine;

public class WarpLookAheadCollider : MonoBehaviour
{
    private Collider[] lookAheadColliders;
    private bool colliding;

    private GameObject knifeObject;
    private KnifeController knifeController;

    private SafeWarpCollider safeWarpCollider;

    private Rigidbody rb;

    private Vector3 lastUsablePos;

    private bool collidersEnabled;

    void OnEnable()
    {
        colliding = false;
        lookAheadColliders = GetComponents<Collider>();

        rb = GetComponent<Rigidbody>();

        Enabled(false);

        this.AddObserver(OnAttachKnife, KnifeController.AttachLookAheadColliderNotification);
        this.AddObserver(UpdateWarpLookAhead, SafeWarpCollider.UpdateLookAheadColliderNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnAttachKnife, KnifeController.AttachLookAheadColliderNotification);
        this.RemoveObserver(UpdateWarpLookAhead, SafeWarpCollider.UpdateLookAheadColliderNotification);
    }

    void FixedUpdate()
    {
        // keeps velocity at zero without disabling rigidbody etc
        rb.velocity = Vector3.zero;
        
        if (collidersEnabled && knifeObject == null)
        {
            Enabled(false);
        }
    }

    // called by notification from SafeWarpCollider
    void UpdateWarpLookAhead(object sender, object args)
    {
        if (!collidersEnabled)
            return;

        // If not colliding current position is safe
        if (!colliding)
            lastUsablePos = transform.position;

        colliding = false;

        // Move to safecollider position and match rotation
        Transform safeColliderTransform = (Transform)args;

        rb.MovePosition(safeColliderTransform.position); // using MovePosition here updates collisions
        transform.rotation = safeColliderTransform.rotation;

        // TODO: figure out if this is unnesessary - we are moving to this location anyway so colliding should be false next update
        if (safeWarpCollider.IsSafeToWarp())
            lastUsablePos = safeColliderTransform.position;
    }

    void OnAttachKnife(object sender, object args)
    {
        knifeController = (KnifeController)args;
        knifeObject = knifeController.gameObject;
        safeWarpCollider = knifeObject.GetComponentInChildren<SafeWarpCollider>();
        colliding = false;
        transform.position = knifeObject.transform.position;
        transform.rotation = GlobalGravityControl.GetGravityRotation();
        lastUsablePos = knifeObject.transform.position;
        Enabled(true);
    }

    // Enables and disables colliders if not needed
    public void Enabled(bool _enabled)
    {
        foreach (Collider col in lookAheadColliders)
        {
            col.enabled = _enabled;
        }
        collidersEnabled = _enabled;

        if (!collidersEnabled)
            rb.velocity = Vector3.zero;
    }

    // TODO: may just need to return lastUsable Pos
    public Vector3 WarpPosition()
    {
        if (colliding)
            return lastUsablePos;

        return transform.position;
    }

    void OnTriggerStay(Collider col)
    {
        colliding = true;
    }

    void OnTriggerExit(Collider col)
    {
        colliding = false;
    }
}
