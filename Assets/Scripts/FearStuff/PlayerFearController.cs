using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Types of fear. Darkness is here for convience, should not be applied directly as a fear that a character has.
/// </summary>
public enum FearTypes { FearTypeA, FearTypeB, FearTypeC, FearTypeD, Darkness };

public class PlayerFearController : MonoBehaviour {
    //private bool EnemyFear;
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

    // <summary>
    // The rate at which the players fear depletes normally
    // <summary>
    [Tooltip("The value at which the player's fear depletes")]
    public int normalFearFade = 1;

    /// <summary>
    /// The rate at which the players fear depletes while in a safe zone
    /// </summary>
    [Tooltip("The value at which the player's fear depletes while in a safe zone")]
    public int safezoneFearFade = 15;

    /// <summary>
    /// Changes the delay between when a player gets hit and when they can start recovering fear. Set to 0 to disable.
    /// </summary>
    [Tooltip("Changes the delay between when a player gets hit and when they can start recovering fear. Set to 0 to disable.")]
    public float fearDecayCooldown = 0;

    /// <summary>
    /// Sets the amount of damage a trap does when it hits a player
    /// </summary>
    [Tooltip("Sets the amount of damage a trap does when it hits a player")]
    public int trapdamage = 40;

    /// <summary>
    /// The fear modifier applied by darkness. Set to 1.0f for no modifier.
    /// </summary>
    [Tooltip("The fear modifier applied by darkness. Set to 1.0f for no modifier.")]
    public float darknessFearChange = 1.5f;

    //What this person is afraid of
    /// <summary>
    /// Add all fears that this character is afraid of here
    /// </summary>
    [Tooltip("Add all fears that this character is afraid of here")]
    public FearTypes[] fears;

    #endregion

    //FMOD initialization and variable setup - Jake
    [FMODUnity.EventRef]
    public string PlayerStateEvent;
    FMOD.Studio.EventInstance playerState;
    float Fearpar = 0.0f;
    
    //This is just a place holder, I need to figure out how to find wether or not the player is in fear range first
    //I've set this variable up. It's true when out of fear range, false when in - Ben
    public bool outOfRange;

    /// <summary>
    /// Same Function as outOfRange for for passive fear zones rather than active
    /// </summary>
    public bool outOfZone;

    //Tells wether player uses normal fear depletion or safezone fear depletion
    public bool safe = false;

    /// <summary>
    /// Serves to allow for fear decay to go on cooldown. Should not be change manualy. See fearDecayCooldown variable to change functionality.
    /// </summary>
    public bool fearCanDecay;

    /// <summary>
    /// Modifier which is applied to all fear changes. Used primarly for darkness. Visable for debug
    /// </summary>
    public float currentFearModifier;

    /// <summary>
    /// The actual time until fear can decay again.
    /// </summary>
    private float fearCooldownTime;

    /// <summary>
    /// Halts fear operations if true;
    /// </summary>
    [Tooltip("Halts fear operations if true. Do not change manualy from inspector unless for debuging")]
    public bool playerIsDead;

    /// <summary>
    /// Fear zones that the player is in. Only visable for debug, should not be manualy changed.
    /// </summary>
    public List<FearZone> withinFearZones;

    private Collider2D[] inRange;
    private LayerMask enemyMask;

    /// <summary>
    /// Changes the character's fear. Please use this function in liu of directly changing fear, as this includes several checks.
    /// </summary>
    /// <param name="fearChange">How much to change the fear Modifiers are applied on top of this value</param>
    /// <param name="killable">True of this fear is able to kill the player, False otherwise</param>
    public void ChangeFear(int fearChange, bool killable)
    {
        int newFear = currentFear + (int)(fearChange * currentFearModifier);
        if(killable) { currentFear = Mathf.Clamp(newFear, 0, maxFear); }
        else { currentFear = Mathf.Clamp(newFear, 0, maxFear - 1); }

        if(currentFear == maxFear)
        {
            //TriggerDeath();
        }
    }

    private void Start()
    {
        //Getting FMOD Event
        //playerState = FMODUnity.RuntimeManager.CreateInstance(PlayerStateEvent);
        //playerState.start();
        

        currentFear = 0;
        currentFearModifier = 1.0f;
        enemyMask = 1 << LayerMask.NameToLayer("Enemy");
        InvokeRepeating("FearTicker", 1, fearTickTime);
        fearCanDecay = true;
        playerIsDead = false;
    }

    private void Awake()
    {
        Debug.Log("Adding player to list");
        GameManager.GM.PlayerCharacters.Add(this);
    }
    //Updating The Fear Parameter in FMOD - Jake
    /*void Update()
    {
        playerState.setParameterValue("Fear", Fearpar);
        Debug.Log(Fearpar);
        
        //Increases Fear Parameter Amount
        foreach (Collider2D enemy in inRange)
        {
            if (EnemyFear == true)
            {
                Fearpar += 0.01f;
            }
        }
        //Decreases Fear Parameter Amount
        
            if (safe == true)
            {
                //Decreases Fear Parameter - Jake
                Fearpar -= 0.2f;
            }
            else if (fearCanDecay)
            {
                //Decreases Fear Parameter - Jake
                Fearpar -= 0.01f;
            }
        
        //Sets min and max fear - Jake
        if (Fearpar <= 0)
        {
            Fearpar = 0;
        }
        if (Fearpar >= 15)
        {
            Fearpar = 15;
        }
    }*/

