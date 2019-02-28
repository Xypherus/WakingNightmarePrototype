using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerStateMachine : CharacterStateNetwork {

    public bool isGrounded;

    public PlayerStateMachine() {
        //TODO: create an instance of every PlayerCharacterState
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    class PlayerWalking : PlayerCharacterState {
        PlayerController player;

        public PlayerWalking(CharacterStateNetwork network, PlayerController player) : base("Player Walking", network)
        { this.player = player; }

        public override void Subject()
        {
            if (!player.isGrounded && player.rigidbody.velocity.y > 0) { Transition("Player Rising"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y < 0) { Transition("Player Falling"); }
            else if (player.isGrounded && player.isProne) { Transition("Player Crawling"); }
            else if (player.grabbed && player.LedgeNearby()) { Transition("Player On Ledge"); }
            else if (player.grabbed && player.LadderNearby()) { Transition("Player On Ladder"); }
        }

        public override void Update() {
            
        }
    }
}

public class PlayerCharacterState : CharacterState {
    public PlayerCharacterState(string name, CharacterStateNetwork network) : base(name,network) { }

    public override void Update()
    {
        throw new System.NotImplementedException();
    }
    public override void OnStateExit()
    {
        throw new System.NotImplementedException();
    }
    public override void Subject()
    {
        throw new System.NotImplementedException();
    }
}
