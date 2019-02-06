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
            if (Input.GetButtonDown("Action")) {
                LadderMountDismount(maxReach);
            }

            if (!isProne && isGrounded && Input.GetButtonDown("Jump")) {
                Jump();
            }
        }
    }

    // Update is called once per frame
    protected override void FixedUpdate()
    {
        isProne = Input.GetButton("Prone");
        Move();

        base.FixedUpdate();
    }

    protected virtual void Move() {
        if (!ladder)
        {
            if (isProne)
            {
                capsuleCollider.size = new Vector2(capsuleCollider.size.x, crouchHeight);
                _sprite.localScale = new Vector3(_sprite.localScale.x, _initSpriteHeight / 2);
            }
            else {
                capsuleCollider.size = new Vector2(capsuleCollider.size.x, height);
                _sprite.localScale = new Vector3(_sprite.localScale.x, _initSpriteHeight);
            }

            MoveHorizontal();
            MaxSpeedCheck();
            Decelerate();
        }
        else {
            _sprite.localScale = new Vector3(_sprite.localScale.x, _initSpriteHeight);
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
        if (isGrounded) {
            DismountLadder();
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
