using UnityEngine;
using System.Collections;

public class UtiliseGravity : MonoBehaviour
{
    /*
     * Used to apply gravity to GameObjects
     */

    // Modifies gravity strength
    [SerializeField] private float gravityModifier = 1f;

    private Rigidbody rb;

    private bool useGravity;

    private IEnumerator tempGravityDisableCoroutine;

    private Vector3 currentGravityVector;
    private float currentGravityStrength;

    private float gravStrengthModifier = 1f;

    /*
     * This could have a lot more options,
     * multiplication factor for strength
     * torque rather than velocity?
     * 
     * start and end positions?
     * 
     * axis/rotation lock achieved by Rigidbody settings
     * 
     * settings for kinematic?
     * make sure not effected by hitting player model?
     * 
     * Only affected when player parented to?
     * Stand on level to move?
     * stand on edge of cog to rotate?
     * 
     * Shouldn't need dynamic collisions as won't be moving too fast (so far)
     */

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            Debug.LogError("No Rigidbody found");

        // Just in case
        rb.useGravity = false;
        useGravity = true;

        UpdateGravityValues();
    }

    void OnEnable()
    {
        this.AddObserver(OnBoostNotification, BoostRing.BoostNotification);
        this.AddObserver(OnGravityChange, GlobalGravityControl.GravityChangeNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnBoostNotification, BoostRing.BoostNotification);
        this.RemoveObserver(OnGravityChange, GlobalGravityControl.GravityChangeNotification);
    }

    void OnGravityChange(object sender, object args)
    {
        UpdateGravityValues();
    }

    // Update current gravity direction and strength
    void UpdateGravityValues()
    {
        currentGravityStrength = GlobalGravityControl.GetGravityStrength() * gravityModifier;
        currentGravityVector = GlobalGravityControl.GetCurrentGravityVector();
    }

    void FixedUpdate()
    {
        if (!useGravity)
            return;

        UpdateGravityValues();

        if (!gravStrengthModifier.Equals(1f))
            rb.AddForce(currentGravityVector * currentGravityStrength * gravStrengthModifier, ForceMode.Acceleration);
        else
            rb.AddForce(currentGravityVector * currentGravityStrength, ForceMode.Acceleration);
    }

    public bool UseGravity()
    {
        return useGravity;
    }

    public void SetUseGravity(bool useGrav)
    {
        useGravity = useGrav;
    }

    // Handles BoostNotifications from BoostRing
    void OnBoostNotification(object sender, object args)
    {
        Info<GameObject, Vector3> info = (Info<GameObject, Vector3>)args;
        if (info.arg0 != gameObject)
            return;

        TempDisableGravity(0.2f);
    }

    /*
     * Disables gravity on the object for a short time
     */
    public void TempDisableGravity(float _time, float _fadeTime = 0f)
    {
        if (tempGravityDisableCoroutine != null)
            StopCoroutine(tempGravityDisableCoroutine);

        tempGravityDisableCoroutine = TempDisableGravityCoroutine(_time, _fadeTime);
        StartCoroutine(tempGravityDisableCoroutine);
    }

    /*
     * Coroutine to handle temp gravity disable and fadein
     */
    private IEnumerator TempDisableGravityCoroutine(float _time, float _fadeTime)
    {
        useGravity = false;
        yield return new WaitForSeconds(_time);
        useGravity = true;

        // Fade gravity back in
        float fade = 0f;
        while (fade < 1f)
        {
            gravStrengthModifier = fade;
            fade += Time.deltaTime * (Time.timeScale / _fadeTime);
            yield return 0;
        }
        gravStrengthModifier = 1f;
    }
}
