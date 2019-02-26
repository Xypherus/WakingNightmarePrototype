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
            //Test for grabbing ladders/ledges
            if (Input.GetButtonDown("Grab"))
            {
                if (!ladder && ledge == null)
                {
                    MountNearestLadder(maxReach);
                    if (!ladder)
                    {
                        GrabLedge();
                    }
                }
            }

            //Test for Jumping
            if (Input.GetButtonDown("Jump"))
            {
                Jump();
            }

            if (Input.GetButtonDown("Prone") && (ladder || ledge != null)) {
                if (ladder) { DismountLadder(); }
                else { ReleaseLedge(); }
            }
        }
    }

    // Update is called once per frame
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        ParseInput();
    }

    protected virtual void ParseInput() {

        //Test For Crouching OR sprinting (can not be both)
        if (Input.GetAxisRaw("Prone") > 0 && ledge == null)
        {
            if (Input.GetButtonDown("Prone"))
            {
                transform.position = new Vector2(transform.position.x, transform.position.y - ((transform.localScale.y / 2) - (transform.localScale.y / 4)));
            }
            isProne = true;
        }
        else if (Input.GetAxisRaw("Sprinting") > 0 && ledge == null) {
            sprinting = true;
        }

        //check for Prone release
        if (Input.GetAxisRaw("Prone") < 1 && isProne) {
            Vector2 newSize = new Vector2(width * transform.localScale.x, 0.02f);
            Vector2 newPos = new Vector2(transform.position.x, transform.position.y + (transform.localScale.y * height));
            Collider2D[] ceilings = Physics2D.OverlapCapsuleAll(newPos, newSize, CapsuleDirection2D.Vertical, 0f, 1 << LayerMask.NameToLayer("Environment"));
            Transform ground = GetGround();
            Collider2D ceiling = null;

            foreach (Collider2D collider in ceilings) {
                Debug.Log("got ceiling", collider.transform);
                if (ground)
                {
                    if (collider.transform != ground) { ceiling = collider; }
                }
                else { ceiling = collider; }
            }

            if (!ceiling) { isProne = false; }
            else { isProne = true; Debug.Log("Cant Uncrouch", ceiling.transform); }
        }

        //check for sprint release
        if (Input.GetAxisRaw("Sprinting") < 1 && sprinting) {
            sprinting = false;
        }

        if (!ladder && ledge == null && Input.GetAxisRaw("Vertical") != 0) {
            MountNearestLadder(maxReach);
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

            if (Input.GetAxisRaw("Horizontal") != 0)
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
            rigidbody.AddForce(new Vector2(jumpForce/2 * Input.GetAxisRaw("Horizontal"), jumpForce));
        }
        else if (ledge != null) {
            ReleaseLedge();
            rigidbody.AddForce(new Vector2(jumpForce / 2 * Input.GetAxisRaw("Horizontal"), jumpForce / 2) * Mathf.Sqrt(2));
        }
        else if (isGrounded) {
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
