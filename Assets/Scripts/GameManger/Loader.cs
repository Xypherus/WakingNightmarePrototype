using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains the scripts to load up all game manger objects
/// </summary>
public class Loader : MonoBehaviour {

    public GameObject gameManger;

    private void Awake()
    {
        if (GameManager.GM == null) { Instantiate(gameManger); }
    }

}
