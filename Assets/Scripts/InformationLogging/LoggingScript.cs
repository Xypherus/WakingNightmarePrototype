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

    public string GetName()
    {
        return areaName;
    }
}

/// <summary>
/// Script to output events from the playtest.
/// </summary>
public class LoggingScript : MonoBehaviour {

    public static LoggingScript LS;

    List<AreaLog> areaLogs = new List<AreaLog>();

    private void Awake()
    {
        if(LS != null) { Destroy(gameObject); }
        else { LS = this; }

        DontDestroyOnLoad(LS);
    }

    public void LogFail(string name)
    {
        int logIndex = areaLogs.FindIndex(AreaLog => AreaLog.GetName() == name);
        if(logIndex != -1)
        {
            areaLogs[logIndex].AddFail();
        }
        else
        {
            areaLogs.Add(new AreaLog(name));
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
