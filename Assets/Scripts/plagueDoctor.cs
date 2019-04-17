using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using soundTool.soundManager;

public class plagueDoctor : MonoBehaviour {

    public Transform target;
    public float pursueRange;
    public float distanceToTarget;


    private float lastRangeTime;

    float rangeDelay;

    public Transform projectSpawn;
    public GameObject projectile;
    public float rangedRange;
    float timeTillHit;

    Rigidbody2D rb;
    public float speed;
    public float distance;

    private Animator PlaAnim;

    public Transform originPoint;
    public Transform originPoint2;
    public Transform jumpPoint;
    private Vector2 dir = new Vector2(-1, 0);
    public float range;

    public bool flipped;
    public AudioClip Spit;
    public AudioClip Wander;
    // Use this for initialization
    void Start()
    {
        PlaAnim = GetComponent<Animator>();
        target = GameObject.FindWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        timeTillHit = Random.Range(1.0f, 2.0f);
        rangeDelay = Random.Range(2.0f, 3.0f);
        distanceToTarget = Vector3.Distance(transform.position,target.position);

      
        if (distanceToTarget < pursueRange  && distanceToTarget > rangedRange)
        {
            Pursue();
        }
        
        if (distanceToTarget < rangedRange)
        {
            Ranged();
        }
    }
    

     void Pursue()
     {
         RaycastHit2D hitJump = Physics2D.Raycast(jumpPoint.position, dir, range, 1 << LayerMask.NameToLayer("Environment"));
        RaycastHit2D hitFloor = Physics2D.Raycast(originPoint2.position, dir, range, 1 << LayerMask.NameToLayer("Environment"));

        if (hitJump == true && !gameObject.CompareTag("Player"))
        {
            rb.AddForce(Vector2.up * 10000 * Time.deltaTime);
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
        PlaAnim.SetTrigger("IsSpitting");
        PlaAnim.SetTrigger("IsWalking");
        SoundManager.PlaySound(Wander, 1f);
        transform.Translate(Vector2.right * -speed * Time.deltaTime);
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
    void Flip()
    {
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
        flipped = !flipped;
    }
}

