using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour {

    public Transform player;
    public float speed;
    public Vector3 offset;
	
	// Update is called once per frame
	void Update () {
        if (!player) { FindPlayer(); }

        MoveCamera();
	}

    void MoveCamera() {
        if (player) {
            transform.position = Vector3.Lerp(transform.position, player.position + offset, speed * Time.deltaTime);
        }
    }

    void FindPlayer() {
        player = FindObjectOfType<PlayerController>().transform;
        if (!player) { Debug.LogError("The camera can not find the player. Is it in the scene?"); }
    }

    void OnDrawGizmosSelected() {
        if (!player) { FindPlayer(); }

        transform.position = player.position + offset;
    }
}
