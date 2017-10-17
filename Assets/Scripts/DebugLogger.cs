using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLogger : MonoBehaviour
{

    private KeyCode playerLogCode = KeyCode.P;
    private KeyCode continuousLogCode = KeyCode.L;

    private bool loggingPlayer;
    private string lastDebugString;
    private GameObject player;
    private Rigidbody playerRb;

	// Use this for initialization
	void Start ()
	{
	    loggingPlayer = false;
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            Debug.LogError("no player object found");
	    playerRb = player.GetComponent<Rigidbody>();

        // print logger control instructions
        Debug.Log("P : Log player stats at current frame");
	    Debug.Log("P+L : Turn on continuous player logging");
    } 
	
	// Update is called once per frame
    void Update()
    {
        PlayerLog();
    }

    private void PlayerLog()
    {
        if (loggingPlayer)
        {
            LogPlayerStats();

            if (Input.GetKeyDown(playerLogCode))
                loggingPlayer = false;
        }
        else
        {
            if (Input.GetKey(playerLogCode) && Input.GetKey(continuousLogCode))
            {
                loggingPlayer = true;
                return;
            }

            if (Input.GetKeyUp(playerLogCode))
                LogPlayerStats();
        }
    }

    private void LogPlayerStats()
    {
        // print/log position velocity etc
        string playerDebugString = "Player: ";

        playerDebugString += "Pos " + player.transform.position + " ";
        playerDebugString += "Vel " + playerRb.velocity + " ";
        playerDebugString += "Spd " + playerRb.velocity.magnitude.ToString("F2") + " ";

        float localXZSpeed = Vector3.ProjectOnPlane(playerRb.velocity, GlobalGravityControl.GetCurrentGravityVector()).magnitude;
        float localYSpeed = Vector3.Project(playerRb.velocity, GlobalGravityControl.GetCurrentGravityVector()).magnitude;

        playerDebugString += "lclXZ " + localXZSpeed.ToString("F2") + " ";
        playerDebugString += "lclY " + localYSpeed.ToString("F2") + " ";

        // dont print new line if identical to last line
        if (playerDebugString == lastDebugString)
            return;

        Debug.Log(playerDebugString);
        lastDebugString = playerDebugString;
    }
}
