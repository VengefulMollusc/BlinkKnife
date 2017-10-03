using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using AssemblyCSharp;

public class PlayerKnifeController : MonoBehaviour {

    [Header("General Settings")]
	[SerializeField]
	private bool alwaysGravShift = false;

	[SerializeField]
	private int maxWarps = 3;
	private int currentWarps;
	[SerializeField]
	private float warpRechargeTime = 2f;
	private float warpRecharge;

    [SerializeField]
    private float warpWaitTime = 0.2f;
    [SerializeField]
    private float bounceWarpWaitTime = 0.5f;
    private float warpCountDown = 0;
    
    private bool bounceWarp = false;

    [Header("Weapon Settings")]
	[SerializeField]
	private string primaryKnifeBtn = "Fire1";

    [SerializeField]
    private string altKnifeBtn = "Fire2";

    [SerializeField]
    private string abilityButton = "Fire3";

    [SerializeField]
	private float throwStrength = 5f;

    [SerializeField]
    private float throwAngleModifier = 2f;

    [SerializeField]
    private float throwHeightModifier = -0.2f;

	[SerializeField]
	private float spinSpeed = 10f;

    [Header("Prefabs")]
	[SerializeField]
	private GameObject blinkKnifePrefab;

    [SerializeField]
    private GameObject bounceKnifePrefab;

    private bool lockKnife = false;

    private WeaponController weapon;

    [SerializeField]
	private GameObject knifeInHand;
	private Renderer knifeRenderer;
    
	private GameObject uiControllerObject;
	private UIController uiController;
    
	private GameObject player;
	private Collider playerCollider;
    private PlayerMotor playerMotor;

	private GameObject knife;
	private KnifeController knifeController;

    [SerializeField]
    [Range(-100f, 0f)]
    private float warpCost = -30f;

    /*
     * Need to restructure to allow for knife 'mods'
     * Mods work around or change knife functionality
     * 
     * Allow multiple mods?
     * 
     * Active Feature Additions:
     *  Missile Redirect
     *      Becomes the target of all active (launched?) missiles when thrown
     *  
     *  Create Geometry
     *      Similar to cube weapon functionality.
     *      Spawn geometry when knife lands?
     *      raise shield while knife is grounded?
     *      
     *  EMP
     *      Disable enemies/tech within radius of landing
     *      
     *  Movement alteration?
     *      Launch player at surface normal when warp?
     *      Temporary speed boost on warp?
     *      Allow midair warp?
     *      
     *  Gravity shift
     *      Surface knife collided with becomes 'down'
     *      temporary?
     *      
     *  Multi-warp
     *      throw multiple knifes up to a limit, then warp to all of them in sequence
     *      
     *  Infinite warp
     *      temporary infinite warps
     *      
     *      
     * Passive/Behaviour Change:
     *  Instant Travel (Longbow - borderlands)
     *      Raycast target then warp knife instantly
     *      Through transparent surfaces?
     *      Pinpoint accurate
     *      Potential instant warp, no wait time?
     *      
     *  Homing
     *      lock on to enemies?
     *      act like missile?
     */

    /*
     * Could change ui to single bar?
     * Gravity Rush style
     * midair momentum halt uses bar?
     * warp uses percentage of bar?
     */

	void Start (){
	    uiControllerObject = GameObject.FindGameObjectWithTag("UIParent");
        // check for missing prefabs
        if (blinkKnifePrefab == null){
            throw new MissingReferenceException("No blinkKnifePrefab object given.");
        }
        if (bounceKnifePrefab == null)
        {
            throw new MissingReferenceException("No bounceKnifePrefab object given.");
        }
        if (knifeInHand == null){
            throw new MissingReferenceException("No knifeInHand object given.");
        }
        if (uiControllerObject == null){
            throw new MissingReferenceException("No uiController object given.");
        }

		knifeRenderer = knifeInHand.GetComponent<Renderer> ();

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            throw new MissingReferenceException("No player object found.");
        playerCollider = player.GetComponent<Collider> ();
        playerMotor = player.GetComponent<PlayerMotor>();

		uiController = uiControllerObject.GetComponent<UIController> ();

        weapon = GetComponent<WeaponController>();
        weapon.Setup(this);

		currentWarps = maxWarps;

	}

	void Update (){
        // Don't allow input if player is frozen
        if (playerMotor.IsFrozen()) return;

        if (Input.GetButtonDown(primaryKnifeBtn) && !lockKnife)
        {
            // knife primary throw
            // throw blinkknife
            if (knife == null)
            {
                ThrowKnife(throwStrength);
            }
        }
        else if (Input.GetButtonDown(altKnifeBtn) && !lockKnife)
        {
            // secondary knife throw
            // throw bounce knife
            if (knife == null)
            {
                ThrowWarp();
            }
            else if (knife != null && !bounceWarp)
            {
                // return thrown blink knife
                ReturnKnife();
                knifeRenderer.enabled = true;
            }
        } else if (Input.GetButtonDown(abilityButton))
		{
            // weapon button
			if (weapon.ClickMouse (knife, transform, playerCollider)) {
                // if weapon activates, lock knife
				lockKnife = true;
			}
        } else if (Input.GetButtonUp(abilityButton))
        {
            // release weapon button
            if (weapon.ReleaseMouse())
            {
                // unlock knife if weapon released
                lockKnife = false;
            }
        }

        // check warp progress
        CheckIfWarp ();


	    // recharge warp counters
        if (playerMotor.IsOnGround())
		    RechargeWarps ();
	}

