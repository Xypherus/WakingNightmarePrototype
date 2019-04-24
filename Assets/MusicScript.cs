using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicScript : MonoBehaviour {

    public GameObject FearObject;
    private BoxCollider2D BoxCol;
    public bool inbounds = false;

    [FMODUnity.EventRef]
    public string PlayerStateEvent;
    FMOD.Studio.EventInstance playerState;
    float Fear = 0.0f;
    void Start()
    {
        BoxCol = FearObject.GetComponent<BoxCollider2D>();
        playerState = FMODUnity.RuntimeManager.CreateInstance(PlayerStateEvent);
        playerState.start();
    }
    void Update()
    {
        playerState.setParameterValue("Fear", Fear);
        Debug.Log(Fear);
        if (inbounds == true)
        {
            Fear = Fear + 0.01f;
        }
        else
        {
            Fear = Fear - 0.01f;
        }
        if (Fear <= 0)
        {
            Fear = 0;
        }
        if (Fear >= 15)
        {
            Fear = 15;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter");
        inbounds = true;
    }
    private void OnTriggerExit(Collider other)
    {
        inbounds = false;
    }
}

