using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CubeHUD : MonoBehaviour {
    private NetworkManager nm;

    private void Awake() {
        nm = FindObjectOfType<NetworkManager>();
    }

    private NetworkMessageDelegate handler;

    public void SetHandler(NetworkMessageDelegate h) {
        handler = h;
    }
    
    public void SetIP(string s) {
        nm.networkAddress = s;
    }
    
    public void SetPort(int s) {
       nm.networkPort = s;
    }

    public void HostServer() {
        if (!this.nm.IsClientConnected() && !NetworkServer.active &&  this.nm.matchMaker == null)
        nm.StartHost();
    }

    public void JoinServer() {
        Debug.Log("Joining to " + nm.networkAddress + " and " + nm.networkPort);
        if (!this.nm.IsClientConnected() && !NetworkServer.active && this.nm.matchMaker == null) {
            NetworkClient nc = nm.StartClient();
            nc.RegisterHandler(MsgType.Disconnect, handler);
            nc.RegisterHandler(MsgType.Error, handler);
        }
    }
}
