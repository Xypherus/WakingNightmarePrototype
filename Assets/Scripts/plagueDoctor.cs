using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class plagueDoctor : MonoBehaviour {

    public Transform target;
    //public float pursueRange;
    public float distanceToTarget;

    public float meleeRange;
    float lungeHeight;

    private float lastMeleeTime;
    private float lastRangeTime;
    public float meleeDelay;
    float rangeDelay;

    public Transform projectSpawn;
    public GameObject projectile;
    public float rangedRange;
    float timeTillHit;

    Rigidbody2D rb;
    public float speed;
    public float distance;

    private bool movingRight = true;
    public Transform groundDetection;
    // Use this for initialization
    void Start()
    {
        target = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        timeTillHit = Random.Range(1.0f, 2.0f);
        lungeHeight = Random.Range(1.0f, 2.0f);
        rangeDelay = Random.Range(2.0f, 3.0f);
        /*distanceToTarget = Vector3.Distance(transform.position,target.position);

        if (distanceToTarget > pursueRange)
        {
            Patrol();
        }
        if (distanceToTarget < pursueRange  && distanceToTarget > meleeRange)
        {
            Pursue();
        }
        */
        if (distanceToTarget < rangedRange && distanceToTarget > meleeRange)
        {
            Ranged();
        }
        if (distanceToTarget < meleeRange)
        {
            Melee();
        }
    }
    /*
     void Patrol()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
        //RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, distance);
        RaycastHit2D groundInfo = Physics2D.Raycast(transform.position, Vector2.down, distance);
        if (groundInfo.collider == false)
        {
            if (movingRight == true)
            {
                transform.eulerAngles = new Vector3(0, -180, 0);
                movingRight = false;
            }
            else
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                movingRight = true;
            }
        }
    }
    */
    /* void Pursue()
     {
         if (distanceToTarget < pursueRange)
         {
             transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
         }
     }
     */
    void Melee()
    {
        if (Time.time > lastMeleeTime + meleeDelay)
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
            lastMeleeTime = Time.time;
        }
    }
    void Ranged()
    {
        if (Time.time > lastRangeTime + rangeDelay)
        {
            //            Instantiate(projectile, projectSpawn.position, projectSpawn.rotation);
            //            lastAttackTime = Time.time;
            float xdistance;
            xdistance = target.position.x - projectSpawn.position.x;
            float ydistance;
            ydistance = target.position.y - projectSpawn.position.y;

            float arcAngle = Mathf.Atan((ydistance + 4.905f * (timeTillHit * timeTillHit)) / xdistance);

            float totalVelo = xdistance / (Mathf.Cos(arcAngle) * timeTillHit);
            float xVelo, yVelo;
            xVelo = totalVelo * Mathf.Cos(arcAngle);
            yVelo = totalVelo * Mathf.Sin(arcAngle);


            GameObject projectileInstance = Instantiate(projectile, projectSpawn.position, Quaternion.Euler(new Vector3(0, 0, 0))) as GameObject;
            Rigidbody2D rigid;
            rigid = projectileInstance.GetComponent<Rigidbody2D>();
            rigid.velocity = new Vector2(xVelo, yVelo);
            lastRangeTime = Time.time;

        }
    }
    void flip()
    {
        if (target.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(3, 3, 1);
        }
        else if (target.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-3, 3, 1);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("jump"))
        {
            rb.AddForce(Vector2.up * 600f);
        }
    }
}

