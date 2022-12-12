using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Level : NetworkBehaviour {
    [ClientRpc]
    public void RpcToggle(bool show) {
        gameObject.SetActive(show);
    }
}
