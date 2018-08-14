using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideWithWaveShader : MonoBehaviour
{
    public WaveShaderPositionTracker wavePositionTracker;
    public LayerMask rayLayerMask;
    public float skinThickness;

    private Rigidbody rb;
    
	void Start ()
	{
	    rb = GetComponent<Rigidbody>();
	}
	
	void FixedUpdate () {
		// raycast in direction of velocity and apply force away from sand if within skin range
	}

    void OnTriggerStay()
    {
        // if below sand level at position, apply upward force to bring to surface
    }
}
