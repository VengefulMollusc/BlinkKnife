using UnityEngine;

[RequireComponent(typeof(SoftSurface))]
public class GravityPanel : MonoBehaviour
{
    /*
     * Attached to GameObjects that trigger gravity shifts when hit by knife
     */

    [SerializeField]
    private bool customGravity = false;
    [SerializeField]
    private Vector3 customGravVector;

    [SerializeField]
    private bool useSurfaceNormal = false;

    /*
     * Return the gravity vector this panel will shift to
     */
    public Vector3 GetGravityVector()
    {
        if (useSurfaceNormal) return Vector3.zero;
        if (customGravity) return customGravVector;
        return -transform.up;
    }
}
