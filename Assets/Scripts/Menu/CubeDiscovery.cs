using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CubeDiscovery : NetworkDiscovery {
    void Start() {
        showGUI = false;
    }
    
    public override void OnReceivedBroadcast(string fromAddress, string data) {
		Debug.Log("Received broadcast: " + fromAddress);
		Debug.Log("Data: " + data);
    }
}
