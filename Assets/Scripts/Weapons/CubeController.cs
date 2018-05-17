using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CubeController : MonoBehaviour {

    [SerializeField]
    private float speed = 1f;

    [SerializeField]
    private float maxScale = 10f;
    [SerializeField]
    private float scaleDuration = 1f;

    private Rigidbody rb;
    private Collider[] playerColliders;
    private bool activated;
    
	//void Start () {
 //       rb = GetComponent<Rigidbody>();
	//}

    public void Setup(Transform _camera, Collider[] _playerCol, Transform _target)
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = _camera.forward * speed;
        playerColliders = _playerCol;
        //Utilities.IgnoreCollisions(GetComponent<Collider>(), playerColliders, true);

        if (_target != null)
        {
            Activate(_target);
        }
    }
	
	void FixedUpdate () {
        // spin?
	}

    public void Toggle()
    {
        if (!activated) Activate(null);
        else Deactivate();
    }

    private void Activate(Transform _target)
    {
        // align with face normal and push all the way out of surface?

        if (_target != null)
        {
            transform.SetParent(_target.parent);
        }
        //Utilities.IgnoreCollisions(GetComponent<Collider>(), playerColliders, false);
        activated = true;
        rb.isKinematic = true;
        transform.up = Vector3.up;
        //transform.localScale = new Vector3(0f, 0f, 0f);
        StartCoroutine(ScaleUp());
    }

    IEnumerator ScaleUp()
    {
        float t = 0.0f;
        while (t < 1f)
        {
            t += Time.fixedDeltaTime * (Time.timeScale / scaleDuration);
            float scale = Mathf.Lerp(transform.localScale.x, maxScale, t);
            transform.localScale = new Vector3(scale, scale, scale);
            yield return 0;
        }
    }

    private void Deactivate()
    {
        StartCoroutine(ScaleDown());
    }

    IEnumerator ScaleDown()
    {
        float t = 0.0f;
        while (t < 1f)
        {
            t += Time.fixedDeltaTime * (Time.timeScale / scaleDuration);
            float scale = Mathf.Lerp(transform.localScale.x, 0, t);
            transform.localScale = new Vector3(scale, scale, scale);
            yield return 0;
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision col)
    {
        if (transform.parent == null && !col.gameObject.CompareTag("Player"))
        {
            transform.SetParent(col.gameObject.transform);
        }
        if (activated) return;
        Activate(null);
    }
}
