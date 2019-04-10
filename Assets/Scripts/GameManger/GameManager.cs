using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public static GameManager GM;

    private void Awake()
    {
        if (GM != null) { Destroy(gameObject); }
        else { GM = this; }

        DontDestroyOnLoad(GM);
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
        SceneManager.LoadScene(toLoad);
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
}
