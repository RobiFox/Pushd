using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking.Types;

public class CubeServer : MonoBehaviour {
	public string ip;
	public int port;
	public int currentPlayers;

	public TextMeshProUGUI ipText;
	public TextMeshProUGUI playersText;
	public GameObject joinButton;

	public void UpdateInfo() {
		ipText.text = ip + ":" + port;
		playersText.text = "Játékosok: " + currentPlayers + "/4";
		joinButton.SetActive(currentPlayers < 4);
	}
}
