using UnityEngine;

public class JumpCollider : MonoBehaviour
{

    private GameObject player;
    private PlayerMotor playerMotor;

    //private PhysicMaterial playerMaterial;
    //private float staticFriction;
    //private float dynamicFriction;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerMotor = player.GetComponent<PlayerMotor>();

        Utilities.IgnoreCollisions(GetComponent<Collider>(), player.GetComponents<Collider>(), true);

        //playerMaterial = player.GetComponent<Collider>().material;
        //staticFriction = playerMaterial.staticFriction;
        //dynamicFriction = playerMaterial.dynamicFriction;
    }

    //void UpdateFriction()
    //{
        
    //}

    void OnTriggerStay(Collider col)
    {
        playerMotor.SetOnGround(true);

        // TODO: replace parenting code with relative movement while colliding
        // This will need to be changed to accomodate not all things being tagged scenery
        // possibly use layers?
        //if (col.CompareTag("Scenery") || col.CompareTag("GravityPanel"))
        //{
        //    playerMotor.transform.SetParent(col.transform);
        //}
    }

    void OnTriggerExit(Collider col)
    {
        //if (col.CompareTag("Scenery"))
        //{
        //    playerMotor.transform.SetParent(null);
        //}
    }

    private void OnTriggerEnter(Collider col)
    {
        // added to prevent errors with DontGoThroughThings
    }

}
