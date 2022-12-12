using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class NetworkActor : NetworkBehaviour {

    [SerializeField] [Range(1, 20)] public int maxLives = 10;
    [SerializeField] [Range(10, 30)] public int turnTimer = 30;
    [SerializeField] [Range(1, 10)] public int turnPerCube = 3;

    void Update() {
        foreach(CubeScript script in FindObjectsOfType<CubeScript>()) {
            if (script.transform.position.y < -5) {
                Destroy(script.gameObject);
            }
        }
    }
    
    public void OnPlayerConnect(NetworkIdentity ni) {
        if (GetCurrentPlayers() > 4
                || FindObjectOfType<NetworkData>().isGameInProgress
                || FindObjectOfType<NetworkData>().gameFinished) {
            ni.connectionToClient.Disconnect();
        } else {
            ni.GetComponent<PlayerScript>().id = GetId();
        }
    }

    public void Vote(PlayerScript voter) {
        if (voter.isServer) {
            StartGame();
        } else {
            int requiredVotes = GetCurrentPlayers() - 1;
            int currentVotes = 0;
            foreach (PlayerScript ps in FindObjectsOfType<PlayerScript>()) {
                if (ps.hasVoted) {
                    currentVotes++;
                }
            }

            if (currentVotes >= requiredVotes) {
                StartGame();
            }
        }
    }

    void StartGame() {
        if (FindObjectOfType<NetworkData>().isGameInProgress) {
            return;
        }
        FindObjectOfType<NetworkData>().isGameInProgress = true;
        FindObjectOfType<NetworkData>().turnPerCube = turnPerCube;
        
        print("Starting Game");
        
        foreach (PlayerScript ps in FindObjectsOfType<PlayerScript>()) {
            ps.waitTilCubeSpawn = turnPerCube;
            ps.livesLeft = maxLives;
            ps.RpcBeginGame();
            ps.RpcUpdateTurn(FindObjectOfType<NetworkData>().turn);
            ps.RpcSpawnCube(FindSpawnPoint(ps.id));
        }
        SetNextTurn(false);
    }

    private int GetCurrentPlayers() {
        return FindObjectsOfType<PlayerScript>().Length;
    }

    private int GetId() {
        int id = Random.Range(0, 4);
        foreach (PlayerScript ps in FindObjectsOfType<PlayerScript>()) {
            if (ps.id == id) {
                return GetId();
            }
        }
        return id;
    }

    public List<GameObject> CanCubeMove(GameObject go, List<GameObject> list, int x, int z) {
        if (list == null) {
            list = new List<GameObject>();
        }

        if (!list.Contains(go)) {
            list.Add(go);
        }

        Collider[] colliders = Physics.OverlapSphere(new Vector3(go.transform.position.x + x, go.transform.position.y, go.transform.position.z + z), 0.25f);
        if (colliders.Length == 0) {
            return list;
        } else {
            foreach (Collider col in colliders) {
                if (col.gameObject.layer == 9) {
                    return null;
                    //return new List<GameObject>();
                }

                if (col.gameObject.CompareTag("Cube")) {
                    return CanCubeMove(col.gameObject, list, x, z);
                }
            }
        }

        return list;
    }

    public Vector3 FindSpawnPoint(int id) {
        foreach(SpawnPoint go in FindObjectsOfType<SpawnPoint>()) {
            Debug.Log("Found spawnpoint " + go.name + " with id " + go.id);
            if (go.id == id) {
                return go.transform.position;
            }
        }

        return new Vector3(0, 2, 0);
    }

    public void SetNextTurn(bool spawnCube) {
        if (CheckWinner(false) != null) {
            return;
        }

        IncreaseTurn();
        if (FindObjectOfType<NetworkData>().turn < 4) {
            foreach (PlayerScript go in FindObjectsOfType<PlayerScript>()) {
                if (go.id == FindObjectOfType<NetworkData>().turn) {
                    foreach (PlayerScript ps in FindObjectsOfType<PlayerScript>()) {
                        if (ps.id == FindObjectOfType<NetworkData>().turn) {
                            ps.MyTurn();
                        }
                        ps.RpcUpdateTurn(FindObjectOfType<NetworkData>().turn);
                    }

                    int cubesAvailable = 0;
                    foreach(GameObject gcb in GameObject.FindGameObjectsWithTag("Cube")) {
                        if (gcb.transform.parent.GetComponent<PlayerScript>().id == FindObjectOfType<NetworkData>().turn
                            && gcb.transform.position.y >= 0.8) {
                            cubesAvailable++;
                        }
                    }

                    if (cubesAvailable == 0
                        && spawnCube) {
                        foreach (PlayerScript ps in FindObjectsOfType<PlayerScript>()) {
                            if (ps.id == FindObjectOfType<NetworkData>().turn) {
                                ps.waitTilCubeSpawn = -100;
                                break;
                            }
                        }
                    }
                    if(spawnCube) go.CmdSpawnCube();
                    return;
                }
            }
            SetNextTurn();
        } else {
            FindObjectOfType<NetworkData>().turn = -1;
            SetNextTurn();
        }
    }

    public void IncreaseTurn() {
        FindObjectOfType<NetworkData>().turn++;
        FindObjectOfType<NetworkData>().turn %= 4;
        foreach (PlayerScript ps in FindObjectsOfType<PlayerScript>()) {
            if (ps.id == FindObjectOfType<NetworkData>().turn) {
                if (ps.livesLeft <= 0) {
                    IncreaseTurn();
                }
                break;
            }
        }
    }
    
    public void SetNextTurn() {
        SetNextTurn(true);
    }

    public PlayerScript CheckWinner(bool disconnect) {
        int peopleWithLives = 0;
        if (disconnect) {
            peopleWithLives = -1;
        }
        PlayerScript lastPlayer = null;
        foreach (PlayerScript go in FindObjectsOfType<PlayerScript>()) {
            if (go.livesLeft > 0) {
                peopleWithLives++;
                lastPlayer = go;
            }
        }

        if (peopleWithLives <= 1
            || GetCurrentPlayers() <= 1) {
            if (lastPlayer == null) {
                
                Debug.Log("FINAL PLAYEEER IIIDDDD: wtf its null");
            } else
            Debug.Log("FINAL PLAYEEER IIIDDDD: " + lastPlayer.id);
            EndGame(lastPlayer);
            return lastPlayer;
        }

        return null;
    }

    public void EndGame(PlayerScript winner) {
        Debug.Log("FINAL PLAYEEER IIIDDDD from here: " + winner.id);
        FindObjectOfType<NetworkData>().isGameInProgress = false;
        FindObjectOfType<NetworkData>().gameFinished = true;
        foreach (PlayerScript go in FindObjectsOfType<PlayerScript>()) {
            go.RpcShowEndScreen(winner.id);
        }
        StartCoroutine(StopServer());
    }

    IEnumerator StopServer() {
        yield return new WaitForSeconds(10.0f);
        NetworkServer.DisconnectAll();
    }
}
