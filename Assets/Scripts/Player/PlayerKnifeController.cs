using UnityEngine;

public class PlayerKnifeController : MonoBehaviour
{
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
    private float warpWaitTime = 0.1f;
    private float warpCountDown;

    private bool autoWarp;

    [Header("Knife Settings")]
    [SerializeField]
    private string leftMouse = "Fire1";

    [SerializeField]
    private string rightMouse = "Fire2";

    [SerializeField]
    private string middleMouse = "Fire3";

    [SerializeField]
    private float throwStrength = 5f;

    //[SerializeField]
    //private float throwAngleModifier = 2f;

    //[SerializeField]
    //private float throwHeightModifier = -0.2f;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject primaryKnifePrefab;

    [SerializeField]
    private GameObject secondaryKnifePrefab;

    private WeaponController weapon;

    [SerializeField]
    private GameObject knifeInHand;
    private Renderer knifeViewModelRenderer;

    // TODO: replace with notification system to handle UI variables
    //private GameObject uiControllerObject;
    //private UIController uiController;

    private GameObject player;
    private Collider[] playerColliders; // TODO: this should be removed - weapon shouldn't collide through collision matrix instead
    private PlayerMotor playerMotor;

    private GameObject knife;
    private KnifeController knifeController;

    private WarpLookAheadCollider warpLookAheadCollider;

    private bool playerIsLit;

    /*
     * Could change ui to single bar?
     * Gravity Rush style
     * midair momentum halt uses bar?
     * warp uses percentage of bar?
     */

    void OnEnable()
    {
        //uiControllerObject = GameObject.FindGameObjectWithTag("UIParent");
        // check for missing prefabs
        if (primaryKnifePrefab == null)
            throw new MissingReferenceException("No primaryKnifePrefab object given.");
        if (secondaryKnifePrefab == null)
            throw new MissingReferenceException("No secondaryKnifePrefab object given.");
        if (knifeInHand == null)
            throw new MissingReferenceException("No knifeInHand object given.");
        //if (uiControllerObject == null)
        //{
        //    throw new MissingReferenceException("No uiController object given.");
        //}

        knifeViewModelRenderer = knifeInHand.GetComponent<Renderer>();

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            throw new MissingReferenceException("No player object found.");
        playerColliders = player.GetComponentsInChildren<Collider>();
        playerMotor = player.GetComponent<PlayerMotor>();

        //uiController = uiControllerObject.GetComponent<UIController>();

        weapon = GetComponent<WeaponController>();
        weapon.Setup(this);

        currentWarps = maxWarps;

        this.AddObserver(OnReturnKnifeNotification, KnifeController.ReturnKnifeNotification);
        this.AddObserver(EndWarp, TransitionCameraController.WarpEndNotification);
        this.AddObserver(OnLightStatusNotification, LightSensor.LightStatusNotification);
        this.AddObserver(OnFibreOpticWarp, KnifeController.FibreOpticWarpNotification);

        warpLookAheadCollider = GameObject.FindGameObjectWithTag("WarpLookAheadCollider").GetComponent<WarpLookAheadCollider>();
    }

    void OnDisable()
    {
        this.RemoveObserver(OnReturnKnifeNotification, KnifeController.ReturnKnifeNotification);
        this.RemoveObserver(EndWarp, TransitionCameraController.WarpEndNotification);
        this.RemoveObserver(OnLightStatusNotification, LightSensor.LightStatusNotification);
        this.RemoveObserver(OnFibreOpticWarp, KnifeController.FibreOpticWarpNotification);
    }

    void Update()
    {
        // Don't allow input if player is frozen
        if (playerMotor.IsFrozen()) return;

        if (Input.GetButtonDown(leftMouse))
        {
            // Throw primary knife
            if (knife == null)
            {
                ThrowKnife(throwStrength);
            }
        }
        else if (Input.GetButtonDown(rightMouse))
        {
            // secondary knife throw
            if (knife == null)
            {
                ThrowKnife(throwStrength, true);
            }
            else
            {
                // return thrown knife
                knifeController.ReturnKnifeTransition();
            }
        }
        else if (Input.GetButtonDown(middleMouse))
        {
            // weapon button
            //if (weapon.ClickMouse((knife != null) ? knife.transform : null, transform, playerColliders))
            weapon.ClickMouse(null, transform, playerColliders);
        }
        else if (Input.GetButtonUp(middleMouse))
        {
            // release weapon button
            weapon.ReleaseMouse();
        }

        // check warp progress
        CheckIfWarp();

        // recharge warp variables
        RechargeWarps();
    }

    /*
     * Checks the current knife and whether we need to warp now
     */
    void CheckIfWarp()
    {
        if (knife != null && knifeController.CanWarp() && currentWarps >= 1)
        { // Require mouse click to warp
            if (autoWarp)
            {
                Warp();
                return;
            }
            if (Input.GetButton(leftMouse))
            {
                // require manual hold click to warp
                if (warpCountDown >= warpWaitTime)
                    Warp();
                else
                    warpCountDown += Time.deltaTime;
            }
        }
        else
        {
            // reset countdown
            warpCountDown = 0f;
        }
    }

