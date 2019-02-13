using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaLog
{
    private string areaName;
    private int fails;
    private int attemptTimeSeconds;

    public AreaLog(string name)
    {
        areaName = name;
        fails = 0;
        attemptTimeSeconds = 0;
    }

    public void AddFail()
    {
        fails += 1;
    }
}

/// <summary>
/// Script to output events from the playtest.
/// </summary>
public class LoggingScript : MonoBehaviour {

    public static LoggingScript LS;

    private void Awake()
    {
        if(LS != null) { GameObject.Destroy(LS); }
        else { LS = this; }

        DontDestroyOnLoad(LS);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
