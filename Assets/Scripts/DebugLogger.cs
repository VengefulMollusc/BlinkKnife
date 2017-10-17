using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLogger : MonoBehaviour
{

    private KeyCode playerLogCode = KeyCode.P;
    private KeyCode continuousLogCode = KeyCode.L;

    private bool loggingPlayer;

    private GameObject player;
    private Rigidbody playerRb;
    private PlayerMotor playerMotor;

	// Use this for initialization
	void Start ()
	{
	    loggingPlayer = false;
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            Debug.LogError("no player object found");
	    playerMotor = player.GetComponent<PlayerMotor>();
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

        Debug.Log(playerDebugString);
    }
}
