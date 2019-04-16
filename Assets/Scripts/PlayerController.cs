using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using soundTool.soundManager;

/// <summary>
/// PlayerController contains implemented methods of NavmeshAgent and responds to Key input accordingly.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerFearController))]
[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerController : NavmeshAgent2D {

    #region Editor Variables
    [Tooltip("How fast this player will reach its defined max speed.")]
    public float accelMultiplier;
    [Tooltip("The force used to propell the player upward. Higher values for objects with higher mass.")]
    public float jumpForce;
    public Sprite vingette;
    #endregion
   
    public bool grabbed;
    public bool jumpped;
    public float crouchSpeed;
    //Added code for accessing fear system - 2019-02-26 <Ben Shackman>
    //Will be used later to alter movement speed
    public PlayerFearController fearController;
    public PlayerStateMachine stateMachine;
    
    protected override void Start()
    {
        //Call start on the base NavmeshAgent2D
        base.Start();

        //Warn the user if the NavmeshArea2D object is not set up in the scene
        if (area == null) { Debug.LogWarning("There is no Navmesh set up in this scene. Not all movement features will be available."); }

        //Gets the fear controller - 2019-02-26 <Ben Shackman>
        fearController = gameObject.GetComponent<PlayerFearController>();
        stateMachine = GetComponent<PlayerStateMachine>();
    }

    protected override void Update() {
        //Call the Update function in the base NavmeshAgent2D
        base.Update();

        if (Time.timeScale > 0 && !pathing) {
            //Test for grabbing ladders/ledges
            
            if (!stateMachine.rising && !stateMachine.onLadder && !stateMachine.onLedge && Input.GetAxisRaw("Vertical") != 0f)
            {
                grabbed = true;
            }
            else if (Input.GetButtonDown("Grab")) { grabbed = true; }
            else { grabbed = false; }

            //Test for Jumping

            if (Input.GetButtonDown("Jump") && (isGrounded || stateMachine.onLadder || stateMachine.onLedge))
            {
                jumpped = true;
            }
            else { jumpped = false; }
        }

        Animate();

    }

    // Update is called once per frame
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        ParseInput();

        grabbed = false;
        jumpped = false;
    }

    public void PlaySound(string soundname) {
        string audioPath = Application.dataPath + "/StreamingAssets/Audio/";
        string clipName = soundname + ".wav";

        SoundManager.GetClipFromPath(audioPath + clipName, SoundManager.PlaySoundVoid);
    }

    public void PlayFootstep() {
        //initialize a string with the path to a random footstep sound found in "../StreamingAssets/Audio/footstep x"
        string audioPath = Application.dataPath + "/StreamingAssets/Audio/";
        string clipName = "footstep " + Random.Range(1, 4) + ".wav";

        SoundManager.GetClipFromPath(audioPath + clipName, SoundManager.PlaySoundVoid);
    }

    private void Animate() {
        animator.SetFloat("fear", fearController.currentFear);
        if (ledge != null)
        {
            animator.SetBool("ledge", true);
        }
        else { animator.SetBool("ledge", false); }

        if (ladder) { animator.SetBool("climbing", true); }
        else { animator.SetBool("climbing", false); }

        if (jumpped) { animator.SetTrigger("jump"); }
        animator.SetBool("sprinting", sprinting);
        animator.SetBool("grounded", isGrounded);
        animator.SetBool("prone", isProne);
        if (rigidbody.velocity.y < 0) { animator.SetBool("falling", true); }
        else { animator.SetBool("falling", false); }
    }

    protected virtual void ParseInput() {

        if (pathing) { return; }

        //Test for ping
        if (Input.GetButtonDown("Ping")) {
            GetComponent<PlayerAI>().ping = new Ping(transform.position);
        }

        //Test For Crouching OR sprinting (can not be both)
        if (Input.GetAxisRaw("Prone") > 0 && ledge == null && !ladder)
        {
            if (Input.GetButtonDown("Prone"))
            {
                transform.position = new Vector2(transform.position.x, transform.position.y - ((transform.localScale.y / 2) - (transform.localScale.y / 4)));
            }
            isProne = true;
        }
        else if (Input.GetAxisRaw("Sprinting") > 0 && ledge == null && !ladder) {
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
    }

    public virtual void Move(Vector2 direction) {
        if (ladder) { ladder.MoveOnLadder(this, direction); }
        else if (!stateMachine.incappacitated && ledge == null) { MoveHorizontal(direction.x); }
    }

    private void MoveHorizontal(float direction)
    {
        Vector3 movement = Vector3.zero;

        if (!isGrounded)
        {
            movement = new Vector3(speed / 2, 0f);
        }
        else
        {
            if (isProne)
            {
                movement = new Vector3(speed / 1.5f, 0f);
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

        if (pathing) {
            Debug.DrawRay(transform.position, direction * movement, Color.red);
        }

        if (direction > 0) { transform.localScale = new Vector3(1, 1, 1); }
        else if (direction < 0) { transform.localScale = new Vector3(-1, 1, 1); }

        if (isGrounded && stateMachine.walking && direction == 0) { animator.SetBool("walking", false); }
        else if (isGrounded && stateMachine.crawling && direction == 0) { animator.SetBool("crawling", false); }
        else if (isGrounded && stateMachine.crawling && direction != 0) { animator.SetBool("crawling", true); }
        else if (isGrounded && stateMachine.walking && direction != 0) { animator.SetBool("walking", true); }

        rigidbody.AddForce(direction * movement);
    }

    private void MaxSpeedCheck()
    {
        if (isProne)
        {
            if (rigidbody.velocity.magnitude > maxSpeed / 2f)
            {
                rigidbody.velocity = rigidbody.velocity.normalized;
                rigidbody.velocity = rigidbody.velocity * (maxSpeed / 1.5f);
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

    public virtual void Jump(float direction) {

        if (isProne) { return; }


        if (ladder) {
            DismountLadder();
            rigidbody.AddForce(new Vector2(jumpForce/2 * direction, jumpForce));
        }
        else if (ledge != null) {
            ReleaseLedge();
            rigidbody.AddForce(new Vector2(jumpForce / 2 * direction, jumpForce / 2) * Mathf.Sqrt(2));
        }
        else if (isGrounded) {
            rigidbody.AddForce(new Vector2(0f, jumpForce));
        }
    }
    
    public virtual void Decelerate() {
        if (!isGrounded) { return; }
        if (pathing && GetWalkDirection() == 0) { StartCoroutine(Decelerator()); }
        else if (!pathing && Input.GetAxisRaw("Horizontal") == 0) { StartCoroutine(Decelerator()); }
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
