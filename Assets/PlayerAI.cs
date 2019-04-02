using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NavmeshAgent2D))]
public class PlayerAI : CharacterStateNetwork {

    public Transform target;
    public Vector2 pingPosition = Vector3.zero;

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
        idle = new PlayerAIIdle(target, agent, pingPosition, this, player);
        moveTo = new PlayerAIMoveTo(target, pingPosition, agent, this, player);

        CreateNetwork();
	}

    private void FixedUpdate() {

        moveTo.target = target;
        moveTo.pingPosition = pingPosition;
        base.FixedUpdate();
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
        public Vector2 pingPosition;

        public PlayerAIIdle(Transform target, NavmeshAgent2D agent, Vector2 pingPosition, CharacterStateNetwork network, PlayerStateMachine player) : base("Player AI Idle", network, target, agent, player) {
            this.pingPosition = pingPosition;
        }

        public override void Subject()
        {
            if (agent.isDead) { Transition("Player AI Dead"); }
            else if (!agent.isStopped) { Transition("Player AI Move To"); }
        }

        public override void Update()
        {
            if (Vector2.Distance(target.position, agent.transform.position) <= agent.stoppingDistance) { agent.isStopped = true; }
            else if (Vector2.Distance(pingPosition, agent.transform.position) <= agent.stoppingDistance) { agent.isStopped = true; }
            else if (agent.path.Count == 0) { agent.isStopped = true; }
            else { agent.isStopped = false; }
        }

        public override void OnStateEnter()
        {
            Debug.Log("Player is now idling", agent.transform);
        }
    }
    class PlayerAIMoveTo : PlayerAIState {
        public Vector2 pingPosition;

        public PlayerAIMoveTo(Transform target, Vector2 pingPosition, NavmeshAgent2D agent, CharacterStateNetwork network, PlayerStateMachine player) : base("Player AI Move To", network, target, agent, player) {
            this.pingPosition = pingPosition;
        }

        public override void Subject()
        {
            if (agent.isDead) { Transition("Player AI Dead"); }
            else if (agent.isStopped) { Transition("Player AI Idle"); }
        }

        public override void FixedUpdate()
        {
            UnityEngine.Profiling.Profiler.BeginSample("Moving AI", agent);

            if (target) {
                //path to target.
                agent.FindPathTo(target.position, 1000);
                //get target node
                NavmeshNode2D targetNode = agent.GetTargetNodeInPath();

                Debug.DrawLine(agent.transform.position, targetNode.worldPosition);
                Debug.Log("The ai's player state is " + player.activeState.name, agent);
                Debug.Log(agent.GetWalkVector() + " is the walk vector", agent);
                
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
                if (targetNode.type == NavmeshNode2D.NodeType.Ladder && !player.player.ladder)
                {
                    //grab the nearest ladder.
                    player.player.grabbed = true;
                }
                //else if agent.GetTargetNodeInPath is not a ladder node and agent is already on ladder, 
                else if (targetNode.type != NavmeshNode2D.NodeType.Ladder && player.player.ladder && !(player.player.ladder.transform.position.y < agent.transform.position.y))
                {
                    //jump off of ladder
                    player.player.jumpped = true;
                }
                //else if agent.GetTargetNodeInPath is ledge, and not already on ledge,
                else if (targetNode.type == NavmeshNode2D.NodeType.Ledge && player.player.ledge == null)
                {
                    //grab nearest ledge
                    player.player.grabbed = true;
                }
                //else if agent.GetTargetNodeInPath is not ledge and already on ledge,
                else if (targetNode.type == NavmeshNode2D.NodeType.Ledge && player.player.ledge != null)
                {
                    //jump off ledge
                    player.player.jumpped = true;
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
                }
            }

            UnityEngine.Profiling.Profiler.EndSample();
            if (Vector2.Distance(target.position, agent.transform.position) <= agent.stoppingDistance) { agent.isStopped = true; }
            else if (Vector2.Distance(pingPosition, agent.transform.position) <= agent.stoppingDistance) { agent.isStopped = true; }
            else if (agent.path.Count == 0) { agent.isStopped = true; }
            else { agent.isStopped = false; }
        }
    }
    #endregion

}

public class PlayerAIState : CharacterState {
    public Transform target;
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
