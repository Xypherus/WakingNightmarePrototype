using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFearController : MonoBehaviour {

    #region EditorVariables
    /// <summary>
    /// The highest possable fear value that the player can have, Should generaly be set to 100
    /// </summary>
    [Tooltip("The highest possable fear value that the player can have, Should generaly be set to 100")]
    public int maxFear = 100;

    /// <summary>
    /// Only visable for testing purposes, should never be manualy changed. Represents the player's current fear.
    /// </summary>
    [Tooltip("Only visable for testing purposes, should never be manualy changed. Represents the player's current fear.")]
    public int currentFear;

    #endregion

    private void Start()
    {
        currentFear = 0;
    }

    public void FearUpdater()
    {

    }

}
