using UnityEngine;
using System.Collections;

public class CircleRenderer : MonoBehaviour {

	[SerializeField]
	private int segments;
	[SerializeField]
	private float radius;

	private LineRenderer line;

	// Use this for initialization
	void Start () {
		line = gameObject.GetComponent<LineRenderer> ();

		line.positionCount = segments + 1;
		line.useWorldSpace = false;
	}
	
	// Update is called once per frame
	void Update () {
		CreatePoints ();
	}

	void CreatePoints (){
		float x;
		float y;
		float z = 2f;

		float angle = 0f;

		for (int i = 0; i < (segments + 1); i++){
			x = Mathf.Sin (Mathf.Deg2Rad * angle);
			y = Mathf.Cos (Mathf.Deg2Rad * angle);
			line.SetPosition (i, new Vector3 (x, y, z) * radius);
			angle += (360f / segments);
		}
	}
}
