﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public static GameManager GM;
    public GameObject PauseUI;

    public List<PlayerFearController> PlayerCharacters;

    private void Awake()
    {
        if (GM != null) { Destroy(gameObject); }
        else { GM = this; }

        DontDestroyOnLoad(GM);
    }

    private void Start()
    {
        LoadLevel();
    }

    delegate void OnLevelLoaded();
    /// <summary>
    /// Actions to be performed upon loading a level
    /// </summary>
    OnLevelLoaded levelLoaded;

    /// <summary>
    /// Loads specified level
    /// </summary>
    /// <param name="toLoad">The level ID that is to be loaded</param>
    public void LoadLevel(int toLoad)
    {
        Debug.Log("Level Loaded");
        PlayerCharacters.Clear();
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(toLoad);
        //if(toLoad != 0) { BuildPauseUI(); }
        if(levelLoaded != null) { levelLoaded(); }
    }

    /// <summary>
    /// Loads specified level.
    /// When called without any paramaters, automaticly Reloads current level
    /// </summary>
    public void LoadLevel()
    {
        LoadLevel(SceneManager.GetActiveScene().buildIndex);
    }

    private void BuildPauseUI()
    {
        Debug.Log("Building UI");
        Instantiate(PauseUI);
    }

    /// <summary>
    /// Contains all functions required to end the level
    /// </summary>
    public void DoLevelEnd()
    {
        GameObject.Find("Pause Menu Parent").GetComponent<PauseMenu>().OpenWinMenu();
    }

    private void Update()
    {
        if(PlayerCharacters.Count != 0)
        {
            int deadPlayers = 0;
            foreach(PlayerFearController player in PlayerCharacters)
            {
                if (player.playerIsDead) { deadPlayers++; }
            }
            if (deadPlayers >= PlayerCharacters.Count)
            {
                //DO DEATH THINGS
                GameObject.Find("Pause Menu Parent").GetComponent<PauseMenu>().OpenDeathMenu();
                Time.timeScale = 0.0f;
                Debug.Log("U R DED");
            }
        }
    }
}
