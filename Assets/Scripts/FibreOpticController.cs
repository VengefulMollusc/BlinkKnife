using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEngine;

public class FibreOpticController : MonoBehaviour
{
    [SerializeField] private FibreOpticController otherEndFibreOpticController;
    [SerializeField] private Transform bezierTargetTransform;

    private float transitionTime = 2f; // TODO: replace with actual calculation based on overall bezier length

    /*
     * TODO: Need a method to designate one end as 'primary' controller
     * Primary controller will control drawing of fibre optic geometry etc
     */

    // Use this for initialization
    void OnEnable () {
		if (bezierTargetTransform != null)
            transform.LookAt(bezierTargetTransform);
        else 
            Debug.LogError("No Bezier Target Transform Given");

	    if (IsConnected() && !otherEndFibreOpticController.IsConnected())
	        otherEndFibreOpticController.SetOtherEndController(this);

        this.AddObserver(OnFibreOpticWarp, KnifeController.FibreOpticWarpNotification);
	}

    void OnDisable()
    {
        this.RemoveObserver(OnFibreOpticWarp, KnifeController.FibreOpticWarpNotification);
    }

    /*
     * Handles start of fibre optic warp and triggers animation transition of knife along bezier
     */ 
    void OnFibreOpticWarp(object sender, object args)
    {
        if (!IsConnected())
        {
            Debug.LogError("Not connected to other FibreOpticController");
            return;
        }

        Info<GameObject, Transform> info = (Info<GameObject, Transform>) args;
        Transform knifeTransform = info.arg1;

        StartCoroutine(TransitionKnife(knifeTransform));
    }

    // Transitions the knife along the bezier
    private IEnumerator TransitionKnife(Transform _knifeTransform)
    {
        float t = 1f;
        while (t > 0)
        {
            t -= Time.deltaTime * (Time.timeScale / transitionTime);
            _knifeTransform.position = GetBezierPosition(t);

            yield return 0;
        }
    }

    /*
     * Lerps along the bezier defined by both FibreOpticControllers and the bezier targets
     * 
     * allows bezier control point movement during transition
     */
    public Vector3 GetBezierPosition(float _t)
    {
        return Utilities.LerpBezier(transform.position, 
            bezierTargetTransform.position,
            otherEndFibreOpticController.GetBezierTargetPosition(), 
            otherEndFibreOpticController.transform.position,
            _t);
    }

    //public Quaternion GetInitialRotation()
    //{
    //    Vector3 forwardRelToGravity =
    //        Vector3.ProjectOnPlane(transform.forward, GlobalGravityControl.GetCurrentGravityVector());

    //    return Quaternion.LookRotation(forwardRelToGravity.normalized, -GlobalGravityControl.GetCurrentGravityVector());
    //}

    // used so player can warp at the right speed
    public float GetTransitionTime()
    {
        return transitionTime;
    }

    // Used so that other end can get target position for bezier calculation
    public Vector3 GetBezierTargetPosition()
    {
        return bezierTargetTransform.position;
    }

    // Used to make sure that both ends are connected properly
    public void SetOtherEndController(FibreOpticController _other)
    {
        otherEndFibreOpticController = _other;
    }

    // returns true if this controller has a reference to the controller at the other end
    public bool IsConnected()
    {
        return otherEndFibreOpticController != null;
    }
}
