using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentipedeAI : MonoBehaviour
{
    public Transform target;
    public float pursueRange;
    public float distanceToTarget;

    public float lungeRange;
    float lungeHeight;

    private float lastLungeTime;

    public float lungeDelay;


    Rigidbody2D rb;
    public float speed;
    public Transform spawnTwo;

    public int Health = 1;
    public float distance;

    public Transform originPoint;
    private Vector2 dir = new Vector2(-1, 0);
    public float range;

    NavmeshAgent2D agent;
    // Use this for initialization
    void Start()
    {
        agent = GetComponent<NavmeshAgent2D>();
        target = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        lungeHeight = Random.Range(1.0f, 2.0f);
        distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget > pursueRange)
        {
            Patrol();
        }
        if (distanceToTarget < pursueRange && distanceToTarget > lungeRange)
        {
            Pursue();
        }
        if (distanceToTarget < lungeRange)
        {
            Lunge();
        }

    }
    void Patrol()
    {
        RaycastHit2D hit = Physics2D.Raycast(originPoint.position, dir, range);

        if (hit == true)
        {
            Flip();
            speed *= -1;
            dir *= -1;
        }
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }
    void Pursue()
    {
        AIBS();
    }
    void Lunge()
    {
        if (Time.time > lastLungeTime + lungeDelay)
        {
            float xdistance;
            xdistance = target.position.x - transform.position.x;
            float ydistance;
            ydistance = target.position.y - transform.position.y;

            float arcAngle = Mathf.Atan((ydistance + 4.905f * (lungeHeight * lungeHeight)) / xdistance);

            float totalVelo = xdistance / (Mathf.Cos(arcAngle) * lungeHeight);
            float xVelo, yVelo;
            xVelo = totalVelo * Mathf.Cos(arcAngle);
            yVelo = totalVelo * Mathf.Sin(arcAngle);


            rb.velocity = new Vector2(xVelo, yVelo);

            Debug.Log("hit");
            lastLungeTime = Time.time;
        }
    }
    void Flip()
    {
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
    public Vector2 pingPosition;



    void AIBS()
    {

        UnityEngine.Profiling.Profiler.BeginSample("Moving AI", agent);
        if (Vector2.Distance(target.position, agent.transform.position) <= agent.stoppingDistance) { agent.isStopped = true; }
        else if (Vector2.Distance(pingPosition, agent.transform.position) <= agent.stoppingDistance) { agent.isStopped = true; }
        else { agent.isStopped = false; }

        if (target && !agent.isStopped)
        {
            //path to target.
            agent.FindPathTo(target.position, 100);
            //get target node
            NavmeshNode2D targetNode = agent.GetTargetNodeInPath();

            Debug.DrawLine(agent.transform.position, targetNode.worldPosition);
            
            Debug.Log(agent.GetWalkVector() + " is the walk vector", agent);

            if (targetNode.type == NavmeshNode2D.NodeType.Walkable ||
                     targetNode.type == NavmeshNode2D.NodeType.Crawlable)
            {
                //if the target node is a crawl node
                if (targetNode.type == NavmeshNode2D.NodeType.Crawlable)
                {
                    //toggle crouch
                    target.player.isProne = true;
                }
                //else if the target node is a walk node
                else if (targetNode.type == NavmeshNode2D.NodeType.Walkable)
                {
                    //untoggle crouch
                    player.player.isProne = false;
                }
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }
}

