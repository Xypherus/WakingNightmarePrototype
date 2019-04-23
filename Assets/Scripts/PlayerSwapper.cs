using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwapper : MonoBehaviour {

    public static PlayerSwapper playerSwapper;

    public float maxPlayerDistance;
    public List<PlayerController> players;
    public PlayerController currentPlayer;
    public FollowCamera followCam;

    private void Awake()
    {
        if (playerSwapper)
        {
            Destroy(playerSwapper);
            playerSwapper = this;
        }
        else { playerSwapper = this; }
    }

    // Use this for initialization
    void Start () {

        players = new List<PlayerController>();
        FindPlayers();
        ChangePlayer("Thomas");
	}
	
	// Update is called once per frame
	void Update () {
        followCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowCamera>();
        CheckPlayerListIntegrity();

        if (Time.timeScale > 0) {
            CheckPlayerDistances();

            if (Input.GetButtonDown("Swap")) {
                ChangeToNextPlayer();
            }

            if (Input.GetButtonDown("Call")) {
                CallOtherPlayers();
            }

            if (currentPlayer.stateMachine.incappacitated) {
                ChangeToNextPlayer();
            }
        }
	}

    void CheckPlayerListIntegrity() {
        foreach (PlayerController player in players) {
            if (player == null) {
                Debug.LogError("Fucking bullshit", this);
                FindPlayers();
                break;
            }
        }
    }

    void CheckPlayerDistances() {
        foreach (PlayerController player in players) {
            if (player == currentPlayer) { continue; }

            if (Vector2.Distance(player.transform.position, currentPlayer.transform.position) >= maxPlayerDistance)
            {
                Debug.DrawLine(player.transform.position, currentPlayer.transform.position, Color.red);
                if (Application.isPlaying)
                {
                    player.GetComponent<PlayerFearController>().currentFear = 100;
                }
            }
            else {
                Debug.DrawLine(player.transform.position, currentPlayer.transform.position, Color.blue);
            }
        }
    }

    void CallOtherPlayers() {
        foreach (PlayerController player in players) {
            if (player == currentPlayer) { continue; }

            PlayerAI ai = player.GetComponent<PlayerAI>();
            ai.ping = null;
            ai.target = currentPlayer.transform;
        }
    }

    public List<PlayerController> FindPlayers() {
        players = new List<PlayerController>();
        currentPlayer = null;
        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("Player")) {
            PlayerController stateMachine = playerObj.GetComponent<PlayerController>();
            if (stateMachine) {
                if (!players.Contains(stateMachine)) { players.Add(stateMachine); }
            }
        }

        ChangePlayer("Thomas");
        if (!currentPlayer && players.Count > 0) { currentPlayer = players[0]; }

        return players;
    }

    public void ChangePlayer(string playerName) {
        foreach (PlayerController player in players) {
            if (player.playerName == playerName) { currentPlayer = player; }
            else { player.pathing = true; }
        }

        currentPlayer.pathing = false;
        currentPlayer.GetComponent<PlayerAI>().ping = null;
        followCam.player = currentPlayer.transform;
    }

    public void ChangePlayer(int playerIndex) {
        for (int i = 0; i < players.Count; i++) {
            PlayerController player = players[i];

            if (i == playerIndex) { currentPlayer = player; }
            else { player.pathing = true; }
        }

        currentPlayer.pathing = false;
        followCam.player = currentPlayer.transform;
    }

    public void ChangeToNextPlayer() {
        int currentPlayerIndex = players.IndexOf(currentPlayer);
        int nextPlayerIndex = (int) Mathf.Repeat(currentPlayerIndex+1, players.Count);

        ChangePlayer(nextPlayerIndex);
    }

    public void OnDrawGizmos()
    {
        FindPlayers();
        if (players.Count > 0)
        {
            if (!currentPlayer)
            {
                currentPlayer = players[0];
            }

            CheckPlayerDistances();
        }
    }
}
