using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerScript : NetworkBehaviour {

	[SyncVar(hook = "OnIdChange")]
	public int id;

	[SyncVar]
	public bool hasVoted = false;

	[SyncVar(hook="OnLifeChange")] 
	public int livesLeft = 20;

	[SyncVar] public int waitTilCubeSpawn;

	private GameObject voteHud;
	private GameObject gameHud;
	private GameObject endHud;
	private GameObject tutorialPanel;
	private GameObject button;
	private GameObject nextMap;
	private GameObject prevMap;
	private GameObject serverMaster;
	private GameObject exitButton;
	private TextMeshProUGUI voteText;
	private TextMeshProUGUI playerCounter;
	private TextMeshProUGUI gameHudTurnText;
	private TextMeshProUGUI livesLeftText;
	private TextMeshProUGUI winnerText;
	private TextMeshProUGUI tutorialText;

	private bool gameActive = false;

	public AudioClip blockMove;
	public AudioClip lifeLost;
	public AudioClip victory;
	public AudioClip defeat;
	
	private GameObject levelsList;
	private Level[] levels;
	
	void Start () {
		if (isLocalPlayer) {
			voteHud = GameObject.FindGameObjectWithTag("Vote Hud");
			gameHud = GameObject.FindGameObjectWithTag("Game Hud");
			endHud = GameObject.FindGameObjectWithTag("End Hud");
			button = GameObject.FindGameObjectWithTag("Vote Button");
			nextMap = GameObject.FindGameObjectWithTag("Next Map");
			prevMap = GameObject.FindGameObjectWithTag("Previous Map");
			tutorialPanel = GameObject.FindGameObjectWithTag("Tutorial Panel");
			serverMaster = GameObject.FindGameObjectWithTag("Server Master");
			voteText = GameObject.FindGameObjectWithTag("Vote Text").GetComponent<TextMeshProUGUI>();
			gameHudTurnText = GameObject.FindGameObjectWithTag("Turn Text").GetComponent<TextMeshProUGUI>();
			playerCounter = GameObject.FindGameObjectWithTag("Player Counter").GetComponent<TextMeshProUGUI>();
			livesLeftText = GameObject.FindGameObjectWithTag("Lives Text").GetComponent<TextMeshProUGUI>();
			winnerText = GameObject.FindGameObjectWithTag("Winner Text").GetComponent<TextMeshProUGUI>();
			tutorialText = GameObject.FindGameObjectWithTag("Tutorial Text").GetComponent<TextMeshProUGUI>();
			exitButton = GameObject.FindGameObjectWithTag("Exit Button");
			tutorialPanel.SetActive(false);
			voteHud.SetActive(false);
			gameHud.SetActive(false);
			endHud.SetActive(false);
			serverMaster.SetActive(false);
			button.GetComponent<Button>().onClick.AddListener(Vote);
			
			print(GameObject.FindGameObjectWithTag("Level List") + "");
			levelsList = GameObject.FindGameObjectWithTag("Level List");
			levels = levelsList.GetComponentsInChildren<Level>(true);

			if (isServer) {
				Debug.Log("Am the server");
				nextMap.GetComponent<Button>().onClick.AddListener(NextMapChange);
				prevMap.GetComponent<Button>().onClick.AddListener(PrevMapChange);
				if (PlayerPrefs.GetInt("Settings_RandomMap", 0) == 1) {
					CmdChangeLevel(Random.Range(0, levels.Length));
				} else {
					CmdChangeLevel(0);
				}

				exitButton.GetComponent<Button>().onClick.AddListener(StopHost);
			} else {
				foreach (Level go in levels) {
					go.gameObject.SetActive(false);
				}

				levels[FindObjectOfType<NetworkData>().level].gameObject.SetActive(true);
				
				exitButton.GetComponent<Button>().onClick.AddListener(StopClient);
			}
		}
	}

	private void StopHost() {
		NetworkManager.singleton.StopHost();
	}
	
	private void StopClient() {
		NetworkManager.singleton.StopClient();
	}

	private void NextMapChange() {
		CmdChangeLevel(FindObjectOfType<NetworkData>().level + 1);
	}
	private void PrevMapChange() {
		CmdChangeLevel(FindObjectOfType<NetworkData>().level - 1);
	}

	[Command]
	public void CmdChangeLevel(int i) {
		if (!isServer) {
			return;
		}

		//int map = i % levels.Length;
		int map = (i % levels.Length + levels.Length) % levels.Length;
		Debug.Log("Loading level: " + map);
		FindObjectOfType<NetworkData>().level = map;
		/*foreach (Level go in levels) {
			//go.RpcToggle(false);
		}*/
		//levels[map].RpcToggle(true);
		foreach (PlayerScript ps in FindObjectsOfType<PlayerScript>()) {
			ps.RpcToggle(map);
		}
	}

	[ClientRpc]
	public void RpcToggle(int level) {
		if (isLocalPlayer) {
			foreach (Level go in levels) {
				go.gameObject.SetActive(false);
			}
			levels[level].gameObject.SetActive(true);
		}
	}
	
	void Update() {
		if (isLocalPlayer) {
			if (!gameActive) {
				int players = FindObjectsOfType<PlayerScript>().Length;
				if (voteHud.activeSelf != (players >= 2)) {
					voteHud.SetActive(players >= 2);
					if (isServer) {
						voteText.text = "START";
						serverMaster.SetActive(true);
					}
				}

				if (button.activeSelf) {
					playerCounter.text = "Játékosok: " + players + "/4";
				}

				return;
			}

			// up -> -x
			// right -> +z
			
			if (Input.GetKeyDown(KeyCode.UpArrow)) {
				CmdMoveCubes(-1, 0);
				HideTutorial();
			} else if (Input.GetKeyDown(KeyCode.RightArrow)) {
				CmdMoveCubes(0, 1);
				HideTutorial();
			} else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
				CmdMoveCubes(0, -1);
				HideTutorial();
			} else if (Input.GetKeyDown(KeyCode.DownArrow)) {
				CmdMoveCubes(1, 0);
				HideTutorial();
			}
			
			/*foreach(CubeScript script in FindObjectsOfType<CubeScript>()) {
				if (script.transform.position.y < -5) {
					Destroy(script.gameObject);
				}
			}*/
			foreach(CubeScript cs in FindObjectsOfType<CubeScript>()) {
				RaycastHit hit;
				if (cs.transform.position.y < -5) {
					Destroy(cs.gameObject);
				} else if (!Physics.Raycast(cs.transform.position, transform.TransformDirection(Vector3.down), out hit,
					Mathf.Infinity)
						&& cs.gameObject.GetComponent<Rigidbody>() == null) {
					cs.gameObject.AddComponent<Rigidbody>();
				}
			}
		}
	}
	
	[Command]
	void CmdMoveCubes(int x, int z) {
		if (x == z
		    || Math.Abs(x) > 1
		    || Math.Abs(z) > 1
		    || FindObjectOfType<NetworkData>().GetTurn() != id
		    || FindObjectOfType<NetworkData>().gameFinished) {
			return;
		}

		waitTilCubeSpawn--;
		List<GameObject> moveablesDupes = new List<GameObject>();
		for (int i = 0; i < transform.childCount; i++) {
			GameObject go = transform.GetChild(i).gameObject;
			List<GameObject> gml = FindObjectOfType<NetworkActor>().CanCubeMove(go, null, x, z);
			if (gml != null) {
				moveablesDupes.AddRange(gml);
			}
		}

		List<GameObject> moveables = moveablesDupes.Distinct().ToList();
		/*foreach(GameObject go in moveables) {
			if (go.transform.parent.GetComponent<PlayerScript>().id != id) {
				finalMoveables.Add(go);
			}
		}*/
		int cubesMoved = 0;
		foreach(GameObject go in moveables) {
			try {
				var parent = go.transform.parent;
				parent.GetComponent<PlayerScript>().RpcMove(go.transform.GetSiblingIndex(), x, z);
				cubesMoved++;
			} catch (Exception) {
				
			}
		}

		if (cubesMoved > 0) {
			foreach (PlayerScript ps in FindObjectsOfType<PlayerScript>()) {
				ps.RpcMoveSound();
			}

			FindObjectOfType<NetworkActor>().SetNextTurn();
		} else {
			if (PlayerPrefs.GetInt("TutorialFailMove", 0) == 0) {
				PlayerPrefs.SetInt("TutorialFailMove", 1);
				tutorialPanel.SetActive(true);
				tutorialText.text = "Egyik kockád sem tudott megmozdulni!\nEz azt jelenti, hogy még mindig te következel, amíg nem mozdulsz el egy olyan irányba, amerre legalább egy kockád elmozdul.";
			}
		}
	}

	public override void OnStopAuthority() {
		base.OnStopAuthority();
		CmdCheckWinner(true);
	}

	[ClientRpc]
	void RpcMoveSound() {
		if (isLocalPlayer
				&& !GetComponent<AudioSource>().isPlaying) {
			GetComponent<AudioSource>().PlayOneShot(blockMove);
		}
	}
	
	[ClientRpc]
	void RpcMove(int child, int x, int z) {
		GameObject go = transform.GetChild(child).gameObject;
		var position = go.transform.position;
		go.transform.position = new Vector3(position.x + x, position.y, position.z + z);
		
		RaycastHit hit;
		if (!Physics.Raycast(go.transform.position, transform.TransformDirection(Vector3.down), out hit,
			Mathf.Infinity)) {
			livesLeft--;
			if (isLocalPlayer) {
				CmdCheckWinner(false);
			}
		}

		/*for (int i = 0; i < transform.childCount; i++) {
			GameObject go = transform.GetChild(i).gameObject;
			print("Moving");
			go.GetComponent<Rigidbody>().AddForce(x * 5, 0, z * 5, ForceMode.VelocityChange);/
			//go.GetComponent<Rigidbody>().MovePosition(new Vector3(go.transform.position.x + x, go.transform.position.y, go.transform.position.z + z));
			//go.GetComponent<Rigidbody>().velocity = new Vector3(x * 3f, 0, z * 3f);
			//Vector3.MoveTowards(go.transform.position, new Vector3(go.transform.position.x + x, go.transform.position.y, go.transform.position.z + z), 1000 * Time.deltaTime);
		}*/
	}

	[Command]
	void CmdCheckWinner(bool dis) {
		FindObjectOfType<NetworkActor>().CheckWinner(dis);
	}

	[ClientRpc]
	public void RpcShowEndScreen(int winner) {
		if (isLocalPlayer) {
			print("Winner (from Rpc): " + winner);
			NetworkData nd = FindObjectOfType<NetworkData>();
			String color = nd.GetColor(winner);
			if (winner == id) {
				GetComponent<AudioSource>().PlayOneShot(victory);
				winnerText.text = "TE NYERTÉL!";
			} else {
				GetComponent<AudioSource>().PlayOneShot(defeat);
				winnerText.text = color + " NYERT!";
			}

			gameHud.SetActive(false);
			endHud.SetActive(true);
		}
	}

	[Command]
	void CmdRigidbodyServerSide(GameObject go) {
		
		RaycastHit hit;
		if (!Physics.Raycast(go.transform.position, transform.TransformDirection(Vector3.down), out hit,
			Mathf.Infinity)) {
			go.AddComponent<Rigidbody>();
		}
	}

	void OnLifeChange(int life) {
		if (isLocalPlayer
				&& gameActive) {
			Debug.Log(livesLeft + ", " + life);
			if (life < livesLeft) {
				if (PlayerPrefs.GetInt("Tutorial", 0) >= 3
						&& PlayerPrefs.GetInt("AlreadyDied", 0) == 0) {
					PlayerPrefs.SetInt("AlreadyDied", 1);
					tutorialPanel.SetActive(true);
					tutorialText.text = "Oof! Leestél, és ezzel veszítettél egy életet.\nHa az életed 0 -ra csökken, kiestél a játékból.";
				}
			}

			livesLeft = life;
			CmdChangeLife(life);
		}
	}

	[Command]
	public void CmdChangeLife(int life) {
		RpcChangeLife(life);
	}

	[ClientRpc]
	public void RpcChangeLife(int life) {
		if (isLocalPlayer) {
			GetComponent<AudioSource>().PlayOneShot(lifeLost);
			livesLeftText.text = "ÉLET: " + life;
		}
	}

	[Command]
	public void CmdBeginGame() {
		RpcBeginGame();
		livesLeft = GetComponent<NetworkActor>().maxLives;
	}
	
	[ClientRpc]
	public void RpcBeginGame() {
		if (isLocalPlayer) {
			gameActive = true;
			voteHud.SetActive(false);
			gameHud.SetActive(true);
			CmdChangeLife(livesLeft);
		}
	}

	[ClientRpc]
	public void RpcUpdateTurn(int i) {
		if (isLocalPlayer) {
			if (i == id) {
				gameHudTurnText.text = "TE JÖSSZ!";
			} else {
				gameHudTurnText.text = FindObjectOfType<NetworkData>().GetColor(i);
			}

			gameHudTurnText.color = FindObjectOfType<NetworkData>().colors[i % FindObjectOfType<NetworkData>().colors.Length];
		}
	}
	
	/*IEnumerator MoveCoroutine(int x, int z, Vector3 target) {
		for (int i = 0; i < transform.childCount; i++) {
			GameObject go = transform.GetChild(i).gameObject;
			go.transform.position = Vector3.MoveTowards(go.transform.position, );
		}
	}*/

	void Vote() {
		if (!hasVoted) {
			if (isLocalPlayer
					&& !isServer) {
				voteText.text = "SZAVAZTÁL";
			}
			CmdVote();
		}
	}

	[Command]
	void CmdVote() {
		hasVoted = true;
		FindObjectOfType<NetworkActor>().Vote(this);
	}
	
	[Command]
	void CmdOnPlayerConnect() {
		FindObjectOfType<NetworkActor>().OnPlayerConnect(GetComponent<NetworkIdentity>());
	}

	void OnIdChange(int i) {
		id = i;
	}
	
	public override void OnStartLocalPlayer() {
		base.OnStartLocalPlayer();
		CmdOnPlayerConnect();
	}
	
	[Command]
	public void CmdSpawnCube() {
		if (waitTilCubeSpawn <= 0) {
			Vector3 spawn = FindObjectOfType<NetworkActor>().FindSpawnPoint(id);
			if (Physics.OverlapSphere(spawn, 0.25f).Length == 0) {
				Debug.Log("Spawning Cube. Colliders: ");
				Debug.Log("Vector: " + spawn);
				waitTilCubeSpawn = FindObjectOfType<NetworkData>().turnPerCube;
				RpcSpawnCube(spawn);
				OnCubeSpawn();
			}
		}
	}

	[ClientRpc]
	public void RpcSpawnCube(Vector3 spawn) {
		if (Physics.OverlapSphere(spawn, 0.25f).Length > 0) return;
		GameObject obj = Instantiate(FindObjectOfType<NetworkData>().cube, transform);
		obj.transform.position = spawn;
		obj.GetComponent<MeshRenderer>().material.color = FindObjectOfType<NetworkData>().colors[id % FindObjectOfType<NetworkData>().colors.Length];
	}

	[ClientCallback]
	public void OnCubeSpawn() {
		if (PlayerPrefs.GetInt("Tutorial", 0) == 2) {
			PlayerPrefs.SetInt("Tutorial", 3);
			tutorialPanel.SetActive(true);
			tutorialText.text = "Megjelent egy kocka!\nMegadott körönként, megjelenik egy hozzád tartozó kocka (ha van hely!).\n\nVigyázz! Mozgáskor mindegyik kockád mozgatod.";
		}
	}

	[ClientCallback]
	public void MyTurn() {
		if (PlayerPrefs.GetInt("Tutorial", 0) == 0) {
			PlayerPrefs.SetInt("Tutorial", 1);
			tutorialPanel.SetActive(true);
			tutorialText.text = "Üdv! Ahogy látom elöször játszol.\nJelenleg te következel. A jelenlegi színedet a Bal Alsó sarokban lévö szöveg színe határozza meg, illetve azt is, hogy most ki megy. A jelenlegi színed " 
			                    + FindObjectOfType<NetworkData>().GetColor(id) + ". A nyilakkal tudsz mozogni. \n\nVigyázz, le ne ess!";
		}
	}

	private void HideTutorial() {
		tutorialPanel.SetActive(false);
		if (PlayerPrefs.GetInt("Tutorial", 0) == 1) {
			PlayerPrefs.SetInt("Tutorial", 2);
			tutorialPanel.SetActive(true);
			tutorialText.text = "Sikeresen megmozdultál!\nHa egy ellenség felé mozogsz, aki melletted van, azt a kockát is is eltolod.\nEzt használd ki arra, hogy lelökd a pályáról!\n\nA sötét szürke színü kockák megakadályozzák a mozgást.\nJelenleg várnod kell, hogy újra te következz.";
		}
	}
}
