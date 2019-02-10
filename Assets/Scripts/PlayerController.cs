using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]

public class PlayerController : NavmeshAgent2D {
    
    public float accelMultiplier;
    public float jumpForce;
    public bool isProne = false;

    private Rigidbody2D rb;

    

    protected override void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        base.Start();
    }

    protected void Update() {
        if (Time.timeScale > 0) {

            if (Input.GetButton("Prone")) { isProne = true; }
            else {
                Vector2 newSize = new Vector2(width*transform.localScale.x, height*transform.localScale.y);
                Vector2 newPos = new Vector2(transform.position.x, (transform.position.y - (crouchHeight * transform.localScale.y) / 2 + (height * transform.localScale.y) / 2));
                Collider2D ceiling = Physics2D.OverlapCapsule(newPos, newSize, CapsuleDirection2D.Vertical, 0f, 1 << LayerMask.NameToLayer("Environment"));

                if (!ceiling) { isProne = false; }
                else { isProne = true; }
            }

            if (Input.GetButtonDown("Action")) {
                LadderMountDismount(maxReach);
            }

            if (!isProne && isGrounded && Input.GetButtonDown("Jump")) {
                Jump();
            }

            if (ledge != null) {
                if (isProne)
                {
                    ReleaseLedge();
                }
                else if (Input.GetButtonDown("Jump")) {
                    ClimbLedge();
                }
            }
        }
    }

    // Update is called once per frame
    protected override void FixedUpdate()
    {
        if (isProne) { capsuleCollider.size = new Vector2(width, crouchHeight); }
        else { capsuleCollider.size = new Vector2(width, height); }

        if (rb.velocity.y < -5 && !ladder & ledge == null) {
            GrabLedge();
        }

        Move();

        base.FixedUpdate();
    }

    protected virtual void Move() {
        if (!ladder && ledge == null)
        {
            MoveHorizontal();
            MaxSpeedCheck();
            Decelerate();
        }
        else if (ladder && ledge == null)
        {
            ladder.MoveOnLadder(GetComponent<NavmeshAgent2D>(), new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));
        }
    }

    private void MoveHorizontal()
    {
        Vector3 movement = Vector3.zero;
        float direction = Mathf.Clamp(Input.GetAxis("Horizontal") * accelMultiplier, -1f, 1f);

        if (isProne)
        {
            movement = new Vector3(speed / 2, 0f);
        }
        else {
            movement = new Vector3(speed, 0);
        }
        
        rb.AddForce(direction * movement);
    }

    private void MaxSpeedCheck()
    {
        if(isProne)
        {
            if (rb.velocity.magnitude > maxSpeed / 2f)
            {
                rb.velocity = rb.velocity.normalized;
                rb.velocity = rb.velocity * (maxSpeed / 2f);
            }
        }
        else
        {
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized;
                rb.velocity = rb.velocity * maxSpeed;
            }
        }
        
    }

    protected virtual void Jump() {
        if (ladder) { DismountLadder(); }
        else if (ledge != null) { ClimbLedge(); return; }

        if (isGrounded) {
            rb.AddForce(new Vector2(0f, jumpForce));
        }
    }
    
    protected virtual void Decelerate() {
        if (Input.GetAxisRaw("Horizontal") == 0) { StartCoroutine(Decelerator()); }
        else { StopCoroutine(Decelerator()); }
    }

    protected IEnumerator Decelerator() {
        float direction = Mathf.Abs(rb.velocity.x) / rb.velocity.x;
        while (rb.velocity.x != 0) {
            Vector2 deceleration = new Vector2(direction * (1/accelMultiplier), 0);
            rb.velocity -= deceleration;

            if ((rb.velocity.x * direction) - (1/accelMultiplier) < 0) { rb.velocity = new Vector2(0, rb.velocity.y); }
            yield return new WaitForEndOfFrame();
        }
    }
    
}
