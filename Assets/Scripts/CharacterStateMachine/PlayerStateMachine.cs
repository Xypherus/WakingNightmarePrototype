using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : CharacterStateNetwork {
    public PlayerController player;

	// Use this for initialization
	void Start () {
        player = GetComponent<PlayerController>();

        PlayerWalking walking = new PlayerWalking(this, player);
        PlayerCrawling crawling = new PlayerCrawling(this, player);
        PlayerJumping jumping = new PlayerJumping(this, player, 0.5f);
        PlayerOnLadder onLadder = new PlayerOnLadder(this, player);
        PlayerOnLedge onLedge = new PlayerOnLedge(this, player);
        PlayerRising rising = new PlayerRising(this, player);
        PlayerFalling falling = new PlayerFalling(this, player);

        walking.AddTransition(crawling);
        walking.AddTransition(falling);
        walking.AddTransition(rising);
        walking.AddTransition(jumping);
        walking.AddTransition(onLadder);
        walking.AddTransition(onLedge);

        crawling.AddTransition(walking);
        crawling.AddTransition(rising);
        crawling.AddTransition(falling);
        crawling.AddTransition(onLedge);
        crawling.AddTransition(onLadder);

        jumping.AddTransition(rising);
        jumping.AddTransition(falling);
        jumping.AddTransition(onLadder);
        jumping.AddTransition(onLedge);
        jumping.AddTransition(walking);

        onLadder.AddTransition(jumping);
        onLadder.AddTransition(falling);

        onLedge.AddTransition(jumping);
        onLedge.AddTransition(falling);

        rising.AddTransition(onLedge);
        rising.AddTransition(onLadder);
        rising.AddTransition(falling);
        rising.AddTransition(walking);

        falling.AddTransition(onLedge);
        falling.AddTransition(onLadder);
        falling.AddTransition(rising);
        falling.AddTransition(walking);

    }

    #region Player States
    class PlayerWalking : PlayerCharacterState {
        PlayerController player;

        public PlayerWalking(CharacterStateNetwork network, PlayerController player) : base("Player Walking", network)
        { this.player = player; }

        public override void Subject()
        {
            //TODO: add a state condition for death when fear is implemented
            if (player.isGrounded && player.jumpped) { Transition("Player Jumping"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y > 0) { Transition("Player Rising"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y < 0) { Transition("Player Falling"); }
            else if (player.isGrounded && player.isProne) { Transition("Player Crawling"); }
            else if (player.grabbed && player.LedgeNearby()) { Transition("Player On Ledge"); }
            else if (player.grabbed && player.LadderNearby()) { Transition("Player On Ladder"); }
        }

        public override void Update() {
            player.rigidbody.AddForce(new Vector2(Mathf.Clamp(Input.GetAxis("Horizontal")*player.accelMultiplier, -1, 1) * player.speed, 0f));

            player.Decelerate();
        }
    }
    class PlayerCrawling : PlayerCharacterState
    {
        PlayerController player;

        public PlayerCrawling(CharacterStateNetwork network, PlayerController player) : base("Player Crawling", network)
        { this.player = player; }

        public override void Subject()
        {
            //TODO: add a state condition for death when fear is implemented
            if (!player.isGrounded && player.rigidbody.velocity.y > 0) { Transition("Player Rising"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y < 0) { Transition("Player Falling"); }
            else if (player.isGrounded && !player.isProne) { Transition("Player Walking"); }
            else if (player.grabbed && player.LedgeNearby()) { Transition("Player On Ledge"); }
            else if (player.grabbed && player.LadderNearby()) { Transition("Player On Ladder"); }
        }

        public override void OnStateEnter()
        {
            player.SetSize(new Vector2(player.width, player.crouchHeight));
        }

        public override void Update()
        {
            player.rigidbody.AddForce(new Vector2(Mathf.Clamp(Input.GetAxis("Horizontal") * player.accelMultiplier, -1, 1) * player.crouchSpeed, 0f));

            player.Decelerate();
        }

        public override void OnStateExit()
        {
            player.SetSize(new Vector2(player.width, player.height));
        }
    }
    class PlayerJumping : PlayerCharacterState
    {
        PlayerController player;
        float elapsedTime;
        float maxTime;

        public PlayerJumping(CharacterStateNetwork network, PlayerController player, float maxTime) : base("Player Jumping", network)
        { this.player = player; this.maxTime = maxTime; }

        public override void Subject()
        {
            //TODO: add a state condition for death when fear is implemented
            if (player.isGrounded) { Transition("Player Walking"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y > 0 && elapsedTime >= maxTime) { Transition("Player Rising"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y < 0 && elapsedTime >= maxTime) { Transition("Player Falling"); }
            else if (player.grabbed && player.LedgeNearby()) { Transition("Player On Ledge"); }
            else if (player.grabbed && player.LadderNearby()) { Transition("Player On Ladder"); }
        }

        public override void OnStateEnter()
        {
            player.rigidbody.AddForce(new Vector2(Input.GetAxis("Horizontal") * (player.jumpForce / 2), player.jumpForce));
        }

        public override void Update()
        {
            elapsedTime += Time.deltaTime;
        }

        public override void OnStateExit()
        {
            elapsedTime = 0f;
        }
    }
    class PlayerOnLadder : PlayerCharacterState
    {
        PlayerController player;

        public PlayerOnLadder(CharacterStateNetwork network, PlayerController player) : base("Player On Ladder", network)
        { this.player = player; }

        public override void Subject()
        {
            //TODO: add a state condition for death when fear is implemented
            if (player.grabbed || !player.ladder) { Transition("Player Falling"); }
            else if (player.jumpped) { Transition("Player Jumping"); }
        }

        public override void OnStateEnter()
        {
            player.MountNearestLadder(player.maxReach);
        }

        public override void Update()
        {
            player.ladder.MoveOnLadder(player, new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));
        }

        public override void OnStateExit()
        {
            if (player.ladder) { player.DismountLadder(); }
        }
    }
    class PlayerOnLedge : PlayerCharacterState
    {
        PlayerController player;

        public PlayerOnLedge(CharacterStateNetwork network, PlayerController player) : base("Player On Ledge", network)
        { this.player = player; }

        public override void Subject()
        {
            //TODO: add a state condition for death when fear is implemented
            if (player.grabbed || player.ledge == null) { Transition("Player Falling"); }
            else if (player.jumpped) { Transition("Player Jumping"); }
        }

        public override void OnStateEnter()
        {
            player.GrabLedge();
        }

        public override void Update()
        {
            if (Input.GetAxis("Vertical") > 0) {
                player.ClimbLedge();
            }
        }

        public override void OnStateExit()
        {
            if (player.ledge == null) { player.ReleaseLedge(); }
        }
    }
    class PlayerFalling : PlayerCharacterState
    {
        PlayerController player;

        public PlayerFalling(CharacterStateNetwork network, PlayerController player) : base("Player Falling", network)
        { this.player = player; }

        public override void Subject()
        {
            //TODO: add a state condition for death when fear is implemented
            if (player.grabbed && player.LadderNearby()) { Transition("Player On Ladder"); }
            else if (player.grabbed && player.LedgeNearby()) { Transition("Player On Ledge"); }
            else if (player.isGrounded) { Transition("Player Walking"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y > 0) { Transition("Player Rising"); }
        }
    }
    class PlayerRising : PlayerCharacterState
    {
        PlayerController player;

        public PlayerRising(CharacterStateNetwork network, PlayerController player) : base("Player Rising", network)
        { this.player = player; }

        public override void Subject()
        {
            //TODO: add a state condition for death when fear is implemented
            if (player.grabbed && player.LadderNearby()) { Transition("Player On Ladder"); }
            else if (player.grabbed && player.LedgeNearby()) { Transition("Player On Ledge"); }
            else if (player.isGrounded) { Transition("Player Walking"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y < 0) { Transition("Player Falling"); }
        }
    }
    #endregion
}

public class PlayerCharacterState : CharacterState {
    public PlayerCharacterState(string name, CharacterStateNetwork network) : base(name,network) { }

    public override void OnStateEnter() { }
    public override void Update() { }
    public override void OnStateExit() { }
    public override void Subject() { }
}
