using System.Collections;
using UnityEngine;

public class JumpCollider : MonoBehaviour
{
    private static bool colliding;

    private float colExitDelay = 0.1f; 
    private Coroutine colExitCoroutine;

    void Start()
    {
        colliding = false;

        // TODO: replace with collision layers
        Utilities.IgnoreCollisions(GetComponent<Collider>(), GameObject.FindGameObjectWithTag("Player").GetComponents<Collider>(), true);
    }

    // Static method to check if player is 'grounded'
    public static bool IsColliding()
    {
        return colliding;
    }

    // called when the player jumps - forces colliding to false
    public static void Jump()
    {
        colliding = false;
    }

    // Collision methods
    void OnTriggerStay(Collider col)
    {
        if (col.isTrigger)
            return;

        if (colExitCoroutine != null)
            StopCoroutine(colExitCoroutine);

        colliding = true;
    }

    void OnTriggerExit(Collider col)
    {
        if (col.isTrigger)
            return;

        if (colExitCoroutine != null)
            StopCoroutine(colExitCoroutine);

        // Start delay coroutine
        colExitCoroutine = StartCoroutine(ColExitDelay());
    }

    // Delays switching colliding to false
    // Allows the player a little more safety when jumping off a ledge etc
    IEnumerator ColExitDelay()
    {
        float t = 0;
        while (t < colExitDelay)
        {
            t += Time.deltaTime;
            yield return 0;
        }

        colliding = false;
    }
}
