using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NavmeshAgent2D))]
public class PlayerAI : CharacterStateNetwork {

    public Transform target;
    public Ping ping;
    public string currentState;
    public string aiState;

    

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
        ping = null;

        dead = new PlayerAIDead(target, agent, this, player);
        idle = new PlayerAIIdle(target, agent, ping, this, player);
        moveTo = new PlayerAIMoveTo(target, ping, agent, this, player);

        CreateNetwork();
	}

    protected override void FixedUpdate() {
        target = PlayerSwapper.playerSwapper.currentPlayer.transform;
        currentState = player.activeState.name;
        aiState = activeState.name;

        if (agent.pathing)
        {
            moveTo.target = target;
            idle.target = target;
            moveTo.ping = ping;
            base.FixedUpdate();
        }
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
            Debug.Log("Player is dead", agent);
        }
    }
    class PlayerAIIdle : PlayerAIState
    {
        public Ping ping;

        public PlayerAIIdle(Transform target, NavmeshAgent2D agent, Ping ping, CharacterStateNetwork network, PlayerStateMachine player) : base("Player AI Idle", network, target, agent, player) {
            this.ping = ping;
        }

        public override void Subject()
        {
            if (agent.isDead) { Transition("Player AI Dead"); }
            else if (!agent.isStopped) { Transition("Player AI Move To"); }
        }

        public override void Update()
        {
            if (Vector2.Distance(target.position, agent.transform.position) <= agent.stoppingDistance) { agent.isStopped = true; }
            else if (ping != null)
            {
                if (Vector2.Distance(ping.pingPosition, agent.transform.position) <= agent.stoppingDistance) { agent.isStopped = true; }
            }
            else if (agent.path.Count == 0) { agent.isStopped = true; }
            else { agent.isStopped = false; }
        }
    }
    class PlayerAIMoveTo : PlayerAIState {
        public Ping ping;

        float timeSinceGrab;

        public PlayerAIMoveTo(Transform target, Ping ping, NavmeshAgent2D agent, CharacterStateNetwork network, PlayerStateMachine player) : base("Player AI Move To", network, target, agent, player) {
            this.ping = ping;
        }

        public override void Subject()
        {
            if (agent.isDead) { Transition("Player AI Dead"); }
            else if (agent.isStopped) { Transition("Player AI Idle"); }
        }

        public override void OnStateEnter()
        {
            timeSinceGrab = Time.time;
        }

        public override void FixedUpdate()
        {
            UnityEngine.Profiling.Profiler.BeginSample("Moving AI", agent);

            if (target) {
                LayerMask mask = new LayerMask();
                mask |= (1 << LayerMask.NameToLayer("Environment"));
                mask |= (1 << LayerMask.NameToLayer("Ladder"));
                RaycastHit2D targetGround = Physics2D.Raycast(target.transform.position, Vector2.down, 1000f, mask);

                //path to target.
                agent.FindPathTo(targetGround.point, 100);
                //get target node
                NavmeshNode2D targetNode = agent.GetTargetNodeInPath();
                NavmeshNode2D currentNode = agent.area.NodeAtPoint(agent.transform.position, agent);

                Debug.DrawLine(agent.transform.position, targetNode.worldPosition);

                //if agent.GetTargetNodeInPath is not a ground node or is connected to the previous node by a jump connection,
                /*
                if (agent.NodeIsTraversible(currentNode) && agent.isGrounded && !agent.ladder && agent.ledge == null && (targetNode.gridPosition.y > currentNode.gridPosition.y || Vector2.Distance(targetNode.worldPosition, agent.transform.position) > agent.jumpDistance)) {
                    player.player.jumpped = true;
                }
                */
                //else if agent.GetTargetNodeInPath is a ladder type node and not already on ladder,
                if ((Time.time - timeSinceGrab) > 2f && targetNode.type == NavmeshNode2D.NodeType.Ladder && !player.player.ladder)
                {
                    //grab the nearest ladder.
                    player.player.grabbed = true;
                    timeSinceGrab = Time.time;
                }
                //else if agent.GetTargetNodeInPath is not a ladder node and agent is already on ladder, 
                else if ((Time.time - timeSinceGrab) > 2f && targetNode.type != NavmeshNode2D.NodeType.Ladder && player.player.ladder && (Mathf.Abs(agent.transform.position.y - targetNode.worldPosition.y) < 0.5 || agent.transform.position.y > targetNode.worldPosition.y))
                {
                    //jump off of ladder
                    player.player.jumpped = true;
                    timeSinceGrab = Time.time;
                }
                //else if agent.GetTargetNodeInPath is ledge, and not already on ledge,
                else if ((Time.time - timeSinceGrab) > 2f && targetNode.type == NavmeshNode2D.NodeType.Ledge && player.player.ledge == null)
                {
                    //grab nearest ledge
                    player.player.grabbed = true;
                    timeSinceGrab = Time.time;
                }
                //else if agent.GetTargetNodeInPath is not ledge and already on ledge,
                else if ((Time.time - timeSinceGrab) > 2f && targetNode.type != NavmeshNode2D.NodeType.Ledge && player.player.ledge != null)
                {
                    //jump off ledge
                    player.player.jumpped = true;
                    timeSinceGrab = Time.time;
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
            else if (ping != null)
            {
                if (Vector2.Distance(ping.pingPosition, agent.transform.position) <= agent.stoppingDistance) { agent.isStopped = true; }
            }
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

public class Ping {
    public Vector2 pingPosition;

    public Ping(Vector2 pingPosition) {
        this.pingPosition = pingPosition;
    }
}
