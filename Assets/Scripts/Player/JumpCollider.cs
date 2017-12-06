using UnityEngine;

public class JumpCollider : MonoBehaviour
{

    private GameObject player;

    private static bool colliding;

    public const string RelativeMovementNotification = "JumpCollider.RelativeMovementNotification";
    private GameObject relativeMovementObject;

    [SerializeField] private LayerMask relativeMotionLayers;

    //private PhysicMaterial playerMaterial;
    //private float staticFriction;
    //private float dynamicFriction;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        colliding = false;

        relativeMovementObject = null;

        Utilities.IgnoreCollisions(GetComponent<Collider>(), player.GetComponents<Collider>(), true);

        //playerMaterial = player.GetComponent<Collider>().material;
        //staticFriction = playerMaterial.staticFriction;
        //dynamicFriction = playerMaterial.dynamicFriction;
    }

    //void UpdateFriction()
    //{
        
    //}

    public static bool IsColliding()
    {
        return colliding;
    }

    void OnTriggerStay(Collider col)
    {
        if (col.isTrigger)
            return;

        colliding = true;
        //playerMotor.SetOnGround(true);


        GameObject colObject = col.gameObject;

        if (colObject != relativeMovementObject && relativeMotionLayers == (relativeMotionLayers | (1 << col.gameObject.layer)))
        {
            relativeMovementObject = colObject;
            this.PostNotification(RelativeMovementNotification, relativeMovementObject.transform);
        }


        // TODO: replace parenting code with relative movement while colliding
        // This will need to be changed to accomodate not all things being tagged scenery
        // possibly use layers?
        //if (col.CompareTag("Scenery") || col.CompareTag("GravityPanel"))
        //{
        //    playerMotor.transform.SetParent(col.transform);
        //}

        // layers method
        //if (relativeMotionLayers == (relativeMotionLayers | (1 << col.gameObject.layer)))
        //{
        //    player.transform.SetParent(col.transform);
        //}
    }

    void OnTriggerExit(Collider col)
    {
        if (col.isTrigger)
            return;

        colliding = false;

        GameObject colObject = col.gameObject;

        if (colObject == relativeMovementObject)
        {
            relativeMovementObject = null;
            this.PostNotification(RelativeMovementNotification, null);
        }
            

        //if (relativeMotionLayers == (relativeMotionLayers | (1 << col.gameObject.layer)))
        //{
        //    player.transform.SetParent(null);
        //}
    }

    //private void OnTriggerEnter(Collider col)
    //{
    //    // added to prevent errors with DontGoThroughThings
    //}

}
