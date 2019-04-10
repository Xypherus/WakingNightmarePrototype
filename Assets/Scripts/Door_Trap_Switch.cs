﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using soundTool.soundManager;

public class Door_Trap_Switch : MonoBehaviour{
    //Definition of Audio Sources used
    public AudioClip Door;
    public AudioClip Switch;
    public AudioClip Trap;


    Triggerscript trigger;
    private Animator open;
    private Rigidbody2D trap;
    public string switch_key = "v";

    private void Start()
    {
        trigger = GetComponentInParent<Triggerscript>();
        trap = gameObject.GetComponent<Rigidbody2D>();
        if(gameObject.GetComponent<Animator>() != null)
        {
            Debug.Log("Door animation found.");
            open = gameObject.GetComponent<Animator>();
        }
    }
    IEnumerator Delay()
    {
        yield return new WaitForSeconds(1.5f);
    }
    private void Update()
    {
        if(trigger.triggered == true && gameObject.CompareTag("Door"))
        {
            open.SetBool("isopen", true);
            SoundManager.PlaySound(Door, 1f);
        }

        if(trigger.triggered == true && gameObject.CompareTag("Trap"))
        {
            if (trap != null)
            {
                Debug.Log("Trap has been triggered");
                SoundManager.PlaySound(Trap);
                trap.isKinematic = false;
            }
        }

        if (trigger.triggered == true && gameObject.CompareTag("Switch"))
        {
            if (Input.GetKeyDown(switch_key))
            {
                open.SetBool("isopen", true);
                SoundManager.PlaySound(Switch);

                //Delays door sound to be one second after switch sound
                StartCoroutine("Delay");
                SoundManager.PlaySound(Door);
            }
        }
        if (trigger.triggered == false)
        {
            open.SetBool("isopen", false);
        }
    }
}