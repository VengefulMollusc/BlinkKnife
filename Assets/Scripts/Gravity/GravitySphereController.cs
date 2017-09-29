using UnityEngine;
using System.Collections;

public class GravitySphereController : MonoBehaviour {

    // set to true to link gravity permanently or on start if combined with linkGravUntilGravWarp
    [SerializeField]
    private bool linkGrav = false;

    // if true, gravity will remain linked until a gravity warp is performed
    // false will break link on any warp (or anything that breaks object parenting)
    [SerializeField]
    private bool linkGravUntilGravWarp = false;

    // if true, gravity will push AWAY from center of sphere while linked.
    // used mainly for gravity inside spheres
    [SerializeField]
    private bool invertGravDirection = false;

    private Transform target;

    private Vector3 currentUp;

    /*
     * When parented to sphere, lets player modify gravity by walking along surface
     * 
     * extend to rings/cylinders?
     * (Need separate logic/script for that)
     * (uses player normal instead of direction to center of mesh)
     * 
     * clock/time puzzle, walk around to increase/decrease time
     * 
     */
    void Start() {
        
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void OnEnable()
    {
        this.AddObserver(OnGravWarp, PlayerMotor.GravityWarpNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnGravWarp, PlayerMotor.GravityWarpNotification);
    }

    // Handles GravWarpNotification from PlayerMotor
    void OnGravWarp(object sender, object args)
    {
        GameObject warpObject = (GameObject) args;

        if (warpObject == null)
        {
            Debug.LogError("Object passed in GravityWarpNotification not GameObject");
            return;
        }

        if (!linkGravUntilGravWarp)
            return;
        
        // link gravity if warped object is this object
        LinkGravity(warpObject == gameObject);
    }
	
	/*
     * Player must be last child
     */
	void Update () {
        if (linkGrav || target.IsChildOf(transform))
        {
            currentUp = GlobalGravityControl.GetGravityTarget();
            Vector3 newUp = (target.position - transform.position).normalized;
            if (currentUp != newUp)
            {
                if (invertGravDirection)
                {
                    GlobalGravityControl.ChangeGravity(-newUp, true);
                }
                else
                {
                    GlobalGravityControl.ChangeGravity(newUp, true);
                }
            }
        }
	}

    // sets whether gravity should be linked
    public void LinkGravity(bool _linkGrav)
    {
        linkGrav = _linkGrav;
    }

    // returns the distance to the player from the center of the sphere
    // could be used for variable gravity strength?
    // combine with multiple linked gravity sources with strength based on distance
    //public float DistToPlayer()
    //{
    //    return Vector3.Distance(target.position, transform.position);
    //}
}
