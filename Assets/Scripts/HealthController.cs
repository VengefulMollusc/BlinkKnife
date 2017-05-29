using UnityEngine;
using System.Collections;

public class HealthController : MonoBehaviour {

    // CHANGE THIS TO GENERIC RESOURCE CONTROLLER?

    [SerializeField]
    private float healthMax = 100f;
    private float health;
    [SerializeField]
    private float healthRechargeDelay = 5f;
    private float healthRechargeCounter = 0f;
    [SerializeField]
    private float healthRechargeRate = 10f;

    [SerializeField]
    private bool energyIsShield = false;
    [SerializeField]
    private float shldDmgCarryoverThreshold = 10f;

    [SerializeField]
    private float energyMax = 100f;
    private float energy;
    [SerializeField]
    private float energyRechargeDelay = 1f;
    private float energyRechargeCounter = 0f;
    [SerializeField]
    private float energyRechargeRate = 30f;

    // Use this for initialization
    void Start () {
        health = healthMax;
        energy = energyMax;
    }
	
	// Update is called once per frame
	void Update () {
        RechargeEnergy();
        RechargeHealth();
    }

    private void RechargeHealth()
    {
        if (healthRechargeCounter > 0f)
        {
            healthRechargeCounter -= Time.deltaTime;
            return;
        }

        if (health >= healthMax)
            return;

        health += Time.deltaTime * healthRechargeRate;
        if (health > healthMax)
            health = healthMax;
    }

    private void RechargeEnergy()
    {
        if (energyRechargeCounter > 0f)
        {
            energyRechargeCounter -= Time.deltaTime;
            return;
        }

        if (energy >= energyMax)
            return;

        energy += Time.deltaTime * energyRechargeRate;
        if (energy > energyMax)
            energy = energyMax;
    }

    public void ModifyHealth(float _changeAmt)
    {
        float newHealth = health + _changeAmt;

        //if (newHealth <= 0f)
        //{
        //    //if (health > 1f)
        //    //{
        //    //    // KH Second Chance
        //    //    newHealth = 1f;
        //    //}
        //    //else
        //    //{
        //    //    Debug.Log("Player Dead");
        //    //    // Do death things
        //    //}
        //    Debug.Log("Thing Dead");
        //    // Do death things
        //}

        if (newHealth < health)
            healthRechargeCounter = healthRechargeDelay;

        health = Mathf.Clamp(newHealth, 0f, healthMax);
    }

    public void ModifyEnergy(float _changeAmt)
    {
        float newEnergy = energy + _changeAmt;
        // slight threshold here to make last warp more forgiving
        //if (newEnergy <= 0f)
        //{
        //    Debug.Log("Out of energy");
        //}

        if (newEnergy < energy)
            energyRechargeCounter = energyRechargeDelay;

        energy = Mathf.Clamp(newEnergy, 0f, energyMax);
    }

    public void Damage(float _damageAmount)
    {
        if (!energyIsShield)
        {
            DamageHealth(_damageAmount);
            return;
        }

        // use shield before health
        if (energy > 0)
        {
            // add small damage threshold when penetrating remaining shield value
            if (energy < _damageAmount - shldDmgCarryoverThreshold)
                DamageHealth(_damageAmount - shldDmgCarryoverThreshold - energy);
            DamageEnergy(_damageAmount);

            // reset health recharge delay
            healthRechargeCounter = healthRechargeDelay;
        } else
        {
            DamageHealth(_damageAmount);

            // reset shield recharge delay
            energyRechargeCounter = energyRechargeDelay;
        }
    }

    public void DamageHealth(float _damageAmount)
    {
        ModifyHealth(-_damageAmount);
    }

    public void RecoverHealth(float _healAmount)
    {
        ModifyHealth(_healAmount);
    }

    public void DamageEnergy(float _damageAmount)
    {
        ModifyEnergy(-_damageAmount);
    }

    public void RecoverEnergy(float _healAmount)
    {
        ModifyEnergy(_healAmount);
    }

    public float GetHealthNormalised()
    {
        return health / healthMax;
    }

    public float GetEnergyNormalised()
    {
        return energy / energyMax;
    }

    public bool IsDead()
    {
        return health == 0f;
    }
}
