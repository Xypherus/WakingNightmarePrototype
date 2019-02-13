using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerController contains implemented methods of NavmeshAgent and responds to Key input accordingly.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : NavmeshAgent2D {

    #region Editor Variables
    [Tooltip("How fast this player will reach its defined max speed.")]
    public float accelMultiplier;
    [Tooltip("The force used to propell the player upward. Higher values for objects with higher mass.")]
    public float jumpForce;
    #endregion

    bool wasCrouched;

    protected override void Start()
    {
        //Call start on the base NavmeshAgent2D
        base.Start();

        //Warn the user if the NavmeshArea2D object is not set up in the scene
        if (area == null) { Debug.LogWarning("There is no Navmesh set up in this scene. Not all movement features will be available."); }
    }

    protected override void Update() {
        //Call the Update function in the base NavmeshAgent2D
        base.Update();

        if (Time.timeScale > 0) {
            ParseInput();
        }
    }

    // Update is called once per frame
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected virtual void ParseInput() {
        //Test For Crouching OR sprinting (can not be both)
        if (Input.GetButton("Prone") && ledge == null)
        {
            isProne = true;
            wasCrouched = true;
        }
        else if (Input.GetButton("Sprinting") && ledge == null) {
            sprinting = true;
        }

        //check for Prone release
        if (Input.GetButtonUp("Prone") && isProne) {
            Vector2 newSize = new Vector2(width * transform.localScale.x, height * transform.localScale.y);
            Vector2 newPos = new Vector2(transform.position.x, (transform.position.y - (crouchHeight * transform.localScale.y) / 2 + (height * transform.localScale.y) / 2));
            Collider2D ceiling = Physics2D.OverlapCapsule(newPos, newSize, CapsuleDirection2D.Vertical, 0f, 1 << LayerMask.NameToLayer("Environment"));

            if (!ceiling) { isProne = false; wasCrouched = false; }
            else if (wasCrouched) { isProne = true; wasCrouched = true; }
        }

        //check for sprint release
        if (Input.GetButtonUp("Sprinting") && sprinting) {
            sprinting = false;
        }

        //Test for sprinting
        if (Input.GetButton("Sprinting") && !Input.GetButton("Prone") && !isProne)
        {
            sprinting = true;
        }
        else if (!Input.GetButton("Sprinting") && sprinting)
        {
            sprinting = false;
        }

        //Test for grabbing ladders/ledges
        if (Input.GetButtonDown("Grab")) {
            if (ladder || ledge != null)
            {
                if (ladder) { DismountLadder(); }
                else if (ledge != null) { ReleaseLedge(); }
            }
            else {
                MountNearestLadder(maxReach);
                if (!ladder) {
                    GrabLedge();
                }
            }
        }

        //Test for Jumping
        if (Input.GetButtonDown("Jump")) {
            Jump();
        }

        
        Move();
    }

    protected virtual void Move() {
        Crouch();

        if (Input.GetAxisRaw("Vertical") != 0 && ledge != null) {
            ClimbLedge();
        }

        if (ladder) {
            Vector3 movement = new Vector2(speed / 4, 0f);
            float direction = Mathf.Clamp(Input.GetAxis("Horizontal") * accelMultiplier, -1f, 1f);

            ladder.MoveOnLadder(GetComponent<NavmeshAgent2D>(), new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));

            if (isProne)
            {
                Debug.Log("Applying force to ladder");
                Rigidbody2D ladderigidbodyody = ladder.GetComponent<Rigidbody2D>();
                if (ladderigidbodyody)
                {
                    ladderigidbodyody.AddRelativeForce(direction * movement);

                }
            }
        }

        MoveHorizontal();
        Decelerate();
    }

    private void MoveHorizontal()
    {
        Vector3 movement = Vector3.zero;
        float direction = Mathf.Clamp(Input.GetAxis("Horizontal") * accelMultiplier, -1f, 1f);

        if (!isGrounded)
        {
            movement = new Vector3(speed / 2, 0f);
        }
        else
        {
            if (isProne)
            {
                movement = new Vector3(speed / 2, 0f);
            }
            else if (sprinting)
            {
                movement = new Vector3(speed * 2, 0f);
            }
            else
            {
                movement = new Vector3(speed, 0);
            }
        }

        rigidbody.AddForce(direction * movement);
    }

    private void MaxSpeedCheck()
    {
        if (isProne)
        {
            if (rigidbody.velocity.magnitude > maxSpeed / 2f)
            {
                rigidbody.velocity = rigidbody.velocity.normalized;
                rigidbody.velocity = rigidbody.velocity * (maxSpeed / 2f);
                {
                    rigidbody.velocity = rigidbody.velocity.normalized;
                    rigidbody.velocity = rigidbody.velocity * (maxSpeed / 2f);
                }
            }
            else if (sprinting)
            {
                if (rigidbody.velocity.magnitude > maxSpeed * 2f)
                {
                    rigidbody.velocity = rigidbody.velocity.normalized;
                    rigidbody.velocity = rigidbody.velocity * (maxSpeed * 2f);
                }
            }
            else
            {
                if (rigidbody.velocity.magnitude > maxSpeed)
                {
                    rigidbody.velocity = rigidbody.velocity.normalized;
                    rigidbody.velocity = rigidbody.velocity * maxSpeed;
                }
            }

        }
    }

    protected virtual void Jump() {
        if (ladder) {
            DismountLadder();
            rigidbody.AddForce(new Vector2(jumpForce/2 * Input.GetAxisRaw("Horizontal"), jumpForce/2) * Mathf.Sqrt(2));
            return;
        }
        else if (ledge != null) {
            ReleaseLedge();
            rigidbody.AddForce(new Vector2(jumpForce / 2 * Input.GetAxisRaw("Horizontal"), jumpForce / 2) * Mathf.Sqrt(2));
            return;
        }

        if (isGrounded) {
            rigidbody.AddForce(new Vector2(0f, jumpForce));
        }
    }
    
    protected virtual void Decelerate() {
        if (!isGrounded) { return; }
        if (Input.GetAxisRaw("Horizontal") == 0) { StartCoroutine(Decelerator()); }
        else { StopCoroutine(Decelerator()); }
    }

    protected IEnumerator Decelerator() {
        float direction = Mathf.Abs(rigidbody.velocity.x) / rigidbody.velocity.x;

        while (rigidbody.velocity.x != 0 && isGrounded) {
            Vector2 deceleration = new Vector2(direction * (1/accelMultiplier), 0);
            rigidbody.velocity -= deceleration;

            if ((rigidbody.velocity.x * direction) - (1/accelMultiplier) < 0) { rigidbody.velocity = new Vector2(0, rigidbody.velocity.y); }
            yield return new WaitForEndOfFrame();
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
    }
}
