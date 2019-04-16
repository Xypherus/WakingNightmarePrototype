using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {

    private GameObject pauseMenu;
    public Text PauseMenuText;

    private bool DeathTriggered;

	// Use this for initialization
	void Start () {
        pauseMenu = transform.GetChild(0).gameObject;
        pauseMenu.SetActive(false);
        DeathTriggered = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Pause") && !DeathTriggered)
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

    public void OpenDeathMenu()
    {
        DeathTriggered = true;
        pauseMenu.SetActive(true);
        PauseMenuText.text = "U AR DED";
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