    /*
     * Checks the current warp countdown and whether we need to warp now
     */
	void CheckIfWarp (){
        if (!lockKnife && knife != null && ((Input.GetButton(primaryKnifeBtn) && knifeController.HasCollided()) || bounceWarp)) { // Require mouse click to warp
          //if (!lockKnife && knife != null && (knifeController.HasCollided() || bounceWarp)) { // warps instantly without mouse click
          // we are trying to warp
            if (((bounceWarp && warpCountDown >= bounceWarpWaitTime) || (!bounceWarp && warpCountDown >= warpWaitTime)) && currentWarps >= 1) {
                // warp if wait time is reached

                // Remove this line to have countdown ui stay filled while warping
                warpCountDown = 0f;

                Warp ();
			} else
            {
                warpCountDown += Time.deltaTime;
            }
		}
        else
        {
            // reset countdown
            warpCountDown = 0f;
        }

        // activate warp ui
        //warpUIFill.rectTransform.localScale = new Vector3 (warpCountDown/warpWaitTime, warpCountDown/warpWaitTime, 1f);
	}

    // recharge warps based on time
	void RechargeWarps (){
		if (currentWarps < maxWarps){
			warpRecharge -= Time.fixedDeltaTime;
			if (warpRecharge <= 0){
				//warpCounters [currentWarps].enabled = true;
				currentWarps++;

				if (currentWarps < maxWarps){
					warpRecharge = warpRechargeTime;
				}
			}
		}
	}

    /*
     * Throw knife at given strength
     */
	void ThrowKnife (float _strength){

        // unfreeze player if hanging on wall
        // playerMotor.UnFreeze ();

        Quaternion throwDirectionQuaternion = Quaternion.AngleAxis(-throwAngleModifier, transform.right);
        Vector3 throwDirection = throwDirectionQuaternion * transform.forward;
        Vector3 throwPosition = transform.position + (transform.up * throwHeightModifier);

        if (bounceWarp) {
            // throw bounce knife
			knife = (GameObject)Instantiate (bounceKnifePrefab, throwPosition, transform.rotation);
		} else {
            // throw regular (blink) knife
			knife = (GameObject)Instantiate (blinkKnifePrefab, throwPosition, transform.rotation * throwDirectionQuaternion);
		}
		knifeController = knife.GetComponent<KnifeController> ();

        // ignore collisions between knife and this player
		Physics.IgnoreCollision (knife.GetComponent<Collider>(), playerCollider);

		if (knifeController == null){
			Debug.LogError ("No KnifeController found on knife prefab");
			return;
        }

        // set up and throw knife object
        knifeController.Setup (this, -playerMotor.transform.up, spinSpeed);
//		knifeController.Throw ((transform.forward * throwStrength) 
//			+ (playerRb.velocity * 0.5f), this);
		knifeController.Throw (throwDirection * _strength);

        // hide knife view object
		knifeRenderer.enabled = false;

	}

    /*
     * Activate the warp countdown and throw knife
     *  - Warps as soon as countdown is over
     */
    void ThrowWarp()
    {
        if (currentWarps < 1) return;
        bounceWarp = true;
        ThrowKnife(throwStrength);
    }

    /*
     * Returns a thrown knife
     */
    public void ReturnKnife()
    {
        Destroy(knife);
        knife = null;
        knifeController = null;

        //knifeRenderer.enabled = true;
    }

    /*
     * Perform warp to current knife position
     */
	public void Warp (){

		if (knife == null){
			return;
		}

        // move player to knife position and inherit velocity
        bool shiftGravity = (knifeController.ShiftGravity() || alwaysGravShift);
		Vector3 _velocity = knifeController.GetVelocity(bounceWarp).normalized;
		//playerMotor.WarpToKnife(knifeController.GetWarpPosition(), _velocity, knifeController.GetObjectCollided(), knifeController.GetSurfaceNormal());
        playerMotor.WarpToKnife(shiftGravity, _velocity, knifeController);

        if (bounceWarp)
		{
			bounceWarp = false;
		}

        // return knife once warped
        ReturnKnife();

        // remove a warp from warp counters and begin recharge
		currentWarps -= 1;
		//warpCounters [currentWarps].enabled = false;
		if (warpRecharge <= 0){
			warpRecharge = warpRechargeTime;
		}
	}

    /*
     * Passthrough method to pass knife location to ui element
     * TODO: replace with event
     */
	public void SetKnifeMarkerTarget (Transform target, bool gravShift){
		uiController.SetKnifeMarkerTarget (target, gravShift);
	}

    /*
     * Hide and show the knife view model
     * TODO: merge methods
     */
    public void HideKnife()
    {
        knifeRenderer.enabled = false;
    }

    public void UnHideKnife()
    {
        knifeRenderer.enabled = true;
    }

    /*
     * Set the knife lock
     *  - used by weapons
     */
    public void SetKnifeLock(bool _locked)
    {
        lockKnife = _locked;
    }

    // Methods to return info for UI
    public float GetWarpsNormalised()
    {
        return (float)currentWarps / (float)maxWarps;
    }

    public float GetWarpCountdownNormalised()
    {
        if (bounceWarp)
            return warpCountDown / bounceWarpWaitTime;
        return warpCountDown / warpWaitTime;
    }

    public float GetWarpRechargeNormalised()
    {
        return warpRecharge / warpRechargeTime;
    }

}
