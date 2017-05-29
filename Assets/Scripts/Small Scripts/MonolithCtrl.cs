using UnityEngine;
using System.Collections;

public class MonolithCtrl : MonoBehaviour {

	[SerializeField]
	private float rotateSpeed = 2f;
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (0f, Time.fixedDeltaTime * rotateSpeed, 0f);
	}
}
