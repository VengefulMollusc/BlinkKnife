using System.Collections.Generic;
using UnityEngine;

public abstract class LightSource : MonoBehaviour
{

    protected List<GameObject> litObjects;

    protected Light light;

    [SerializeField]
    protected LayerMask layerMask;

    // TODO: remove - testing
    [HideInInspector]
    public Info<List<Vector3>, List<Vector3>, List<bool>> testRaycasts;

    public virtual void Start()
    {
        light = GetComponent<Light>();
    }

    public virtual void OnEnable()
    {
        InvokeRepeating("LightSensorCheck", 0f, GlobalVariableController.LightCheckUpdateFrequency);
    }

    public virtual void OnDisable()
    {
        CancelInvoke("LightSensorCheck");
    }

    /*
     * Check light range for LightSensors that should be lit
     */
    public abstract void LightSensorCheck();

    /*
     * Gets the intensity of the light at a given point.
     * Currently uses attenuated logic (may need to adapt this for different light types)
     * Assumes specific LightSource script has checked that this is within bounds of light
     */
    public virtual float GetIntensity(Vector3 _point)
    {
        float distance = Vector3.Distance(transform.position, _point);
        if (distance > light.range)
            return 0f;

        float normalised = distance / light.range;
        float attenuatedIntensity = 1f / (1f + (25f * normalised * normalised));

        return attenuatedIntensity * light.intensity;
    }

    public virtual List<GameObject> GetLitObjects()
    {
        return litObjects;
    }

    // TODO: remove - test method
    public virtual Info<List<Vector3>, List<Vector3>, List<bool>> GetTestRaycasts()
    {
        return testRaycasts;
    }
}
