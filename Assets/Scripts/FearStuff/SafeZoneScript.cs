using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneScript : MonoBehaviour {

    /// <summary>
    /// Whether or not this safezone is currently in a useable state. False if not
    /// </summary>
    public bool useable;

    /// <summary>
    /// How many characters must be inside this zone for it to activate. Should be two under normal conditions.
    /// </summary>
    public int playerCountToActivate = 2;

    /// <summary>
    /// Public only for debug, list of all players currently within the zone.
    /// </summary>
    public List<PlayerFearController> playersInZone;

	// Use this for initialization
	void Start () {
        useable = true;
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && useable)
        {
            playersInZone.Add(collision.GetComponent<PlayerFearController>());

            if(playersInZone.Count >= playerCountToActivate)
            {
                foreach(PlayerFearController player in playersInZone)
                {
                    player.ApplySafeZone();
                }
                useable = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && useable)
        {
            playersInZone.Remove(collision.GetComponent<PlayerFearController>());
        }
    }

    /// <summary>
    /// Included if needed. Resets the safezone to be useable again
    /// </summary>
    public void ResetSafeZone()
    {
        playersInZone.Clear();
        useable = true;
    }
}
