using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using soundTool.soundManager;

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

    private Animator infAnim;

    public Transform originPoint;
    public Transform originPoint2;
    public Transform jumpPoint;
    private Vector2 dir = new Vector2(-1, 0);
    public float range;

    public bool flipped;
    public AudioClip Melee;
    public AudioClip Wander;
    // Use this for initialization
    void Start()
    {
        infAnim = GetComponent<Animator>();
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
        RaycastHit2D hit = Physics2D.Raycast(originPoint.position, dir, range, 1 << LayerMask.NameToLayer("Environment"));
        RaycastHit2D hitFloor = Physics2D.Raycast(originPoint2.position, dir, range, 1 << LayerMask.NameToLayer("Environment"));

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
        infAnim.ResetTrigger("IsHitting");
        infAnim.SetTrigger("IsWalking");
        transform.Translate(Vector2.right * -speed * Time.deltaTime);
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
        infAnim.ResetTrigger("IsHitting");
        infAnim.SetTrigger("IsWalking");
        SoundManager.PlaySound(Wander, 1f);
        transform.Translate(Vector2.right * -speed * Time.deltaTime);
    }
    
    void Bite()
    {
        if (Time.time > lastBiteTime + biteDelay)
        {
            SoundManager.PlaySound(Melee, 1f);
            Debug.Log("Hit");
            lastBiteTime = Time.time;
            infAnim.ResetTrigger("IsWalking");
            infAnim.SetTrigger("IsHitting");
            
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
