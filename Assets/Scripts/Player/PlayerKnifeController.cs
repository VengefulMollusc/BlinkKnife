using UnityEngine;

public class PlayerKnifeController : MonoBehaviour
{


    [Header("General Settings")]
    [SerializeField]
    private bool alwaysGravShift = false;

    [SerializeField]
    private float timeToAutoRecall = 1.5f;
    private float autoRecallTimer;

    [SerializeField]
    private int maxWarps = 3;
    private int currentWarps;
    [SerializeField]
    private float warpRechargeTime = 2f;
    private float warpRecharge;

    [SerializeField]
    private float warpWaitTime = 0.1f;
    [SerializeField]
    private float bounceWarpWaitTime = 0.2f;
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
    private Collider[] playerColliders;
    private PlayerMotor playerMotor;

    private GameObject knife;
    private KnifeController knifeController;

    private WarpLookAheadCollider warpLookAheadCollider;

    private bool playerIsLit;

    //[SerializeField]
    //[Range(-100f, 0f)]
    //private float warpCost = -30f;

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

    void OnEnable()
    {
        uiControllerObject = GameObject.FindGameObjectWithTag("UIParent");
        // check for missing prefabs
        if (blinkKnifePrefab == null)
        {
            throw new MissingReferenceException("No blinkKnifePrefab object given.");
        }
        if (bounceKnifePrefab == null)
        {
            throw new MissingReferenceException("No bounceKnifePrefab object given.");
        }
        if (knifeInHand == null)
        {
            throw new MissingReferenceException("No knifeInHand object given.");
        }
        if (uiControllerObject == null)
        {
            throw new MissingReferenceException("No uiController object given.");
        }

        knifeRenderer = knifeInHand.GetComponent<Renderer>();

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            throw new MissingReferenceException("No player object found.");
        playerColliders = player.GetComponentsInChildren<Collider>();
        playerMotor = player.GetComponent<PlayerMotor>();

        uiController = uiControllerObject.GetComponent<UIController>();

        weapon = GetComponent<WeaponController>();
        weapon.Setup(this);

        currentWarps = maxWarps;

        this.AddObserver(OnKnifeBounce, KnifeController.KnifeBounceNotification);
        this.AddObserver(EndWarp, TransitionCameraController.WarpEndNotification);
        this.AddObserver(OnLightStatusNotification, LightSensor.LightStatusNotification);
        this.AddObserver(OnFibreOpticWarp, KnifeController.FibreOpticWarpNotification);

        warpLookAheadCollider = GameObject.FindGameObjectWithTag("WarpLookAheadCollider").GetComponent<WarpLookAheadCollider>();
    }

    void OnDisable()
    {
        this.RemoveObserver(OnKnifeBounce, KnifeController.KnifeBounceNotification);
        this.RemoveObserver(EndWarp, TransitionCameraController.WarpEndNotification);
        this.RemoveObserver(OnFibreOpticWarp, KnifeController.FibreOpticWarpNotification);
    }

