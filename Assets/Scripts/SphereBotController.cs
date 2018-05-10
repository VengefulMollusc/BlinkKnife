using UnityEngine;
using System.Collections;

public class SphereBotController : MonoBehaviour {

    [SerializeField]
    private GameObject mainSphere;
    private Collider[] sphereColliders;

    [SerializeField]
    private float aimLockAngle = 45f;
    [SerializeField]
    private float turningSpeed = 10f;

    [Header("Side Panel Settings")]
    [SerializeField]
    private GameObject leftPanel;
    [SerializeField]
    private GameObject rightPanel;
    [SerializeField]
    private float panelOpenAngle = 30f;
    [SerializeField]
    private float panelOpenDuration = 1f;

    /*
     * missilePositions length must be correct, 
     * right, left, right, left    
     */

    /*
    * USE ONE FIELD FOR MISSILEPOSITION PARENT OBJECTS
    * CREATE ARRAY FROM TRANSFORMS OF CHILDREN
    * 
    * ONE PARENT FOR EACH SIDE   
    */
    [Header("Weapon Settings")]
    [SerializeField]
    private Transform leftMissilePosParent;
    private Transform[] leftMissilePositions;
    [SerializeField]
    private Transform rightMissilePosParent;
    private Transform[] rightMissilePositions;
    [SerializeField]
    private GameObject missilePrefab;
    [SerializeField]
    private float launchTimer = 0.05f;
	[SerializeField]
	private int refireCount = 0;

    private MissileController[] missiles;
    private int missileCount;

	private Transform target;

    private GameObject player;

    private LineRenderer lineRen;

    private bool inTransition = false;
    private bool isOpen = false;
	private bool loaded = false;
	private bool firing = false;

    void Start () {
        if (mainSphere == null)
        {
            Debug.LogError("mainSphere missing in SphereBot");
        }
        if (leftPanel == null || rightPanel == null)
        {
            Debug.LogError("Panel object(s) missing in SphereBot");
        }
		if (leftMissilePosParent == null || rightMissilePosParent == null)
        {
            Debug.LogError("missile pos parent objects missing incorrect length in SphereBot");
        }

        sphereColliders = mainSphere.GetComponents<Collider>();
        lineRen = GetComponent<LineRenderer>();

        // left and right must have same length
        missileCount = leftMissilePosParent.childCount;
        leftMissilePositions = new Transform[missileCount];
		rightMissilePositions = new Transform[missileCount];
		for (int i = 0; i < missileCount; i++)
        {
            leftMissilePositions[i] = leftMissilePosParent.GetChild(i);
			rightMissilePositions[i] = rightMissilePosParent.GetChild(i);
        }

        player = GameObject.FindGameObjectWithTag("Player");
    }
	
	//void FixedUpdate () {
 //       transform.LookAt(player.transform.position);
	//}

    private void Update()
    {
        if (Input.GetButtonDown("Fire3") && !inTransition)
        {
			StartCoroutine(FiringSequence(player.transform));
        }

        if (FollowPlayer())
        {
            DrawLaser();
        } else
        {
            lineRen.enabled = false;
        }
        
    }

    private bool FollowPlayer()
    {
        Vector3 relative = player.transform.position - transform.position;
        if (Vector3.Angle(transform.forward, relative) < aimLockAngle)
        {
            //transform.LookAt(player.transform.position);
            //Quaternion.RotateTowards();
            Vector3 newForward = Vector3.RotateTowards(transform.forward, relative, turningSpeed * Time.deltaTime, 0f);
            transform.LookAt(transform.position + newForward);
            return true;
        }
        return false;
    }

    private void DrawLaser()
    {
        lineRen.enabled = true;
        lineRen.SetPosition(0, transform.position);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))
        {
            // Locks hit position to center of player when hit - stops line judder but causes jump when leaving player
            //if (hit.rigidbody != null && hit.rigidbody.gameObject == player)
            //{
            //    lineRen.SetPosition(1, player.transform.position);
            //    return;
            //} 
            lineRen.SetPosition(1, hit.point);
        }
    }

//    private void Load(Transform _target)
//    {
//        if (isOpen) return;
//
//		target = _target;
//        LoadMissiles();
//        OpenPanels();
//    }

	private void LoadMissiles()
    {
        if (missiles != null) return;
		missiles = new MissileController[missileCount * 2];
		for (int i = 0; i < missileCount; i++)
        {
			Transform leftPos = leftMissilePositions [i];
			Transform rightPos = rightMissilePositions [i];
            GameObject leftMissile = GameObject.Instantiate(missilePrefab, leftPos.position, leftPos.rotation) as GameObject;
			GameObject rightMissile = GameObject.Instantiate(missilePrefab, rightPos.position, rightPos.rotation) as GameObject;
			missiles[i] = leftMissile.GetComponent<MissileController>();
			missiles[i].Setup(leftPanel.transform, sphereColliders);
			missiles[i + missileCount] = rightMissile.GetComponent<MissileController>();
			missiles[i + missileCount].Setup(rightPanel.transform, sphereColliders);
            if (target != null)
            {
                missiles[i].SetTarget(target);
                missiles[i + missileCount].SetTarget(target);
            }
        }

		loaded = true;
    }

//    private void Fire()
//    {
//        if (!isOpen) return;
//
//		StartCoroutine(FiringSequence());
//
////		if (refireCount > 0) {
////			StartCoroutine (Refire ());
////		} else {
////			target = null;
////			ClosePanels();
////		}
//    }

	IEnumerator FiringSequence(Transform _target){
		if (firing)
			yield break;
		firing = true;
		target = _target;
		OpenPanels();
		yield return new WaitUntil (() => isOpen);
		for (int i = 0; i <= refireCount; i++){
			yield return new WaitUntil (() => missiles == null);
			LoadMissiles();
			yield return new WaitUntil (() => loaded);
			StartCoroutine(LaunchMissiles(i < refireCount));
		}
	}

	IEnumerator LaunchMissiles(bool refire)
    {
		for (int i = 0; i < missileCount; i++)
        {
            missiles[i].Fire();
            yield return new WaitForSeconds(launchTimer);
			missiles[i + missileCount].Fire();
			yield return new WaitForSeconds(launchTimer);
        }
        missiles = null;
		loaded = false;

		if (!refire) {
			yield return new WaitForSeconds (0.5f);
			ClosePanels ();
		} 
    }

    private void OpenPanels()
    {
        StartCoroutine(Transition(panelOpenAngle));
    }

    private void ClosePanels()
    {
        StartCoroutine(Transition(0f));
		firing = false;
    }

    IEnumerator Transition(float targetAngle)
    {
        if (inTransition) yield break;

        inTransition = true;

        //float leftStartAngle = leftPanel.transform.localEulerAngles.z;
        //float rightStartAngle = rightPanel.transform.localEulerAngles.z;

        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.fixedDeltaTime * (Time.timeScale / panelOpenDuration);
            float leftAngle = Mathf.LerpAngle(leftPanel.transform.eulerAngles.z, targetAngle, t);
            leftPanel.transform.localEulerAngles = new Vector3(0f, 0f, leftAngle);

            float rightAngle = Mathf.LerpAngle(rightPanel.transform.eulerAngles.z, -targetAngle, t);
            rightPanel.transform.localEulerAngles = new Vector3(0f, 0f, rightAngle);

            yield return 0;
        }
        
        isOpen = (targetAngle == panelOpenAngle);
        inTransition = false;
    }
    
}
