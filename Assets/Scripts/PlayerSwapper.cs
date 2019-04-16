using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwapper : MonoBehaviour {

    public static PlayerSwapper playerSwapper;

    public List<PlayerController> players;
    public PlayerController currentPlayer;
    new public FollowCamera camera;

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
        camera = FindObjectOfType<FollowCamera>();

        players = new List<PlayerController>();
        FindPlayers();
        ChangePlayer("Thomas");
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.timeScale > 0) {
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

    void CallOtherPlayers() {
        foreach (PlayerController player in players) {
            if (player == currentPlayer) { continue; }

            PlayerAI ai = player.GetComponent<PlayerAI>();
            ai.ping = null;
            ai.target = currentPlayer.transform;
        }
    }

    public void FindPlayers() {
        foreach (GameObject playerObj in GameObject.FindGameObjectsWithTag("Player")) {
            PlayerController stateMachine = playerObj.GetComponent<PlayerController>();
            if (stateMachine) {
                if (!players.Contains(stateMachine)) { players.Add(stateMachine); }
            }
        }
    }

    public void ChangePlayer(string playerName) {
        foreach (PlayerController player in players) {
            if (player.name == playerName) { currentPlayer = player; }
            else { player.pathing = true; }
        }

        currentPlayer.pathing = false;
        currentPlayer.GetComponent<PlayerAI>().ping = null;
        camera.player = currentPlayer.transform;
    }

    public void ChangePlayer(int playerIndex) {
        for (int i = 0; i < players.Count; i++) {
            PlayerController player = players[i];

            if (i == playerIndex) { currentPlayer = player; }
            else { player.pathing = true; }
        }

        currentPlayer.pathing = false;
        camera.player = currentPlayer.transform;
    }

    public void ChangeToNextPlayer() {
        int currentPlayerIndex = players.IndexOf(currentPlayer);
        int nextPlayerIndex = (int) Mathf.Repeat(currentPlayerIndex+1, players.Count);

        ChangePlayer(nextPlayerIndex);
    }
}
