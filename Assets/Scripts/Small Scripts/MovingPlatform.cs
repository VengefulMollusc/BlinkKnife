using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 50f;

    [SerializeField] private Vector3 startPos = Vector3.zero;
    [SerializeField] private Vector3 endPos = Vector3.zero;

    private float t = -90f;

    private Rigidbody rb;

    void OnEnable()
    {
        if (startPos == endPos)
            Debug.LogError("Start and end positions are the same");

        transform.position = startPos;

        rb = GetComponent<Rigidbody>();
    }

	// Update is called once per frame
	void Update () {
	    t += Time.deltaTime * moveSpeed;

	    float lerpPercent = (Mathf.Sin(Mathf.Deg2Rad * t) + 1) / 2f;

        //Debug.Log(lerpPercent);

        rb.MovePosition(Vector3.Lerp(startPos, endPos, lerpPercent));
    }
}
