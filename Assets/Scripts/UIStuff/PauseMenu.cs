using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour {

    private GameObject pauseMenu;
    private GameObject winMenu;
    public Text PauseMenuText;

    private bool EndTriggered;

	// Use this for initialization
	void Start () {
        pauseMenu = transform.GetChild(0).gameObject;
        winMenu = transform.GetChild(1).gameObject;
        pauseMenu.SetActive(false);
        winMenu.SetActive(false);
        EndTriggered = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButtonDown("Pause") && !EndTriggered)
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
        EndTriggered = true;
        pauseMenu.SetActive(true);
        PauseMenuText.text = "U AR DED";
    }

    public void OpenWinMenu()
    {
        EndTriggered = true;
        winMenu.SetActive(true);
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
