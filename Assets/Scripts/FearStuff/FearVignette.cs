using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FearVignette : MonoBehaviour {

    Image image;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update () {
        float maxFear = PlayerSwapper.playerSwapper.currentPlayer.fearController.maxFear;
        float currentFear = PlayerSwapper.playerSwapper.currentPlayer.fearController.currentFear;

        image.sprite = PlayerSwapper.playerSwapper.currentPlayer.vingette;
        image.color = new Color(1, 1, 1, (currentFear / maxFear));
	}
}
