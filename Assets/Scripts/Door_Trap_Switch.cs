using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_Trap_Switch : MonoBehaviour{
    Triggerscript trigger;
    private Animator open;
    private Rigidbody2D trap;
    public string switch_key = "v";

    private void Start()
    {
        trap = gameObject.GetComponent<Rigidbody2D>();
        if(gameObject.GetComponent<Animator>() != null)
        {
            Debug.Log("Door animation found.");
            open = gameObject.GetComponent<Animator>();
        }
    }

    private void Update()
    {

        if(trigger.triggered == true && gameObject.CompareTag("Door"))
        {
            open.SetBool("isopen", true);
        }

        if(trigger.triggered == true && gameObject.CompareTag("Trap"))
        {
            trap.isKinematic = false;
        }

        if (trigger.triggered == true && gameObject.CompareTag("Switch"))
        {
            if (Input.GetKeyDown(switch_key))
            {
                open.SetBool("isopen", true);
            }
        }
        if (trigger.triggered == false)
        {
            open.SetBool("isopen", false);
        }
    }
}