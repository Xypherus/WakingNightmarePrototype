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

    public int Health = 1;
    public float distance;

    public Transform originPoint;
    public Transform originPoint2;
    public Transform jumpPoint;
    private Vector2 dir = new Vector2(-1, 0);
    public float range;

    public bool flipped;
    // Use this for initialization
    void Start()
    {
        target = GameObject.Find("Thomas").transform;
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
        RaycastHit2D hitFloor = Physics2D.Raycast(originPoint2.position, dir, range);

        if (hit == true)
        {
            Flip();
            speed *= -1;
            dir *= -1;
        }
        if (!hitFloor)
        {
            Flip();
            speed *= -1;
            dir *= -1;
        }
        transform.Translate(Vector2.right * -speed * Time.deltaTime);
    }
    void Pursue()
    {
        RaycastHit2D hitJump = Physics2D.Raycast(jumpPoint.position, dir, range);
        RaycastHit2D hitFloor = Physics2D.Raycast(originPoint2.position, dir, range);

        if (hitJump == true)
        {
            rb.AddForce(Vector2.up * 1000 * Time.deltaTime);
        }

        if (target.position.x > transform.position.x && !flipped)
        {
            Flip();
            speed *= -1;
        }
        else if (target.position.x < transform.position.x &&  flipped)
        {
            Flip();
            speed *= -1;
        }
        transform.Translate(Vector2.right * -speed * Time.deltaTime);
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
        flipped = !flipped;
    }
}

