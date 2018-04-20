using UnityEngine;
using System.Collections;
using AssemblyCSharp;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    [Header("General/Colours")]
    [SerializeField]
    private Color baseUiColor = new Color32(255, 100, 0, 255);
    [SerializeField]
    private Color altUiColor = new Color32(0, 175, 255, 255);

    [SerializeField]
    private GameObject uiKnifeMarker;
    private KnifeMarker knifeMarker;

    [SerializeField]
    private float decayDelay = 0.5f;
    [SerializeField]
    [Range(0.0f, 1f)]
    private float decaySpeed = 0.5f;
    
    private GameObject playerCamera;
    private PlayerKnifeController playerKnifeController;
    private GameObject player;
    private PlayerMotor playerMotor;

    [Header("Health")]
    [SerializeField]
    private Image healthUI;
    [SerializeField]
    private Image healthTransitionUI;
    [SerializeField]
    private Color healthBaseColor = new Color32(255, 100, 0, 255);
    [SerializeField]
    private Color healthDecayColor = new Color32(200, 0, 0, 200);

    private float currentHealth;
    private float healthTransition;
    private Coroutine healthTransitionCoroutine;

    [Header("Energy")]
    [SerializeField]
    private Image energyUI;
    [SerializeField]
    private Image energyTransitionUI;
    [SerializeField]
    private Color energyBaseColor = new Color32(0, 175, 255, 255);
    [SerializeField]
    private Color energyDecayColor = new Color32(0, 0, 200, 200);

    private float currentEnergy;
    private float energyTransition;
    private Coroutine energyTransitionCoroutine;

    [Header("Warp")]
    [SerializeField]
    private Image warpUI;
    [SerializeField]
    private Color warpBaseColor = new Color32(0, 175, 255, 255);
    private float currentWarps;

    [SerializeField]
    private Image warpRechargeUI;
    [SerializeField]
    private Color warpRechargeColor = new Color32(0, 175, 255, 255);
    private float currentWarpRecharge;

    [SerializeField]
    private Image warpCountdownUI;
    [SerializeField]
    private Color warpCountdownColor = new Color32(0, 175, 255, 255);
    private float currentWarpCountdown;




    // Use this for initialization
    void OnEnable () {
        playerCamera = GameObject.Find("MainCamera");

        if (uiKnifeMarker == null)
            throw new MissingReferenceException("No knife marker object given.");

        knifeMarker = uiKnifeMarker.GetComponent<KnifeMarker>();
        knifeMarker.SetColours(baseUiColor, altUiColor);

        playerKnifeController = playerCamera.GetComponent<PlayerKnifeController>();

        player = GameObject.FindGameObjectWithTag("Player");
        playerMotor = player.GetComponent<PlayerMotor>();

        // health variables
        healthUI.color = healthBaseColor;
        healthTransitionUI.color = healthDecayColor;

        //currentHealth = playerMotor.GetHealthNormalised();
        currentHealth = 1f;
        healthTransition = currentHealth;

        // energy variables
        energyUI.color = energyBaseColor;
        energyTransitionUI.color = energyDecayColor;

        //currentEnergy = playerMotor.GetEnergyNormalised();
        currentEnergy = 1f;
        energyTransition = currentEnergy;

        // Warp variables
        warpUI.color = warpBaseColor;
        warpRechargeUI.color = warpRechargeColor;
        warpCountdownUI.color = warpCountdownColor;

        currentWarps = playerKnifeController.GetWarpsNormalised();
        currentWarpRecharge = playerKnifeController.GetWarpRechargeNormalised();
        currentWarpCountdown = playerKnifeController.GetWarpCountdownNormalised();

        this.AddObserver(HandleKnifeMarkerNotification, KnifeController.ShowKnifeMarkerNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(HandleKnifeMarkerNotification, KnifeController.ShowKnifeMarkerNotification);
    }
	
	// Update is called once per frame
	void Update () {
        // Update values
        UpdateHealth();
        UpdateEnergy();
        UpdateWarp();

        // Draw UI
        UpdateUI();
    }

    /*
     * Draws UI elements
     */
    private void UpdateUI()
    {
        // Update health UI
        healthUI.fillAmount = currentHealth;
        healthTransitionUI.fillAmount = healthTransition;

        // Update energy UI
        energyUI.fillAmount = currentEnergy;
        energyTransitionUI.fillAmount = energyTransition;

        // Update warp UI
        warpUI.fillAmount = currentWarps;
        // if currentwarps is full, dont display the recharge bar
        // (it's full by default)
        if (currentWarps >= 1f) {
            warpRechargeUI.fillAmount = 0f;
        } else {
            warpRechargeUI.fillAmount = currentWarpRecharge;
        }
        warpCountdownUI.fillAmount = currentWarpCountdown;
    }

    /*
     * Updates health values and triggers transition
     */
    private void UpdateHealth()
    {
        //float newHealth = playerMotor.GetHealthNormalised();
        float newHealth = 1f;

        if (newHealth == currentHealth)
            return;

        if (newHealth > currentHealth)
        {
            currentHealth = newHealth;
            // StopCoroutine(healthTransitionCoroutine);
            return;
        }

        // newHealth < currentHealth
        if (healthTransition < currentHealth)
            healthTransition = currentHealth;

        currentHealth = newHealth;

        if (healthTransitionCoroutine != null)
            StopCoroutine(healthTransitionCoroutine);
        healthTransitionCoroutine = StartCoroutine(HealthTransition());
    }

    

    /*
     * Updates health values and triggers transition
     */
    private void UpdateEnergy()
    {
        //float newEnergy = playerMotor.GetEnergyNormalised();
        float newEnergy = 1f;

        if (newEnergy == currentEnergy)
            return;

        if (newEnergy > currentEnergy)
        {
            currentEnergy = newEnergy;
            return;
        }

        // newEnergy < currentEnergy
        if (energyTransition < currentEnergy)
            energyTransition = currentEnergy;

        currentEnergy = newEnergy;

        if (energyTransitionCoroutine != null)
            StopCoroutine(energyTransitionCoroutine);
        energyTransitionCoroutine = StartCoroutine(EnergyTransition());
    }

    private void UpdateWarp()
    {
        currentWarps = playerKnifeController.GetWarpsNormalised();
        currentWarpRecharge = 1-playerKnifeController.GetWarpRechargeNormalised();
        currentWarpCountdown = playerKnifeController.GetWarpCountdownNormalised();
    }

    void HandleKnifeMarkerNotification(object sender, object args)
    {

        if (args == null)
        {
            knifeMarker.SetTarget(null, false);
            return;
        }

        Info<Transform, bool> info = (Info<Transform, bool>) args;
        knifeMarker.SetTarget(info.arg0, info.arg1);
    }

    public void SetKnifeMarkerTarget(Transform _target, bool _altColour)
    {
        knifeMarker.SetTarget(_target, _altColour);
    }

    /*
     * waits for decayDelay, then transitions to current value over decayTime
     */
    private IEnumerator HealthTransition()
    {
        yield return new WaitForSeconds(decayDelay);

        while (healthTransition > currentHealth)
        {
            healthTransition -= Time.deltaTime * decaySpeed;
            yield return 0;
        }

        // unsure if nessecary
        healthTransitionCoroutine = null;
    }

    private IEnumerator EnergyTransition()
    {
        yield return new WaitForSeconds(decayDelay);

        while (energyTransition > currentEnergy)
        {
            energyTransition -= Time.deltaTime * decaySpeed;
            yield return 0;
        }

        // unsure if nessecary
        energyTransitionCoroutine = null;
    }
}
