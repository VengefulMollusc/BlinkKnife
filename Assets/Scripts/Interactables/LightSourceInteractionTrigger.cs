using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LightSource))]
public class LightSourceInteractionTrigger : InteractionTrigger
{
    private LightSource lightSource;

    private GameObject targetObject;
    private bool targetIsLit;
    
    void Start()
    {
        lightSource = GetComponent<LightSource>();

        // Defaults to player as target
        if (targetObject == null)
            targetObject = GameObject.FindGameObjectWithTag("Player");
    }

    void OnEnable()
    {
        InvokeRepeating("CheckForObject", 0f, GlobalVariableController.LightCheckUpdateFrequency);
    }

    void OnDisable()
    {
        CancelInvoke("CheckForObject");
    }

    /*
     * Checks if the target object is in the list of objects lit by this light source
     * Activates if lit state has changed
     */
    void CheckForObject()
    {
        List<GameObject> litObjects = lightSource.GetLitObjects();

        bool lit = litObjects.Contains(targetObject);

        if (lit != targetIsLit)
        {
            targetIsLit = lit;
            ActivateTriggers(targetIsLit);
        }
    }
}
