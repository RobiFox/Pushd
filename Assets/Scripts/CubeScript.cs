using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CubeScript : NetworkBehaviour {
    [ClientRpc]
    public void RpcDestroy() {
        Destroy(this);
    }
}
