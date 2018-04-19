using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEngine;

public class FibreOpticController : MonoBehaviour
{
    [SerializeField] private FibreOpticController otherEndFibreOpticController;
    [SerializeField] private Transform bezierTargetTransform;

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
        FibreOpticController fibreOpticController = (FibreOpticController)args;
        if (fibreOpticController != this)
            return;

        if (!IsConnected())
        {
            Debug.LogError("Not connected to other FibreOpticController");
            return;
        }

        Debug.Log("Fibre Optic warp effect here");


        //Info<GameObject, KnifeController> info = (Info<GameObject, KnifeController>)args;
        //KnifeController knifeController = info.arg1;

        // stick knife into other end. Should allow lookahead colliders to do their thing
        //knifeController.StickToSurface(otherEndFibreOpticController.transform.position, -otherEndFibreOpticController.transform.forward, otherEndFibreOpticController.gameObject, true);


        //StartCoroutine(TransitionKnife(knifeTransform));
    }

    public void WarpKnife(KnifeController _knifeController)
    {
        if (!IsConnected())
        {
            Debug.LogError("Not connected to other FibreOpticController");
            return;
        }

        // stick knife into other end. Should allow lookahead colliders to do their thing
        _knifeController.StickToSurface(otherEndFibreOpticController.transform.position, -otherEndFibreOpticController.transform.forward, otherEndFibreOpticController.gameObject, true);
    }

    // Transitions the knife along the bezier
    //private IEnumerator TransitionKnife(Transform _knifeTransform)
    //{
    //    float t = 1f;
    //    while (t > 0)
    //    {
    //        t -= Time.deltaTime * (Time.timeScale / transitionTime);
    //        _knifeTransform.position = GetBezierPosition(t);

    //        yield return 0;
    //    }
    //}

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
    public float GetDuration()  // TODO: replace with actual calculation based on overall bezier length
    {
        return 2f;
    }

    // Used so that other end can get target position for bezier calculation
    public Vector3 GetBezierTargetPosition()
    {
        return bezierTargetTransform.position;
    }

    public Vector3 GetStartPosition()
    {
        return transform.position;
    }

    // Used to make sure that both ends are connected properly
    public void SetOtherEndController(FibreOpticController _other)
    {
        otherEndFibreOpticController = _other;
    }

    public Vector3 GetEndPosition()
    {
        return otherEndFibreOpticController.transform.position;
    }

    // Get rotations for aligning transition camera
    public Quaternion GetStartRotation()
    {
        return transform.rotation;
    }
    
    public Quaternion GetEndRotation()
    {
        return Quaternion.AngleAxis(180, otherEndFibreOpticController.transform.up) * otherEndFibreOpticController.transform.rotation;
    }

    // returns true if this controller has a reference to the controller at the other end
    public bool IsConnected()
    {
        return otherEndFibreOpticController != null;
    }
}
