using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour {

    private GameObject pauseMenu;

	// Use this for initialization
	void Start () {
        pauseMenu = transform.GetChild(0).gameObject;
        pauseMenu.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Pause"))
        {
            if(pauseMenu.activeInHierarchy)
            {
                pauseMenu.SetActive(false);
                Time.timeScale = 1.0f;
            }
            else
            {
                pauseMenu.SetActive(true);
                Time.timeScale = 0.0f;
            }
        }
	}

    public void Restart()
    {
        GameManager.GM.LoadLevel();
    }

    public void ReturnToMain()
    {
        GameManager.GM.LoadLevel(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
