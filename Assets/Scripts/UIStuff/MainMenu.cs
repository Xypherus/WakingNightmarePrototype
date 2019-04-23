using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour {

    /// <summary>
    /// The number of the first level in the build settings
    /// </summary>
    [Tooltip("The number of the first level in the build settings")]
    public int firstLevelValue = 1;

    /// <summary>
    /// The number of the credits level in the build settings
    /// </summary>
    [Tooltip("The number of the credits level in the build settings")]
    public int creditsLevelValue = 2;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	void StartGame()
    {
        GameManager.GM.LoadLevel(firstLevelValue);
    }

    void LoadCreditsMenu()
    {
        GameManager.GM.LoadLevel(creditsLevelValue);
    }
}