    /*
     * Update lit status based on notification from player
     */
    void OnLightStatusNotification(object sender, object args)
    {
        Info<GameObject, float> info = (Info<GameObject, float>)args;
        if (info.arg0 == player)
            playerIsLit = info.arg1 > 0f;
    }

    /*
     * recharge warps based on whether the player is grounded and/or lit
     */
    void RechargeWarps()
    {
        // charge up to max warps if lit, or just one if not
        if (currentWarps < ((playerIsLit) ? maxWarps : 1))
        {
            // Modify recharge rate if not lit or not grounded
            float rechargeSpeedMod = 1f;
            if (!playerIsLit)
                rechargeSpeedMod *= 0.5f;
            if (!playerMotor.IsOnGround())
                rechargeSpeedMod *= 0.25f;

            warpRecharge -= (Time.deltaTime * rechargeSpeedMod);

            // up warp count if recharge is complete
            if (warpRecharge <= 0)
            {
                currentWarps++;

                //if (currentWarps < maxWarps) // should restrict recharge to a single warp while not lit
                warpRecharge = warpRechargeTime;
            }
        }

        if (!playerIsLit && currentWarps > 0)
        {
            // decay partial recharges if not lit
            if (warpRecharge < warpRechargeTime)
                warpRecharge += (Time.deltaTime * 0.1f);

            if (warpRecharge > warpRechargeTime)
                warpRecharge = warpRechargeTime;
        }
    }

    /*
     * Throw knife at given strength
     */
    void ThrowKnife(float _strength, bool _secondary = false)
    {
        if (currentWarps < 1)
            return;
        // unfreeze player if hanging on wall
        // playerMotor.UnFreeze ();

        // TODO: remove these. are from when knife was affected by gravity
        //Quaternion throwDirectionQuaternion = Quaternion.AngleAxis(-throwAngleModifier, transform.right);
        //Vector3 throwDirection = throwDirectionQuaternion * transform.forward;
        //Vector3 throwPosition = transform.position + (transform.up * throwHeightModifier);

        // TODO: replace with consistent knife instances. one for each type
        // instantiate knife prefab
        knife = Instantiate((_secondary) ? secondaryKnifePrefab : primaryKnifePrefab, transform.position, GlobalGravityControl.GetGravityRotation());
        knifeController = knife.GetComponent<KnifeController>();

        // check that we have actually got a knife
        if (knifeController == null)
        {
            Debug.LogError("No KnifeController found on knife prefab");
            return;
        }

        autoWarp = knifeController.AutoWarp();

        // set up and throw knife object
        knifeController.Setup(knifeInHand.transform, warpLookAheadCollider);
        knifeController.Throw(transform.forward * _strength);

        // hide knife view object
        HideKnife(true);
    }

    public KnifeController GetActiveKnifeController()
    {
        return knifeController;
    }

    /*
     * Returns a thrown knife and nullifies knife variables
     */
    void OnReturnKnifeNotification(object sender, object args)
    {
        ReturnKnife();
    }

    public void ReturnKnife()
    {
        Destroy(knife);
        knife = null;
        knifeController = null;
        autoWarp = false;
        HideKnife(false);
    }

    /*
     * Triggers FibreOptic warp
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

            autoWarp = false;
        }
        else
        {
            // shouldn't trigger, should always have warps available if knife has been thrown
            // return knife
            ReturnKnife();
        }
    }

    /*
     * Trigger warp transition
     */
    public void Warp()
    {
        if (knife == null)
            return;

        // Remove this line to have countdown ui stay filled while warping
        //warpCountDown = 0f;

        // Check whether gravity is changing for this warp
        bool shiftGravity = (knifeController.ShiftGravity() || alwaysGravShift);

        // Trigger warp transition from playerMotor
        playerMotor.WarpToKnife(shiftGravity, knifeController, autoWarp);

        autoWarp = false;
    }

    void EndWarp(object sender, object args)
    {
        // return knife once warped
        ReturnKnife();

        // remove a warp from warp counters and begin recharge
        currentWarps -= 1;

        warpCountDown = 0f;
        
        // reset warp recharges
        warpRecharge = warpRechargeTime;
    }

    /*
     * Hide and show the knife view model
     */
    public void HideKnife(bool _hideKnife)
    {
        knifeViewModelRenderer.enabled = !_hideKnife;
    }

    /*
     * Methods to return info for UI
     */
    public float GetWarpsNormalised()
    {
        return (float)currentWarps / maxWarps;
    }

    public float GetWarpCountdownNormalised()
    {
        return warpCountDown / warpWaitTime;
    }

    public float GetWarpRechargeNormalised()
    {
        return warpRecharge / warpRechargeTime;
    }

}
