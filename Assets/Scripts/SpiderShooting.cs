using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderShooting : MonoBehaviour {

    public Spider sp;
    public float ShootCD;
    public GameObject WebProjectile;
    //public PlayerSwapper SwappingScript;
    //public GameObject target;
    public GameObject ActivePlayer;

    public void Awake()
    {
        //SwappingScript = GameObject.FindWithTag("Swapper").GetComponent<PlayerSwapper>();
        //sp = GetComponentInParent<Spider>();
        //Debug.Log("SP is " + GetComponentInParent<Spider>());
    }

    public void Start()
    {
        
    }

    void Update()
    {
        //ActivePlayer = SwappingScript.ActivePlayer;
        ActivePlayer = PlayerSwapper.playerSwapper.currentPlayer.gameObject;
    }

	void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("StartAttackRoutine");
        if (collision.gameObject ==ActivePlayer)
        {
         
            StartCoroutine("Attacking");
            sp.shooting = true;
        }
    }
    IEnumerator Attacking()
    {

        while (sp.target != null)
        {

            GameObject bullet =  Instantiate(WebProjectile, gameObject.transform.position, Quaternion.identity);
            bullet.transform.parent = gameObject.transform;
            yield return new WaitForSeconds(ShootCD);
        }
        yield return null;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject == ActivePlayer)
        {
            StopCoroutine("Attacking");
            sp.shooting = false;
        }
    }
}
