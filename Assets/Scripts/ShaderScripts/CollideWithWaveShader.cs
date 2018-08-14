using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideWithWaveShader : MonoBehaviour
{
    public LayerMask rayLayerMask;
    public float skinThickness;

    private Rigidbody rb;
    
	void Start ()
	{
	    rb = GetComponent<Rigidbody>();
	}
	
	void FixedUpdate () {
		
	}

    void OnTriggerStay()
    {

    }
}
