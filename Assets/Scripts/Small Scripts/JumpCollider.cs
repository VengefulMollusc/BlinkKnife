using UnityEngine;

public class JumpCollider : MonoBehaviour {

	[SerializeField]
	private PlayerMotor playerMotor;

	void Start (){
		if (playerMotor == null){
			Debug.LogError ("No PlayerMotor given for JumpCollider.");
		}
	}

	void OnTriggerStay (Collider col){
		if (!col.CompareTag("Player")){
			playerMotor.SetOnGround (true);
		}

        // This will need to be changed to accomodate not all things being tagged scenery
        // possibly use layers?
        if (col.CompareTag("Scenery") || col.CompareTag("GravityPanel"))
        {
            playerMotor.transform.SetParent(col.transform);
        }
    }

	void OnTriggerExit (Collider col){
		if (col.CompareTag("Scenery")){
			playerMotor.transform.SetParent (null);
		}
	}

    private void OnTriggerEnter(Collider col)
    {
        // added to prevent errors with DontGoThroughThings
    }

}
