using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyClass : MonoBehaviour {

    //Fear dealt when player collides with the enemy
    public int fearDealt;

    //The distance when the player starts recieving fear
    public int fearDistance;

    //How much fear the player takes while in the proximity of the enemy
    public int fearDOT;

    //How far the player is before the enemy will began their attack
    public int agroDistance;

    //The distance beyond the agroDistance that the Enemy will continue to chase
    public int trackDistance;
    
    //Enemies total health
    public int enemyHealth;

}
