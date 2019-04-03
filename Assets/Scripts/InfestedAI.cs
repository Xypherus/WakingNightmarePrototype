using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfestedAI : MonoBehaviour {
    public Transform target;
    public float pursueRange;
    public float distanceToTarget;

    private float lastBiteTime;
    public float biteDelay;
    public float biteRange;
    Rigidbody2D rb;
    public float speed;
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
        target = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget > pursueRange)
        {
            Patrol();
        }
        if (distanceToTarget < pursueRange && distanceToTarget > biteRange)
        {
            Pursue();
        }
        if (distanceToTarget < biteRange)
        {
            Bite();
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
        else if (target.position.x < transform.position.x && flipped)
        {
            Flip();
            speed *= -1;
        }
        transform.Translate(Vector2.right * -speed * Time.deltaTime);
    }
    
    void Bite()
    {
        if (Time.time > lastBiteTime + biteDelay)
        {
            Debug.Log("Hit");
            lastBiteTime = Time.time;
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
