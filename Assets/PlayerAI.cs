using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NavmeshAgent2D))]
public class PlayerAI : CharacterStateNetwork {

    public Transform target;
    public Vector2 pingPosition;

    NavmeshAgent2D agent;
    PlayerStateMachine player;

    #region AI State Names
    PlayerAIDead dead;
    PlayerAIIdle idle;
    PlayerAIMoveTo moveTo;
    #endregion

    // Use this for initialization
    void Start () {
        agent = GetComponent<NavmeshAgent2D>();
        player = GetComponent<PlayerStateMachine>();

        dead = new PlayerAIDead(target, agent, this, player);
        idle = new PlayerAIIdle(target, agent, this, player);
        moveTo = new PlayerAIMoveTo(target, agent, this, player);

        CreateNetwork();
	}

    private void CreateNetwork() {

        dead.AddTransition(idle);

        idle.AddTransition(dead);
        idle.AddTransition(moveTo);

        moveTo.AddTransition(dead);
        moveTo.AddTransition(idle);

        activeState = idle;
    }

    #region AI State Definitions
    class PlayerAIDead : PlayerAIState {
        public PlayerAIDead(Transform target, NavmeshAgent2D agent, CharacterStateNetwork network, PlayerStateMachine player) : base("Player AI Dead", network, target, agent, player) { }

        public override void Subject()
        {
            if (!agent.isDead) { Transition("Player AI Idle"); }
        }

        public override void OnStateEnter()
        {
            Debug.Log("Player is dead");
        }
    }
    class PlayerAIIdle : PlayerAIState
    {
        public PlayerAIIdle(Transform target, NavmeshAgent2D agent, CharacterStateNetwork network, PlayerStateMachine player) : base("Player AI Idle", network, target, agent, player) { }

        public override void Subject()
        {
            if (agent.isDead) { Transition("Player AI Dead"); }
            else if (agent.path != null) { Transition("Player AI Move To"); }
        }

        public override void OnStateEnter()
        {
            Debug.Log("Player is now idling", agent.transform);
        }
    }
    class PlayerAIMoveTo : PlayerAIState {

        public PlayerAIMoveTo(Transform target, NavmeshAgent2D agent, CharacterStateNetwork network, PlayerStateMachine player) : base("Player AI Move To", network, target, agent, player) { }

        public override void Subject()
        {
            if (agent.isDead) { Transition("Player AI Dead"); }
            else if (agent.path == null) { Transition("Player AI Idle"); }
        }

        public override void FixedUpdate()
        {
            if (target) {
                if (Vector2.Distance(target.position, agent.transform.position) <= agent.stoppingDistance) { return; }
                //path to target.
                agent.FindPathTo(target.position);
                //get target node
                NavmeshNode2D targetNode = agent.GetTargetNodeInPath();

                //if agent.GetTargetNodeInPath is not a ground node or is connected to the previous node by a jump connection,
                /*if ((targetNode.type == NavmeshNode2D.NodeType.Crawlable ||
                     targetNode.type == NavmeshNode2D.NodeType.Walkable) ||
                     previousNode.GetConnection(targetNode).jump)
                {
                    //if agent is grounded, and the player is not crouching, jump
                    if (agent.isGrounded && !agent.isProne)
                    {
                        player.player.jumpped = true;
                    }
                }*/
                //else if agent.GetTargetNodeInPath is a ladder type node and not already on ladder,
                if (targetNode.type == NavmeshNode2D.NodeType.Ladder && !player.onLadder)
                {
                    //grab the nearest ladder.
                    agent.MountNearestLadder(agent.maxReach);
                }
                //else if agent.GetTargetNodeInPath is not a ladder node and agent is already on ladder, 
                else if (targetNode.type != NavmeshNode2D.NodeType.Ladder && player.onLadder)
                {
                    //jump off of ladder
                    player.player.Jump(player.player.GetWalkDirection());
                }
                //else if agent.GetTargetNodeInPath is a ladder
                else if (targetNode.type == NavmeshNode2D.NodeType.Ladder && player.onLadder)
                {
                    //move on ladder towards target node
                    player.player.Move(agent.GetWalkVector());
                }
                //else if agent.GetTargetNodeInPath is ledge, and not already on ledge,
                else if (targetNode.type == NavmeshNode2D.NodeType.Ledge && !player.onLedge)
                {
                    //grab nearest ledge
                    player.player.GrabLedge();
                }
                //else if agent.GetTargetNodeInPath is not ledge and already on ledge,
                else if (targetNode.type == NavmeshNode2D.NodeType.Ledge && player.onLedge)
                {
                    //jump off ledge
                    player.player.Jump(player.player.GetWalkDirection());
                }
                //else if agent.GetTargetNodeInPath is a ground node
                else if (targetNode.type == NavmeshNode2D.NodeType.Walkable ||
                         targetNode.type == NavmeshNode2D.NodeType.Crawlable) {
                    //if the target node is a crawl node
                    if (targetNode.type == NavmeshNode2D.NodeType.Crawlable)
                    {
                        //toggle crouch
                        player.player.isProne = true;
                    }
                    //else if the target node is a walk node
                    else if (targetNode.type == NavmeshNode2D.NodeType.Walkable) {
                        //untoggle crouch
                        player.player.isProne = false;
                    }
                    //move in the direction of the target node
                    player.player.Move(player.player.GetWalkVector());
                }
            }
        }
    }
    #endregion

}

public class PlayerAIState : CharacterState {
    protected Transform target;
    protected NavmeshAgent2D agent;
    protected PlayerStateMachine player;

    public PlayerAIState(string name, CharacterStateNetwork network, Transform target, NavmeshAgent2D agent, PlayerStateMachine player) : base(name, network) {
        this.target = target;
        this.agent = agent;
        this.player = player;
    }

    public override void OnStateEnter() { }
    public override void OnStateExit() { }
    public override void FixedUpdate() { }
    public override void Subject() { }
    public override void Update() { }
}
