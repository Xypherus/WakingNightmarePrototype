using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triggerscript : MonoBehaviour {
    public bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            triggered = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            triggered = false;
        }
    }
}
