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

    /// <summary>
    /// The range at which the player checks for enemies
    /// </summary>
    [Tooltip("The range at which the player checks for enemies")]
    public float fearRange;

    /// <summary>
    /// How frequently the fear tick checks. 
    /// </summary>
    [Tooltip("How frequently the fear tick checks.")]
    public float fearTickTime = .5f;

    #endregion

    private Collider2D[] inRange;
    private LayerMask enemyMask;

    private void Start()
    {
        currentFear = 0;
        enemyMask = LayerMask.GetMask("Enemy");
        InvokeRepeating("FearTicker", 1, fearTickTime);
    }

    private void FearTicker()
    {
        inRange = Physics2D.OverlapCircleAll(transform.position, fearRange, enemyMask);
    }

    public void FearUpdater()
    {

    }

}
