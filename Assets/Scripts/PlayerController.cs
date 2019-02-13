using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]

public class PlayerController : NavmeshAgent2D {
    
    public float accelMultiplier;
    public float jumpForce;
    

    private Rigidbody2D rb;

    

    protected override void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        base.Start();

        if (area == null) { Debug.LogWarning("There is no Navmesh set up in this scene. Not all movement features will be available."); }
    }

    bool wasCrouched;
    protected override void Update() {
        base.Update();

        if (Time.timeScale > 0) {

            if (Input.GetButtonDown("Action")) {
                LadderMountDismount(maxReach);
            }

            if (Input.GetButtonDown("Jump")) {
                Jump();
            }
        }
    }

    // Update is called once per frame
    protected override void FixedUpdate()
    {
        if (isProne) { capsuleCollider.size = new Vector2(width, crouchHeight); }
        else { capsuleCollider.size = new Vector2(width, height); }

        if (rb.velocity.y < 0 && Input.GetButton("Grab") && !ladder & ledge == null) {
            GrabLedge();
        }

        if (ledge != null && Input.GetAxisRaw("Vertical") > 0) {
            ClimbLedge();
        }
        if (ledge != null && Input.GetAxisRaw("Vertical") < 0 || Input.GetButton("Grab")) {
            ReleaseLedge();
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
            Vector3 movement = new Vector2(speed/4, 0f);
            float direction = Mathf.Clamp(Input.GetAxis("Horizontal") * accelMultiplier, -1f, 1f);

            ladder.MoveOnLadder(GetComponent<NavmeshAgent2D>(), new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));

            if (isProne) {
                Debug.Log("Applying force to ladder");
                Rigidbody2D ladderBody = ladder.GetComponent<Rigidbody2D>();
                if (ladderBody)
                {
                    ladderBody.AddRelativeForce(direction * movement);
                }
            }
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
        else
        {
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
        if (ladder) { DismountLadder(); rb.AddForce(new Vector2(0f, jumpForce*2)); return; }
        else if (ledge != null) { ReleaseLedge(); rb.AddForce(new Vector2(0f, jumpForce*2)); return; }

        if (isGrounded) {
            rb.AddForce(new Vector2(0f, jumpForce));
        }
    }
    
    protected virtual void Decelerate() {
        if (!isGrounded) { return; }
        if (Input.GetAxisRaw("Horizontal") == 0) { StartCoroutine(Decelerator()); }
        else { StopCoroutine(Decelerator()); }
    }

    protected IEnumerator Decelerator() {
        float direction = Mathf.Abs(rb.velocity.x) / rb.velocity.x;
        while (rb.velocity.x != 0) {
            if (Input.GetAxisRaw("Horizontal") != 0 || !isGrounded) { break; }
            Vector2 deceleration = new Vector2(direction * (1/accelMultiplier), 0);
            rb.velocity -= deceleration;

            if ((rb.velocity.x * direction) - (1/accelMultiplier) < 0) { rb.velocity = new Vector2(0, rb.velocity.y); }
            yield return new WaitForEndOfFrame();
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
    }
}
