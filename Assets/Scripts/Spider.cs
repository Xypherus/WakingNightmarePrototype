using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider : MonoBehaviour {
    GameObject player;
    PlayerController pc;
   
    public bool shooting;
    public GameObject target;
    private Vector2 startingPosition;
    public float grabSpeed;
    private bool isGrabbing = false;
    private Vector2 dir;

    private bool grabbingDown;
    private bool grabbingUp;
	// Use this for initialization
	void Start () {
        player = GameObject.FindWithTag("Player");
        pc = player.GetComponent<PlayerController>();
       
        startingPosition = gameObject.transform.position;
        
        
	}
	
	// Update is called once per frame
	void Update () {
        target = GetComponentInChildren<SpiderShooting>().ActivePlayer;
        if(grabbingDown && isGrabbing)
        {
            float step = grabSpeed * Time.deltaTime;
            transform.Translate(Vector3.down * step, Space.World);
        }
        if(isGrabbing && grabbingUp)
        {
            float step = grabSpeed * Time.deltaTime;
            transform.Translate(Vector3.up * step, Space.World);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == target)
        {
             isGrabbing = true;
          //  StartCoroutine("Grab");
          
        }
    }
   void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject == target)
        {
            isGrabbing = false;
        }
    }
    IEnumerator Grab()
    {
        GetComponentInChildren<SpiderShooting>().StopCoroutine("Attacking");

            grabbingDown = true;
            yield return new WaitForSeconds(3.0f);
            grabbingDown = false;
            grabbingUp = true;
            yield return new WaitForSeconds(6.0f);
            grabbingUp = false;
        
        StopCoroutine("Grab");
        yield return null;
    }

//    public static RaycastHit2D Linecast(Vector2 start, Vector2 end, int layerMask = DefaultRaycastLayers, float minDepth = -Mathf.Infinity, float MaxDepth = Mathf.Infinity);

}