    /// <summary>
    /// Performs all operations related to passive fear gain.
    /// </summary>
    /// <returns>Returns true if passive fear is being applied, false if not.</returns>
    private bool ApplyPassiveFear()
    {
        inRange = Physics2D.OverlapCircleAll(transform.position, fearRange, enemyMask);
        //Debug.Log("Inrange.length = " + inRange.Length);
        outOfRange = true;
        if (inRange.Length != 0)
        {
            foreach (Collider2D enemy in inRange)
            {
                if(fears.Contains(enemy.GetComponent<EnemyClass>().fearType))
                {
                    outOfRange = false;
                    EnemyClass enemyClass = enemy.GetComponent<EnemyClass>();
                    float distance = Vector2.Distance(enemy.transform.position, transform.position);
                    float fearMod = Mathf.Clamp((fearRange - distance) / fearRange, 0f, .9f) + .1f;
                    //Debug.Log("FearMod = " + fearMod);
                    ChangeFear((int)(enemyClass.fearDOT * fearMod), false);
                    //Increase fear Parameter
                    //EnemyFear = true;
                }
            }
            return true;
        }
        else { return false; }
    }

    #region ZoneFear
    /// <summary>
    /// Carries out all operations related to Zone Fear
    /// </summary>
    /// <returns>Returns true if zone fear is being applied, false if not</returns>
    private bool ApplyZoneFear()
    {
        outOfZone = true;
        float newFearModifier = 1.0f;

        if(withinFearZones.Count != 0)
        {
            foreach(FearZone aFear in withinFearZones)
            {
                if(fears.Contains(aFear.fearType) && aFear.fearModifier != 0)
                {
                    outOfZone = false;
                    newFearModifier *= aFear.fearModifier;
                }
            }
            currentFearModifier = newFearModifier;
            return true;
        }
        else
        {
            currentFearModifier = newFearModifier;
            return false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("FearZone"))
        {
            withinFearZones.Add(collision.GetComponent<FearZone>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("FearZone"))
        {
            withinFearZones.Remove(collision.GetComponent<FearZone>());
        }

        if (collision.gameObject.CompareTag("SafeZone"))
        {
            collision.gameObject.SetActive(false);
        }
    }
    #endregion

    /// <summary>
    /// Performs all operations related to fear decay.
    /// </summary>
    private void ApplyFearDecay()
    {
        //Moved code from FearFade to A function. Same effect, just prevents from having multiple coroutines running - Ben
        if (outOfRange && outOfZone && currentFear > 0)
        {
            if (safe == true)
            {
                ChangeFear(-safezoneFearFade, false);
                //Decreases Fear Parameter - Jake
                //EnemyFear = false;
            }
            else if(fearCanDecay)
            {
                ChangeFear(-normalFearFade, false);
                //Decreases Fear Parameter - Jake
                //EnemyFear = false;
            }
        }
    }

    /// <summary>
    /// Function to put all operations that need to be performed upon death.
    /// Currently empty, change as needed
    /// </summary>
    public void TriggerDeath()
    {
        playerIsDead = true;
    }

    public void UntriggerDeath()
    {
        playerIsDead = false;
    }

    private void FearTicker()
    {
        if(!playerIsDead)
        {
            ApplyZoneFear();
            ApplyPassiveFear();
            ApplyFearDecay();
        }
    }

    //This event applies active fear (On Collision Fear)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Debug.Log("fear dealt", collision.gameObject);
            ChangeFear(collision.gameObject.GetComponent<EnemyClass>().fearDealt, true);
            //Handles the cooldown
            fearCooldownTime = fearDecayCooldown;
            if(fearCanDecay)
            {
                fearCanDecay = false;
                StartCoroutine(SetDecayCooldown());
            }
        }
    }

    IEnumerator SetDecayCooldown()
    {
        while(fearCooldownTime != 0)
        {
            yield return new WaitForSeconds(1f);
            fearCooldownTime--;
        }

        fearCanDecay = true;
    }

    //Leaving this in a region as it may be unneeded. Code for handeling of one time use safezones, which is, I believe how they are intended to work - Ben
    #region Safe Zone Handler
    /// <summary>
    /// Applies safe zone. Public as it is called from the safezone script as needed.
    /// </summary>
    public void ApplySafeZone()
    {
        ChangeFear(-currentFear, false);
    }

    #endregion

    // <summary>
    // Sets the safe zone variable to true if the player is in a safezone
    // <summary>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("SafeZone"))
        {
            safe = true;
        }
    }
    

}
