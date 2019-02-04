using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]

public class PlayerController : MonoBehaviour {

    public float speed;
    public float maxSpeed;
    public float accelMultiplier;
    public bool isProne = false;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        isProne = Input.GetButton("Prone");
        MoveHorizontal();
        MaxSpeedCheck();
    }
    void MoveHorizontal()
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

        Debug.Log(direction);
        rb.AddForce(direction * movement);
    }

    void MaxSpeedCheck()
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
}
