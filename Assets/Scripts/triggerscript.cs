using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triggerscript : MonoBehaviour {
    public bool triggered = false;
    private Door_Trap_Switch trapswitch;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            triggered = true;
            trapswitch.playersInTrigger.Add(other.GetComponent<PlayerController>());
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            triggered = false;
            trapswitch.playersInTrigger.Remove(other.GetComponent<PlayerController>());
        }
    }
}
