using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_Trap_Switch : MonoBehaviour{
    triggerscript trigger;
    private Animator open;
    public string switch_key = "v";

    private void Start()
    {
        gameObject.GetComponent<Animator>();
    }
    private void Update()
    {
        if(trigger.triggered == true && gameObject.CompareTag("Door"))
        {
            open.SetBool("isopen", true);
        }
        if(trigger.triggered == true && gameObject.CompareTag("Trap"))
        {
            gameObject.SetActive(true);
        }
        if (trigger.triggered == true && gameObject.CompareTag("Switch"))
        {
            if (Input.GetKeyDown(switch_key))
            {
                open.SetBool("isopen", true)
            }
        }
        else
        {
            open.SetBool("isopen", false);
        }
    }
}