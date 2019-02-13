﻿using System.Collections;
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

        //Some Inputs can not be parsed in FixedUpdate due to input loss.
        //Fixed update happens every physics step, and thus drops input that does not
        //happen within that physics step. Update calls every frame.
        if (Time.timeScale > 0) {
            //
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

        if (rigidbody.velocity.y < 0 && Input.GetButton("Grab") && !ladder & ledge == null) {
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
                Rigidbody2D ladderigidbodyody = ladder.GetComponent<Rigidbody2D>();
                if (ladderigidbodyody)
                {
                    ladderigidbodyody.AddRelativeForce(direction * movement);
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

        rigidbody.AddForce(direction * movement);
    }

    private void MaxSpeedCheck()
    {
        if(isProne)
        {
            if (rigidbody.velocity.magnitude > maxSpeed / 2f)
            {
                rigidbody.velocity = rigidbody.velocity.normalized;
                rigidbody.velocity = rigidbody.velocity * (maxSpeed / 2f);
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

    protected virtual void Jump() {
        if (ladder) { DismountLadder(); rigidbody.AddForce(new Vector2(0f, jumpForce*2)); return; }
        else if (ledge != null) { ReleaseLedge(); rigidbody.AddForce(new Vector2(0f, jumpForce*2)); return; }

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
        while (rigidbody.velocity.x != 0) {
            if (Input.GetAxisRaw("Horizontal") != 0 || !isGrounded) { break; }
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
