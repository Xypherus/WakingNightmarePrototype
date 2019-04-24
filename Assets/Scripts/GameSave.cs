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

    List<SafeZoneScript> safeZones = new List<SafeZoneScript>();

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

        DontDestroyOnLoad(gameObject);

    }

    private void Start()
    {
        safeZones = new List<SafeZoneScript>(FindObjectsOfType<SafeZoneScript>());

        //temp code
        //LoadGame();
    }

    private void Update()
    {
        //test code
        if (Input.GetKeyDown(KeyCode.M)) { LoadGame(); }
    }

    public static void SaveGame(Vector2 playerposition)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string filePath = Application.persistentDataPath + "/saves/" + DateTime.Now.ToString("yy_MM_dd");

        List<int> usableSafeZones = new List<int>();
        foreach (SafeZoneScript zone in gameSaver.safeZones)
        {
            if (zone.useable && !usableSafeZones.Contains(gameSaver.safeZones.IndexOf(zone))) { usableSafeZones.Add(gameSaver.safeZones.IndexOf(zone)); }
        }

        SaveData data = new SaveData(playerposition, SceneManager.GetActiveScene().buildIndex, usableSafeZones);
        if (!Directory.Exists(Application.persistentDataPath + "/saves")) { Directory.CreateDirectory(Application.persistentDataPath + "/saves"); }

        FileStream file = new FileStream(filePath, FileMode.Create);

        formatter.Serialize(file, data);
        file.Close();
        if(File.Exists(filePath)) { Debug.Log("Created save file at " + filePath); }
        else { Debug.LogError("could not create file at " + filePath); }
    }

    public static void LoadGame()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        SaveData data = (SaveData)formatter.Deserialize(GetLatestSave());

        gameSaver.StartCoroutine(LoadLevel(data.sceneNumber, () => {
            //Find object of type player controller
            //Then delete those characters
            foreach (PlayerController player in FindObjectsOfType<PlayerController>()) {
                Destroy(player.gameObject);
            }

            Instantiate(Resources.Load<GameObject>("Characters/Thomas"), SerializableVector.GetUnityVector(data.playerPosition), Quaternion.identity);
            Instantiate(Resources.Load<GameObject>("Characters/Olivia"), SerializableVector.GetUnityVector(data.playerPosition), Quaternion.identity);

            FindObjectOfType<NavmeshArea2D>().InitializeGrid();
            
            foreach (SafeZoneScript zone in gameSaver.safeZones)
            {
                if (data.usableSafeZones.Contains(gameSaver.safeZones.IndexOf(zone))) { zone.useable = true; }
                else
                {
                    zone.useable = false;
                }
            }
        }));

    }

    public static IEnumerator LoadLevel(int buildIndex, UnityEngine.Events.UnityAction callback) {
        yield return SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);

        callback();
    }

    public static FileStream GetLatestSave()
    {
        DirectoryInfo d = new DirectoryInfo(Application.persistentDataPath + "/saves");
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
    public List<int> usableSafeZones = new List<int>();

    public SaveData(Vector2 playerPosition, int sceneNumber, List<int> usableSafeZones)
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