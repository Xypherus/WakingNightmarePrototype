using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to output events from the playtest.
/// </summary>
public class LoggingScript : MonoBehaviour {

    public static LoggingScript LS;

    public class AreaLog
    {
        private string areaName;
        private int fails;
        private int attemptTimeSeconds;

        public AreaLog(string name)
        {
            areaName = name;
            fails = 1;
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

        public override string ToString()
        {
            return "Area Name: " + areaName + " Fails: " + fails;
        }
    }

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
        else //Should never get called but just to be sure
        {
            areaLogs.Add(new AreaLog(name));
            areaLogs[areaLogs.Count - 1].AddFail();
        }
    }

    public void BuildLog(string name)
    {
        areaLogs.Add(new AreaLog(name));
    }

    private void OnApplicationQuit()
    {
        Debug.Log(DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
        string infoName = "UserLog: " + DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss");
        string path = "Assets/Scripts/InformationLogging/Output.txt";

        StreamWriter stream = new StreamWriter(path, true);
        stream.WriteLine(infoName);
        
        foreach(AreaLog log in areaLogs)
        {
            stream.WriteLine(log.ToString());
        }
        stream.WriteLine("");
        stream.WriteLine("");

        stream.Close();
    }
}
