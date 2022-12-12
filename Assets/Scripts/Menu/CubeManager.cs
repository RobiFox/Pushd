using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CubeManager : NetworkManager {
    public NetworkDiscovery discovery;

    public override void OnStartHost()
    {
        discovery.Initialize();
        discovery.StartAsServer();

    }

    public override void OnStopClient() {
        discovery.StopBroadcast();
    }
}
