using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class GameSave : MonoBehaviour{

    public static GameSave gameSaver;

    public void Awake()
    {
        if (gameSaver)
        {
            Destroy(gameSaver);
            gameSaver = this;
        }
        else
        {
            gameSaver = this;
        }

    }

    public void SaveGame(Vector2 playerposition)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        List<SafeZoneScript> usableSafeZones = new List<SafeZoneScript>();
        foreach (SafeZoneScript zone in FindObjectsOfType<SafeZoneScript>())
        {
            if (zone.useable && !usableSafeZones.Contains(zone)) { usableSafeZones.Add(zone); }
        }

        SaveData data = new SaveData(playerposition, SceneManager.GetActiveScene().buildIndex, usableSafeZones);
        FileStream file = new FileStream(Application.persistentDataPath + "//saves//" + DateTime.Now.ToString("yy-MM-dd"), FileMode.Create);

        formatter.Serialize(file, data);
        file.Close();
    }

    public void LoadGame()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        SaveData data = (SaveData)formatter.Deserialize(GetLatestSave());

        SceneManager.LoadScene(data.sceneNumber);

        Instantiate(Resources.Load<GameObject>("Character//Thomas"), SerializableVector.GetUnityVector(data.playerPosition), Quaternion.identity);
        Instantiate(Resources.Load<GameObject>("Character//Olivia"), SerializableVector.GetUnityVector(data.playerPosition), Quaternion.identity);

        foreach (SafeZoneScript zone in FindObjectsOfType<SafeZoneScript>())
        {
            if (data.usableSafeZones.Contains(zone)) { zone.useable = true; }
            else
            {
                zone.useable = false;
                //zone.SetActive(false); //delete this line if loading zones is weird
            }
        }
    }

    public FileStream GetLatestSave()
    {
        DirectoryInfo d = new DirectoryInfo(Application.persistentDataPath + "//saves");
        d.GetDirectories().OrderByDescending(f => d.CreationTime).Select(f => d.Name).ToList();//sort directoy by date created
        FileInfo[] files = d.GetFiles();

        FileStream file = new FileStream(files[0].FullName, FileMode.OpenOrCreate);
        return file;
    }
}

//Player position
//Scene build index
//List of unusable safe zones

[System.Serializable]
public class SaveData
{
    public SerializableVector playerPosition;
    public int sceneNumber;
    public List<SafeZoneScript> usableSafeZones;

    public SaveData(Vector2 playerPosition, int sceneNumber, List<SafeZoneScript> usableSafeZones)
    {
        this.playerPosition = new SerializableVector(playerPosition);
        this.sceneNumber = sceneNumber;
        this.usableSafeZones = usableSafeZones;
    }
}

[System.Serializable]
public struct SerializableVector
{
    public float x;
    public float y;

    public SerializableVector(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public SerializableVector(Vector2 position)
    {
        x = position.x;
        y = position.y;
    }
    public static Vector2 GetUnityVector(SerializableVector vec)
    {
        Vector2 unityVector = new Vector2(vec.x, vec.y);
        return unityVector;
    }
}