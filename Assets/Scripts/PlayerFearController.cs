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
    /// Use ChangeFear to change this value whenever possable
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

    //How Much Fear Depletes
    [Tooltip("The value at which the player's fear depletes")]
    public int normalFearFade = 1;

    [Tooltip("The value at which the player's fear depletes while in a safe zone")]
    public int safezoneFearFade = 15;

    #endregion
    //This is just a place holder, I need to figure out how to find wether or not the player is in fear range first
    //I've set this variable up. It's true when out of fear range, false when in - Ben
    public bool outOfRange;
    //Tells wether player uses normal fear depletion or safezone fear depletion
    public bool safe = false;

    private Collider2D[] inRange;
    private LayerMask enemyMask;

    /// <summary>
    /// Changes the character's fear
    /// </summary>
    /// <param name="fearChange">The ammount of fear to change, Positive adds fear, negitive removes it</param>
    private void ChangeFear(int fearChange)
    {
        int newFear = currentFear + fearChange;
        currentFear = Mathf.Clamp(newFear, 0, maxFear);
    }

    private void Start()
    {
        currentFear = 0;
        enemyMask = 1 << LayerMask.NameToLayer("Enemy");
        InvokeRepeating("FearTicker", 1, fearTickTime);
    }
    private void Update()
    {
        if (outOfRange == true)
        {
            //StartCoroutine("FearFade");
        }
    }

    /// <summary>
    /// Performs all operations related to passive fear gain.
    /// </summary>
    /// <returns>Returns true if passive fear is being applied, false if not.</returns>
    private bool ApplyPassiveFear()
    {
        inRange = Physics2D.OverlapCircleAll(transform.position, fearRange, enemyMask);
        //Debug.Log("Inrange.length = " + inRange.Length);
        if (inRange.Length != 0)
        {
            outOfRange = false;
            foreach (Collider2D enemy in inRange)
            {
                EnemyClass enemyClass = enemy.GetComponent<EnemyClass>();
                float distance = Vector2.Distance(enemy.transform.position, transform.position);
                float fearMod = Mathf.Clamp((fearRange - distance) / fearRange, 0f, .9f) + .1f;
                //Debug.Log("FearMod = " + fearMod);

                ChangeFear((int)(enemyClass.fearDOT * fearMod));
            }
            return true;
        }
        else
        {
            outOfRange = true;
            return false;
        }
    }

    /// <summary>
    /// Performs all operations related to fear decay.
    /// </summary>
    private void ApplyFearDecay()
    {
        //Moved code from FearFade to A function. Same effect, just prevents from having multiple coroutines running - Ben
        if (outOfRange && currentFear > 0)
        {
            if (safe == true)
            {
                ChangeFear(-safezoneFearFade);
            }
            else
            {
                ChangeFear(-normalFearFade);
            }
        }
    }

    private void FearTicker()
    {
        ApplyPassiveFear();
        ApplyFearDecay();
    }

    public void FearUpdater()
    {

    }

    //Subtracts values from currentfear every [Fearticker] seconds
    IEnumerator FearFade()
    { 
        if(currentFear > 0)
        {
            if(safe == true)
            {
                currentFear = currentFear - safezoneFearFade;
            }
            else
            {
                currentFear = currentFear - normalFearFade;
            }
        }
        yield return new WaitForSeconds(fearTickTime);
    }

    //Sets the safe zone variable to true if the player is in a safezone
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("SafeZone"))
        {
            safe = true;
        }
    }

}
