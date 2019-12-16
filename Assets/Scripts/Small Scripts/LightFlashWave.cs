using UnityEngine;

public class LightFlashWave : MonoBehaviour {

	// Properties
	[SerializeField]
	private string waveFunction = "sin"; // possible values: sin, tri(angle), sqr(square), saw(tooth), inv(verted sawtooth), noise (random)
	[SerializeField]
	private float baseValue = 0.0f; // start
	[SerializeField]
	private float amplitude = 1.0f; // amplitude of the wave
	[SerializeField]
	private float phase = 0.0f; // start point inside on wave cycle
	[SerializeField]
	private float frequency = 0.5f; // cycle frequency per second

	// get the light
	public GameObject lightObject;
	private Light lightComponent;

	// Keep a copy of the original color
	private Color originalColor;

	// Store the original color
	void Start () {
		lightComponent = lightObject.GetComponent<Light>();

		if (lightComponent == null){
			Debug.LogError ("No light object found");
		}

		originalColor = lightComponent.color;
	}

	void Update () {
		lightComponent.color = originalColor * (EvalWave());
	}

	private float EvalWave () {
		float x = (Time.time + phase)*frequency;
		float y;

		x = x - Mathf.Floor(x); // normalized value (0..1)

		if (waveFunction=="sin") {
			y = Mathf.Sin(x*2*Mathf.PI);
		}
		else if (waveFunction=="tri") {
			if (x < 0.5f)
				y = 4.0f * x - 1.0f;
			else
				y = -4.0f * x + 3.0f;  
		}    
		else if (waveFunction=="sqr") {
			if (x < 0.5f)
				y = 1.0f;
			else
				y = -1.0f;  
		}    
		else if (waveFunction=="saw") {
			y = x;
		}    
		else if (waveFunction=="inv") {
			y = 1.0f - x;
		}    
		else if (waveFunction=="noise") {
			y = 1f - (Random.value*2f);
		}
		else {
			y = 1.0f;
		}        
		return (y*amplitude)+baseValue;     
	}
}
