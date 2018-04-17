using System.Collections;
using UnityEngine;

public class JumpCollider : MonoBehaviour
{
    private GameObject player;

    private static bool colliding;

    public const string MovementObjectNotification = "JumpCollider.MovementObjectNotification";
    //private GameObject relativeMovementObject;

    //[SerializeField] private LayerMask relativeMotionLayers;

    //private PhysicMaterial playerMaterial;
    //private float staticFriction;
    //private float dynamicFriction;

    private float colExitDelay = 0.1f; 
    private Coroutine colExitCoroutine;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        colliding = false;

        //relativeMovementObject = null;

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

    public static void Jump()
    {
        colliding = false;
    }

    void OnTriggerStay(Collider col)
    {
        if (col.isTrigger)
            return;

        if (colExitCoroutine != null)
            StopCoroutine(colExitCoroutine);

        colliding = true;
        //playerMotor.SetOnGround(true);


        //GameObject colObject = col.gameObject;

        //if (colObject != relativeMovementObject && relativeMotionLayers == (relativeMotionLayers | (1 << col.gameObject.layer)))
        //{
        //    relativeMovementObject = colObject;
        //    this.PostNotification(MovementObjectNotification, relativeMovementObject);
        //}


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

        //colliding = false;

        if (colExitCoroutine != null)
            StopCoroutine(colExitCoroutine);

        colExitCoroutine = StartCoroutine(ColExitDelay());

        //GameObject colObject = col.gameObject;

        //if (colObject == relativeMovementObject)
        //{
        //    relativeMovementObject = null;
        //    this.PostNotification(MovementObjectNotification, null);
        //}


        //if (relativeMotionLayers == (relativeMotionLayers | (1 << col.gameObject.layer)))
        //{
        //    player.transform.SetParent(null);
        //}
    }

    IEnumerator ColExitDelay()
    {
        float t = 0;
        while (t < colExitDelay)
        {
            t += Time.deltaTime;
            yield return 0;
        }

        colliding = false;
    }
}
