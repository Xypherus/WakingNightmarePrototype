using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : CharacterStateNetwork {
    public PlayerController player;

    public PlayerIncappacitated incappacitated;
    public PlayerWalking walking;
    public PlayerCrawling crawling;
    public PlayerJumping jumping;
    public PlayerJumpFromGrab jumpFromGrab;
    public PlayerOnLadder onLadder;
    public PlayerOnLedge onLedge;
    public PlayerRising rising;
    public PlayerFalling falling;

    // Use this for initialization
    void Start () {
        player = GetComponent<PlayerController>();

        incappacitated = new PlayerIncappacitated(this, player);
        walking = new PlayerWalking(this, player);
        crawling = new PlayerCrawling(this, player);
        jumping = new PlayerJumping(this, player, 0.5f);
        jumpFromGrab = new PlayerJumpFromGrab(this, player, 0.5f);
        onLadder = new PlayerOnLadder(this, player);
        onLedge = new PlayerOnLedge(this, player);
        rising = new PlayerRising(this, player);
        falling = new PlayerFalling(this, player);

        walking.AddTransition(incappacitated);
        walking.AddTransition(crawling);
        walking.AddTransition(falling);
        walking.AddTransition(rising);
        walking.AddTransition(jumping);
        walking.AddTransition(onLadder);
        walking.AddTransition(onLedge);

        crawling.AddTransition(incappacitated);
        crawling.AddTransition(walking);
        crawling.AddTransition(rising);
        crawling.AddTransition(falling);
        crawling.AddTransition(onLedge);
        crawling.AddTransition(onLadder);

        jumping.AddTransition(incappacitated);
        jumping.AddTransition(rising);
        jumping.AddTransition(falling);
        jumping.AddTransition(onLadder);
        jumping.AddTransition(onLedge);
        jumping.AddTransition(walking);

        jumpFromGrab.AddTransition(incappacitated);
        jumpFromGrab.AddTransition(rising);
        jumpFromGrab.AddTransition(falling);
        jumpFromGrab.AddTransition(onLadder);
        jumpFromGrab.AddTransition(onLedge);
        jumpFromGrab.AddTransition(walking);

        onLadder.AddTransition(incappacitated);
        onLadder.AddTransition(jumpFromGrab);
        onLadder.AddTransition(falling);

        onLedge.AddTransition(incappacitated);
        onLedge.AddTransition(jumpFromGrab);
        onLedge.AddTransition(falling);

        rising.AddTransition(incappacitated);
        rising.AddTransition(onLedge);
        rising.AddTransition(onLadder);
        rising.AddTransition(falling);
        rising.AddTransition(walking);

        falling.AddTransition(incappacitated);
        falling.AddTransition(onLedge);
        falling.AddTransition(onLadder);
        falling.AddTransition(rising);
        falling.AddTransition(walking);

        activeState = falling;
    }

    #region Player States
    public class PlayerIncappacitated : PlayerCharacterState {
        PlayerController player;

        public PlayerIncappacitated(CharacterStateNetwork network, PlayerController player) : base("Player Incappacitated", network) {
            this.player = player;
        }

        public override void Subject()
        {
            if (player.fearController.currentFear < player.fearController.maxFear) { Transition("Player Walking"); }
        }

        public override void OnStateEnter()
        {
            player.animator.SetBool("Dead", true);
        }

        public override void OnStateExit()
        {
            player.animator.SetBool("Dead", false);
        }
    }
    public class PlayerWalking : PlayerCharacterState {
        PlayerController player;

        public PlayerWalking(CharacterStateNetwork network, PlayerController player) : base("Player Walking", network)
        { this.player = player; }

        public override void Subject()
        {
            if (player.fearController.currentFear >= player.fearController.maxFear) { Transition("Player Incappacitated"); }
            else if (player.isGrounded && player.jumpped) { Transition("Player Jumping"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y > 0) { Transition("Player Rising"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y < 0) { Transition("Player Falling"); }
            else if (player.isGrounded && player.isProne) { Transition("Player Crawling"); }
            else if (player.grabbed && player.LadderNearby()) { Transition("Player On Ladder"); }
            else if (player.grabbed && player.LedgeNearby()) { Transition("Player On Ledge"); }
        }

        public override void FixedUpdate() {
            float speed = player.speed;
            if (player.sprinting) { speed = player.speed * 2; }

            if (player.pathing) { player.Move(new Vector2 (player.GetWalkDirection(), 0f)); }
            else { player.Move(new Vector2 (Input.GetAxis("Horizontal"), 0f)); }

            player.Decelerate();
        }
    }
    public class PlayerCrawling : PlayerCharacterState
    {
        PlayerController player;

        public PlayerCrawling(CharacterStateNetwork network, PlayerController player) : base("Player Crawling", network)
        { this.player = player; }

        public override void Subject()
        {
            if (player.fearController.currentFear >= player.fearController.maxFear) { Transition("Player Incappacitated"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y > 0) { Transition("Player Rising"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y < 0) { Transition("Player Falling"); }
            else if (player.isGrounded && !player.isProne) { Transition("Player Walking"); }
            else if (player.grabbed && player.LadderNearby()) { Transition("Player On Ladder"); }
            else if (player.grabbed && player.LedgeNearby()) { Transition("Player On Ledge"); }
        }

        public override void OnStateEnter()
        {
            player.SetSize(new Vector2(player.width, player.crouchHeight));
        }

        public override void FixedUpdate()
        {
            player.rigidbody.AddForce(new Vector2(Mathf.Clamp(Input.GetAxis("Horizontal") * player.accelMultiplier, -1, 1) * player.crouchSpeed, 0f));

            player.Decelerate();
        }

        public override void OnStateExit()
        {
            player.SetSize(new Vector2(player.width, player.height));
        }
    }
    public class PlayerJumping : PlayerCharacterState
    {
        PlayerController player;
        float elapsedTime;
        float maxTime;

        public PlayerJumping(CharacterStateNetwork network, PlayerController player, float maxTime) : base("Player Jumping", network)
        { this.player = player; this.maxTime = maxTime; }

        public override void Subject()
        {
            if (player.fearController.currentFear >= player.fearController.maxFear) { Transition("Player Incappacitated"); }
            else if (player.isGrounded) { Transition("Player Walking"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y > 0 && elapsedTime >= maxTime) { Transition("Player Rising"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y < 0 && elapsedTime >= maxTime) { Transition("Player Falling"); }
            else if (player.grabbed && player.LadderNearby()) { Transition("Player On Ladder"); }
            else if (player.grabbed && player.LedgeNearby()) { Transition("Player On Ledge"); }
        }

        public override void OnStateEnter()
        {
            player.Jump(0f);
        }

        public override void FixedUpdate()
        {
            elapsedTime += Time.deltaTime;
        }

        public override void OnStateExit()
        {
            elapsedTime = 0f;
        }
    }
    public class PlayerOnLadder : PlayerCharacterState
    {
        PlayerController player;

        public PlayerOnLadder(CharacterStateNetwork network, PlayerController player) : base("Player On Ladder", network)
        { this.player = player; }

        public override void Subject()
        {
            if (player.fearController.currentFear >= player.fearController.maxFear) { Transition("Player Incappacitated"); }
            else if (player.grabbed || !player.ladder) { Transition("Player Falling"); Debug.Log("tests: " + player.grabbed + " OR " + !player.ladder); }
            else if (player.jumpped) { Transition("Player Jump From Grab"); }
        }

        public override void OnStateEnter()
        {
            player.MountNearestLadder(player.maxReach);
        }

        public override void FixedUpdate()
        {
            if (player.ladder)
            {
                player.ladder.MoveOnLadder(player, new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));

                if (player.ladder.ladderType == Ladder.LadderType.Side) {
                    if (player.ladder.GetComponent<Rigidbody2D>() != null) {
                        player.ladder.GetComponent<Rigidbody2D>().AddForce(new Vector2(player.speed /4 * Input.GetAxis("Horizontal"), 0f));
                    }
                }
            }
        }

        public override void OnStateExit()
        {
            if (player.ladder) { player.DismountLadder(); }
        }
    }
    public class PlayerOnLedge : PlayerCharacterState
    {
        PlayerController player;

        public PlayerOnLedge(CharacterStateNetwork network, PlayerController player) : base("Player On Ledge", network)
        { this.player = player; }

        public override void Subject()
        {
            if (player.fearController.currentFear >= player.fearController.maxFear) { Transition("Player Incappacitated"); }
            else if (player.grabbed || player.ledge == null) { Transition("Player Falling"); }
            else if (player.jumpped) { Transition("Player Jump From Grab"); }
        }

        public override void OnStateEnter()
        {
            player.GrabLedge();
        }

        public override void FixedUpdate()
        {
            if (Input.GetAxis("Vertical") > 0) {
                player.ClimbLedge();
            }
        }

        public override void OnStateExit()
        {
            if (player.ledge != null) { player.ReleaseLedge(); }
        }
    }
    public class PlayerFalling : PlayerCharacterState
    {
        PlayerController player;

        public PlayerFalling(CharacterStateNetwork network, PlayerController player) : base("Player Falling", network)
        { this.player = player; }

        public override void Subject()
        {
            if (player.fearController.currentFear >= player.fearController.maxFear) { Transition("Player Incappacitated"); }
            else if (player.grabbed && player.LadderNearby()) { Transition("Player On Ladder"); }
            else if (player.grabbed && player.LedgeNearby()) { Transition("Player On Ledge"); }
            else if (player.isGrounded) { Transition("Player Walking"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y > 0) { Transition("Player Rising"); }
        }

        public override void FixedUpdate()
        {
            player.rigidbody.AddForce(new Vector2((player.speed / 1.5f) * Input.GetAxis("Horizontal"), 0f));
        }
    }
    public class PlayerRising : PlayerCharacterState
    {
        PlayerController player;

        public PlayerRising(CharacterStateNetwork network, PlayerController player) : base("Player Rising", network)
        { this.player = player; }

        public override void Subject()
        {
            if (player.fearController.currentFear >= player.fearController.maxFear) { Transition("Player Incappacitated"); }
            else if (player.grabbed && player.LadderNearby()) { Transition("Player On Ladder"); }
            else if (player.grabbed && player.LedgeNearby()) { Transition("Player On Ledge"); }
            else if (player.isGrounded) { Transition("Player Walking"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y < 0) { Transition("Player Falling"); }
        }

        public override void FixedUpdate()
        {
            player.rigidbody.AddForce(new Vector2((player.speed / 1.5f) * Input.GetAxis("Horizontal"), 0f));
        }
    }
    public class PlayerJumpFromGrab : PlayerCharacterState
    {
        PlayerController player;
        float elapsedTime;
        float maxTime;

        public PlayerJumpFromGrab(CharacterStateNetwork network, PlayerController player, float maxTime) : base("Player Jump From Grab", network)
        { this.player = player; this.maxTime = maxTime; }

        public override void Subject()
        {
            if (player.fearController.currentFear >= player.fearController.maxFear) { Transition("Player Incappacitated"); }
            else if (player.isGrounded) { Transition("Player Walking"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y > 0 && elapsedTime >= maxTime) { Transition("Player Rising"); }
            else if (!player.isGrounded && player.rigidbody.velocity.y < 0 && elapsedTime >= maxTime) { Transition("Player Falling"); }
            else if (player.grabbed && player.LadderNearby()) { Transition("Player On Ladder"); }
            else if (player.grabbed && player.LedgeNearby()) { Transition("Player On Ledge"); }
        }

        public override void OnStateEnter()
        {
            if (player.pathing)
            {
                player.Jump(player.GetWalkDirection());
            }
            else {
                player.Jump(Input.GetAxisRaw("Horizontal"));
            }
        }

        public override void FixedUpdate()
        {
            elapsedTime += Time.deltaTime;
        }
    }
    #endregion
}

[System.Serializable]
public class PlayerCharacterState : CharacterState {
    public PlayerCharacterState(string name, CharacterStateNetwork network) : base(name,network) { }

    public override void OnStateEnter() { }
    public override void Update() { }
    public override void OnStateExit() { }
    public override void Subject() { }
    public override void FixedUpdate() { }
}
