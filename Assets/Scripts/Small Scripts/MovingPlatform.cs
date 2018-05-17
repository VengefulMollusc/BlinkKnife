using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private float moveDuration = 10f;

    private Vector3 startPos;
    [SerializeField] private Vector3 endPos;

    private bool transitionToEnd = true;
    private float t;

    void Start()
    {
        startPos = transform.position;
    }
    
	void FixedUpdate ()
	{
	    t += (transitionToEnd ? Time.fixedDeltaTime : -Time.fixedDeltaTime) / moveDuration;

	    Vector3 newPos = Vector3.Lerp(startPos, endPos, t);
        
        Info<Transform, Vector3> info = new Info<Transform, Vector3>(transform, newPos - transform.position);
        //rb.MovePosition(newPos); // this calculates velocity as well, so friction(?) adds to relative movement. TODO: find a solution for relativemovement on something with a velocity
        transform.position = newPos;

        // Send notification with movementVector here
        this.PostNotification(RelativeMovementController.RelativeMovementNotification, info);

        if (transitionToEnd ? t > 1f : t < 0f)
	    {
	        transitionToEnd = !transitionToEnd;
	    }
    }
}
