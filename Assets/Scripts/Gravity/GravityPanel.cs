using UnityEngine;
using System.Collections;

public class GravityPanel : MonoBehaviour {

    [SerializeField]
    private bool customGravity = false;
    [SerializeField]
    private Vector3 customGravVector;

    [SerializeField]
    private bool useSurfaceNormal = false;

    /*
     * return the gravity vector this panel will shift to
     */
	public Vector3 GetGravityVector()
    {
        if (useSurfaceNormal) return Vector3.zero;
        if (customGravity) return customGravVector;
        return -transform.up;
    }
}
