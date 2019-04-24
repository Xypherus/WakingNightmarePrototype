using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderBullet : MonoBehaviour
{

    private GameObject Player;
    private GameObject Target;
    public float BulletSpeed;
    PlayerSwapper SwapScript;
    private Vector2 dir;
    
    public GameObject ActivePlayer;
    // Use this for initialization
    void Start()
    {
        Invoke("Die", 3.0f);   
        Player = GameObject.FindWithTag("Player");
        Target = GetComponentInParent<SpiderShooting>().ActivePlayer;

        dir = Target.transform.position - gameObject.transform.position;
        gameObject.transform.parent = null;
        //SwapScript = GameObject.Find("PlayerSwapper").GetComponent<PlayerSwapper>();
        //ActivePlayer = GameObject.Find("PlayerSwapper").GetComponent<PlayerSwapper>().ActivePlayer;
        ActivePlayer = PlayerSwapper.playerSwapper.currentPlayer.gameObject;
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
      
        if (collision.gameObject == ActivePlayer && !collision.CompareTag("Enemy")) 
        {
            PlayerFearController playerFear = collision.GetComponent<PlayerFearController>();
            playerFear.ChangeFear(99, true);
            Destroy(gameObject);
        }
     //   Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {

       
        // transform.position = Vector2.MoveTowards(transform.position, tempPos.position, BulletSpeed * Time.deltaTime);
        transform.Translate(dir * Time.deltaTime, Space.World);
    }
void Die()
    {
        Destroy(gameObject);
    }
    
}
