using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resetter : MonoBehaviour {

    PlayerController player;

	// Use this for initialization
	void Start () {
        player = FindObjectOfType<PlayerController>();
	}

    Vector2 lastPos;
    bool lastIsGrounded;
    Vector2 groundedPos;
    private void FixedUpdate()
    {
        if (lastIsGrounded && !player.isGrounded) {
            groundedPos = lastPos;
        }

        lastPos = player.transform.position;
        lastIsGrounded = player.isGrounded;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>()) {
            player.transform.position = groundedPos;
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

            player.GrabLedge();
        }
    }
}