    void Update()
    {
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
                //knifeRenderer.enabled = true;
            }
        }
        else if (Input.GetButtonDown(abilityButton))
        {
            // weapon button
            if (weapon.ClickMouse(knife, transform, playerColliders))
            {
                // if weapon activates, lock knife
                lockKnife = true;
            }
        }
        else if (Input.GetButtonUp(abilityButton))
        {
            // release weapon button
            if (weapon.ReleaseMouse())
            {
                // unlock knife if weapon released
                lockKnife = false;
            }
        }

        // check warp progress
        CheckIfWarp();

        // check if recall
        CheckIfRecall();

        // recharge warp counters
        RechargeWarps();
    }

    /*
     * Checks if the auto recall timer has run out, if so, returns the thrown knife
     */
    void CheckIfRecall()
    {
        if (autoRecallTimer <= 0 || knife == null || knifeController.HasStuck())
            return;

        autoRecallTimer -= Time.deltaTime;

        if (autoRecallTimer <= 0)
        {
            ReturnKnife();
            //knifeRenderer.enabled = true;
        }
    }

    /*
     * Checks the current warp countdown and whether we need to warp now
     */
    void CheckIfWarp()
    {
        if (!lockKnife && knife != null && ((Input.GetButton(primaryKnifeBtn) && knifeController.HasStuck()) || bounceWarp))
        { // Require mouse click to warp
          //if (!lockKnife && knife != null && (knifeController.HasStuck() || bounceWarp)) { // warps instantly without mouse click
          // we are trying to warp
            if (((bounceWarp && warpCountDown >= bounceWarpWaitTime) || (!bounceWarp && warpCountDown >= warpWaitTime)) && currentWarps >= 1)
            {
                // warp if wait time is reached

                // Remove this line to have countdown ui stay filled while warping
                warpCountDown = 0f;

                Warp();
            }
            else
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

    void OnKnifeBounce(object sender, object args)
    {
        warpCountDown = 0f;
        autoRecallTimer = timeToAutoRecall;
    }

    void OnLightStatusNotification(object sender, object args)
    {
        Info<GameObject, float> info = (Info<GameObject, float>)args;
        if (info.arg0 == player)
            playerIsLit = info.arg1 > 0f;
    }

    // recharge warps based on time (if player is lit)
    void RechargeWarps()
    {
        if (currentWarps < ((playerIsLit) ? maxWarps : 1))
        {
            float rechargeSpeedMod = 1f;
            if (!playerIsLit)
                rechargeSpeedMod *= 0.5f;
            if (!playerMotor.IsOnGround())
                rechargeSpeedMod *= 0.25f;

            warpRecharge -= (Time.deltaTime * rechargeSpeedMod);

            //// recharge warps
            //if (playerMotor.IsOnGround() && playerIsLit)
            //    warpRecharge -= Time.deltaTime;
            //else
            //    warpRecharge -= (Time.deltaTime * 0.2f); // recharge at slower rate when airborne

            if (warpRecharge <= 0)
            {
                currentWarps++;

                if (currentWarps < maxWarps) // should restrict recharge to a single warp while not lit
                    warpRecharge = warpRechargeTime;
            }
        }

        if (!playerIsLit && currentWarps > 0)
        {
            // decay partial recharges
            if (warpRecharge < warpRechargeTime)
                warpRecharge += (Time.deltaTime * 0.1f);

            if (warpRecharge > warpRechargeTime)
                warpRecharge = warpRechargeTime;
        }
    }

    /*
     * Throw knife at given strength
     */
    void ThrowKnife(float _strength)
    {

        // unfreeze player if hanging on wall
        // playerMotor.UnFreeze ();

        Quaternion throwDirectionQuaternion = Quaternion.AngleAxis(-throwAngleModifier, transform.right);
        Vector3 throwDirection = throwDirectionQuaternion * transform.forward;
        Vector3 throwPosition = transform.position + (transform.up * throwHeightModifier);

        if (bounceWarp)
        {
            // throw bounce knife
            //knife = (GameObject)Instantiate (bounceKnifePrefab, throwPosition, transform.rotation);
            knife = Instantiate(bounceKnifePrefab, throwPosition, GlobalGravityControl.GetGravityRotation());
        }
        else
        {
            // throw regular (blink) knife
            //knife = (GameObject)Instantiate (blinkKnifePrefab, throwPosition, transform.rotation * throwDirectionQuaternion);
            knife = Instantiate(blinkKnifePrefab, throwPosition, GlobalGravityControl.GetGravityRotation());
        }
        knifeController = knife.GetComponent<KnifeController>();

        // ignore collisions between knife and this player
        //Utilities.IgnoreCollisions(knife.GetComponent<Collider>(), playerColliders, true);

        if (knifeController == null)
        {
            Debug.LogError("No KnifeController found on knife prefab");
            return;
        }

        // set up and throw knife object
        knifeController.Setup(warpLookAheadCollider);
        //		knifeController.Throw ((transform.forward * throwStrength) 
        //			+ (playerRb.velocity * 0.5f), this);
        knifeController.Throw(throwDirection * _strength);

        // hide knife view object
        knifeRenderer.enabled = false;

        autoRecallTimer = timeToAutoRecall;

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

        UnHideKnife();
        //knifeRenderer.enabled = true;
    }

    /*
     * Triggers FibreOptic warp in playerMotor
     */
    void OnFibreOpticWarp(object sender, object args)
    {
        if (knife == null)
            return;

        if (currentWarps > 0)
        {
            // perform forced fibreoptic warp
            FibreOpticController fibreOpticController = (FibreOpticController)args;

            playerMotor.WarpToKnife(false, knifeController, false, fibreOpticController);

            bounceWarp = false;
        }
        else
        {
            // return knife
            ReturnKnife();
        }
    }

    /*
     * Perform warp to current knife position
     */
    public void Warp()
    {

        if (knife == null)
            return;

        // move player to knife position and inherit velocity
        bool shiftGravity = (knifeController.ShiftGravity() || alwaysGravShift);
        //Vector3 _velocity = knifeController.GetVelocity().normalized;
        //playerMotor.WarpToKnife(knifeController.GetWarpPosition(), _velocity, knifeController.GetStuckObject(), knifeController.GetSurfaceNormal());
        playerMotor.WarpToKnife(shiftGravity, knifeController, bounceWarp);

        bounceWarp = false;
    }

    void EndWarp(object sender, object args)
    {
        // return knife once warped
        ReturnKnife();

        // remove a warp from warp counters and begin recharge
        currentWarps -= 1;

        if (warpRecharge <= 0)
        {
            warpRecharge = warpRechargeTime;
        }
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
