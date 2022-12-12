using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkData : NetworkBehaviour {
    [SyncVar]
    public bool isGameInProgress = false;
    [SyncVar]
    public bool gameFinished = false;

    [SyncVar] public int turn = 0;

    [SyncVar] public int turnPerCube;

    public GameObject cube;

    [SyncVar(hook="LevelChange")] public int level = 0;

    public int GetTurn() {
        return turn;
    }
    
    public Color[] colors = { Color.red, Color.green, Color.cyan, Color.yellow };
    
    public string GetColor(int i) {
        Color c = colors[i % colors.Length];
        if (c == Color.red) {
            return "PIROS";
        } else if (c == Color.cyan) {
            return "KÉK";
        } else if (c == Color.green) {
            return "ZÖLD";
        } else if (c == Color.yellow) {
            return "SÁRGA";
        } else {
            return "NULL";
        }
    }

    void LevelChange(int l) {
        Debug.Log("Level Changed: " + l);
    }
}
