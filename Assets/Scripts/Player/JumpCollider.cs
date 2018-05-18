using System.Collections;
using UnityEngine;

public class JumpCollider : MonoBehaviour
{
    private static bool colliding;

    private float colExitDelay = 0.1f; 
    private Coroutine colExitCoroutine;

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
        yield return new WaitForSeconds(colExitDelay);

        colliding = false;
    }
}
