using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerscript : MonoBehaviour {
    public bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            triggered = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            triggered = false;
        }
    }
}
