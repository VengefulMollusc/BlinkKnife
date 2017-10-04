using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class MissileController : MonoBehaviour {

    [Header("General Settings")]
    [SerializeField]
    private float speed = 30f;
 //   [SerializeField]
	//private float turningStrength = 0.9f;
    [SerializeField]
    [Range(0f, 1f)]
    private float turningStrength = 0.2f;
    private float maxTurningAngle;
    [SerializeField]
	private float startLife = 5f;
	private float life;

    float homingCutoffDist = 5f;

    [Header("Particle Systems")]
	[SerializeField]
	private ParticleSystem explosion;
	[SerializeField]
	private ParticleSystem rocketTrail;

    [Header("Random Spin")]
    [SerializeField]
    private float randomXYRot = 0.8f;
    [SerializeField]
    private float randomZRotMin = 1f;
    [SerializeField]
    private float randomZRotMax = 5f;

    private Rigidbody rb;

	private Vector3 spinVector;
    private float zSpin;

    private Transform target;
    private Transform targetBackup;

	private bool collided = false;

    private bool launched = false;

    private float accuracyIncreaseThreshold;

    private UIMarker uiMarker;
    private Vector2 markerOnScreenSize;

    // target prediction variables
    private Vector3 targetLastPosition;

    // explosion variables
    private const float expRadius = 5f;
    private const float expDamage = 10f;
    private const float expForce = 10f;
    private const float directDamage = 30f;

    private bool useUiMarker = true;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
		life = startLife;

        // setup ui marker
        uiMarker = GetComponent<UIMarker>();
        if (uiMarker == null)
            useUiMarker = false;

        maxTurningAngle = speed * turningStrength;

        // Needs to be consistent spin, but less variation, then can tweak accuracythreshold etc.
        // so values 2-4 instead of 1-5?
        zSpin = Random.Range(randomZRotMin, randomZRotMax);
        //spinVector = new Vector3 (Random.Range(-randomXYRot, randomXYRot), Random.Range(-randomXYRot, randomXYRot), 0f);
        spinVector = new Vector3(Random.Range(-randomXYRot, randomXYRot), Random.Range(-randomXYRot, randomXYRot), zSpin);
	}

    public void Setup(Transform _parent, Transform _target, Collider _collider)
    {
        transform.SetParent(_parent);
        target = _target;
        targetBackup = target;
        Physics.IgnoreCollision(GetComponent<Collider>(), _collider);

        float distToTarget = Vector3.Distance(transform.position, target.position);
        accuracyIncreaseThreshold = distToTarget * 0.8f; // this could be a set value? stop massive spread at distance
        // or clamp maximum value? 50? 100?
        // this would do even weirder things to long distance, much higher multiplier - need to clamp that as well
    }

    public void Fire()
    {
        if (useUiMarker)
        {
            markerOnScreenSize = uiMarker.GetOnScreenImage().rectTransform.sizeDelta;
            uiMarker.GetOnScreenImage().rectTransform.sizeDelta *= 0.2f;
            uiMarker.EnableMarker(true);
        }

        transform.SetParent(null);
		gameObject.layer = 0;
        launched = true;
        rb.isKinematic = false;

        if (target != null)
        {
            targetLastPosition = target.position;
        }

        rocketTrail.Play ();
    }

    void UpdateUIMarker()
    {
        if (!useUiMarker)
            return;

        float distToCamera = Vector3.Distance(transform.position, Camera.main.transform.position);
        // update ui marker
        float uiTransitionNormalised = Mathf.Clamp(distToCamera, 0f, 50f) / 50f;
        uiMarker.GetOnScreenImage().rectTransform.sizeDelta = Vector2.Lerp(markerOnScreenSize, markerOnScreenSize * 0.2f, uiTransitionNormalised);
        //if (uiTransitionNormalised > 0.8f)
        //    uiTransitionNormalised = 0.8f;
        //uiMarker.FadeGB(uiTransitionNormalised);
        //uiMarker.GetOnScreenImage().rectTransform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z * 2f);
    }

    // Update is called once per frame
    void FixedUpdate () {
        // check target - switch to knife
        // this will need to be reworked
        CheckTarget();

		if (!launched || collided) return;

        if (target != null) {
            float distToTarget = Vector3.Distance(transform.position, target.position);
            if (distToTarget > homingCutoffDist)
            {
				RotateToTarget ();
			} else if (Utilities.IsInFront(transform, target)){
                // remove target when inside minimum range
                // and passed target
                target = null;
                targetBackup = null;
            }

            // spin
            float i = Utilities.MapValues(distToTarget, 0f, accuracyIncreaseThreshold, 0f, 1f);
            transform.Rotate(spinVector * i);
        } else
        {
            transform.Rotate(spinVector * 0.2f);
        }

        // Ambient z-axis rotation
        //transform.Rotate(0f, 0f, zSpin);

        //Move
        rb.velocity = transform.forward * speed;

        // check life
        life -= Time.deltaTime;
		if (life <= 0)
			Explode();
    }

    private void CheckTarget()
    {
        //if (target == null)
        //    return;

        GameObject knife = GameObject.FindGameObjectWithTag("TargetOverride");
        if (knife != null)
        {
            target = knife.transform;
        } else
        {
            target = targetBackup;
        }
    }

    private void RotateToTarget()
    {
        /*
         * Predicting the position here makes missiles almost too accurate, even while dodging.
         * whereas just working off the target position makes them easily avoided
         * 
         * there needs to be some tweaking here, maybe with the turning speed too
         * 
         */
        Vector3 aimPosition = PredictTargetPosition();
        //Vector3 aimPosition = target.position;
        Quaternion targetRotation = Quaternion.LookRotation(aimPosition - transform.position);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxTurningAngle);

        
        // testing turning strength
        //float angle = Quaternion.Angle(transform.rotation, targetRotation);
        //float str = Utilities.MapValues(angle, 0f, 180f, turningStrength, 0.01f);
        //transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, str);
    }

    /*
     * Predicts the collision point with the target based on time to collision and
     * the current movement direction of the target
     * 
     * NOTE: predicting target position will lead to some weird behaviour when 
     * throwing knife/stealing target - missiles will aim for waaaaay ahead of knife
     * due to speed of throw. Maybe add speed cap to prediction? will also allow for 
     * outrunning missiles by exceeding cap
     */
    private Vector3 PredictTargetPosition()
    {
        float distToTarget = Vector3.Distance(transform.position, target.position);
        Vector3 targetMovementVector = (target.position - targetLastPosition)/Time.fixedDeltaTime;
        float timeToTarget = distToTarget / speed;

        // cap targetMovementVector magnitude here

        Vector3 prediction = target.position + (targetMovementVector * timeToTarget);

        targetLastPosition = target.position;
        return prediction;
    }

    private void Explode()
    {
        // detach rocket trail
        rocketTrail.Stop();
        rocketTrail.transform.SetParent(null);
        Destroy(rocketTrail.gameObject, rocketTrail.duration);

        // freeze position
        rb.isKinematic = true;
		collided = true;

		// hide geometry
		foreach (Renderer r in GetComponentsInChildren<Renderer>()){
			if (r != GetComponent<Renderer> ()) {
				r.enabled = false;
			}
		}

		// explode
		explosion.Play ();

        // damage and apply explosion force to objects
        // add gravity down vector to boost upward force on explosion
        Utilities.CreateExplosion(transform.position + GlobalGravityControl.GetCurrentGravityVector(), 
            expRadius, expDamage, expForce);

        // destroy ui marker
        uiMarker.DestroyMarker();

		// destroy after exploding
		Destroy(gameObject, explosion.duration);
    }

    private void OnCollisionEnter(Collision col)
    {
        if (!launched) return;
        Explode();

        HealthController hc = col.gameObject.GetComponent<HealthController>();
        if (hc != null)
        {
            hc.Damage(directDamage);
        }
    }
}
