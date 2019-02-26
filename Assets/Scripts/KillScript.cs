using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class KillScript : MonoBehaviour {
    private Scene Currentscene;
    private BoxCollider Kill;
	// Use this for initialization
	void Start ()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Player Has Triggered");
        if (other.gameObject.CompareTag("Player"))
        {
            Currentscene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(Currentscene.name);

        }
    }
}
