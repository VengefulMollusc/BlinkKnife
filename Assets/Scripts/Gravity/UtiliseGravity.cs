using UnityEngine;
using System.Collections;

public class UtiliseGravity : MonoBehaviour {

    [SerializeField] private float gravityModifier = 1f;

    private Rigidbody rb;

    private bool useGravity;

    private IEnumerator tempGravityDisableEnumerator;

    private Vector3 currentGravityVector;
    private float currentGravityStrength;

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

	void OnEnable () {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            Debug.LogError("No Rigidbody found");

        // Just in case
        rb.useGravity = false;
	    useGravity = true;

	    UpdateGravityValues();

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

    void UpdateGravityValues()
    {
        currentGravityStrength = GlobalGravityControl.GetGravityStrength() * gravityModifier;
        currentGravityVector = GlobalGravityControl.GetCurrentGravityVector();
    }

    void FixedUpdate ()
    {
        if (!useGravity)
            return;

        UpdateGravityValues();
        
        rb.AddForce(currentGravityVector * currentGravityStrength, ForceMode.Acceleration);
    }

    // Handles BoostNotifications from BoostRing
    void OnBoostNotification(object sender, object args)
    {
        GameObject obj = args as GameObject;
        if (obj != gameObject)
            return;

        TempDisableGravity(0.2f);
    }

    public void TempDisableGravity(float _time)
    {
        if (tempGravityDisableEnumerator != null)
            StopCoroutine(tempGravityDisableEnumerator);

        tempGravityDisableEnumerator = TempDisableGravityCoroutine(_time);
        StartCoroutine(tempGravityDisableEnumerator);
    }

    private IEnumerator TempDisableGravityCoroutine(float _time)
    {
        useGravity = false;
        while (_time > 0f)
        {
            _time -= Time.deltaTime;
            yield return 0;
        }
        useGravity = true;
    }
}
