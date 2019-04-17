using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlagueProj : MonoBehaviour {

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Shot Hit");
            Destroy(gameObject);
        }
    }
}
