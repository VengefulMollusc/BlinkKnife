using UnityEngine;
using System.Collections;

public class GravitySphereController : MonoBehaviour {

    //private int baseChildCount;

    [SerializeField]
    private bool linkGrav = false;

    [SerializeField]
    private bool linkGravUntilGravWarp = false;

    [SerializeField] private bool invertGravDirection = false;

    [SerializeField]
    private GameObject player;

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
     * need option to allow inverse gravity - for inside sphere
     * 
     */
    void Start() {
        //baseChildCount = transform.childCount;

        player = GameObject.FindGameObjectWithTag("Player");
        target = player.transform;
    }

    void OnEnable()
    {
        this.AddObserver(OnGravWarp, PlayerMotor.GravityWarpNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnGravWarp, PlayerMotor.GravityWarpNotification);
    }

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
        //if (transform.childCount > baseChildCount && !linkGravPermanently)
        //{
        //    if (target == null)
        //    {
        //        target = transform.GetChild(transform.childCount - 1);
        //        if (!target.CompareTag("Player") || target.GetComponent<Renderer>().enabled == false)
        //        {
        //            // second 'if' statement check makes sure player warp transition is complete before turning
        //            target = null;
        //            return;
        //        }
        //    }
        //}

        //if (target != null)
        //{
        //    currentUp = GlobalGravityControl.GetGravityTarget();
        //    Vector3 newUp = (target.position - transform.position).normalized;
        //    if (currentUp != newUp)
        //    {
        //        GlobalGravityControl.ChangeGravity(newUp, true);
        //    }
        //}


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

    public void LinkGravity(bool _linkGrav)
    {
        linkGrav = _linkGrav;
    }

    public float DistToPlayer()
    {
        return Vector3.Distance(player.transform.position, transform.position);
    }
}
