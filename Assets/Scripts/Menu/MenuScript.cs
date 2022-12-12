using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {
    private static readonly int Animation = Animator.StringToHash("animation");
    private static readonly int Backwards = Animator.StringToHash("backwards");
    
    public GameObject mainMenu;
    public GameObject serverFinder;
    public GameObject serverList;
    public CubeServer serverListServer;
    public RectTransform content;

    public GameObject error;
    public TextMeshProUGUI errorMsg;
    
    public TMP_InputField ip;
    public TMP_InputField port;
    
   /* public GameObject options;
    public GameObject optionsRight;
    
    public TextMeshProUGUI r;
    public TextMeshProUGUI g;
    public TextMeshProUGUI b;

    private Material selectedMaterial;*/

   void Start() {
       serverFinder.SetActive(false);
       error.SetActive(false);
   }
  
    public void ResetPlayerPrefs() {
        PlayerPrefs.DeleteAll();
    }

    public void ToggleServerFinder() {
        serverFinder.SetActive(!serverFinder.activeSelf);
        /*if (serverFinder.activeSelf) {
            RefreshServersContent();
        }*/
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void Settings() {
        GetComponent<Animator>().SetTrigger(Animation);
        GetComponent<Animator>().SetBool(Backwards, !GetComponent<Animator>().GetBool(Backwards));
        mainMenu.SetActive(false);
        /*options.SetActive(false);
        optionsRight.SetActive(false);*/
        //StartCoroutine(ShowUI());
    }

    private bool IsAtSettings() {
        return !GetComponent<Animator>().GetBool(Backwards);
    }

    /*IEnumerator ShowUI() {
        yield return new WaitForSeconds(1f);
        if (IsAtSettings()) {
            options.SetActive(true);
        } else {
            mainMenu.SetActive(true);
        }
    }*/

    /*public void ShowRightOptions(Material mat) {
        selectedMaterial = mat;

        UpdateText();
        
        optionsRight.SetActive(true);
    }

    public void AddR(int i) {
        AddColor(i, 0, 0);
    }
    
    public void AddG(int i) {
        AddColor(0, i, 0);
    }
    
    public void AddB(int i) {
        AddColor(0, 0, i);
    }

    private void AddColor(float r, float g, float b) {
        Color c = new Color(Mathf.Clamp(selectedMaterial.color.r + r/255, 0, 255), Mathf.Clamp(selectedMaterial.color.g + g/255, 0, 255), Mathf.Clamp(selectedMaterial.color.b + b/255, 0, 255));
        selectedMaterial.color = c;
        UpdateText();
    }

    private void UpdateText() {
        r.text = selectedMaterial.color.r * 255 + "";
        g.text = selectedMaterial.color.g * 255 + "";
        b.text = selectedMaterial.color.b * 255 + "";
    }*/

    void OnDisconnect(NetworkMessage msg) {
        var message = msg.ReadMessage<ErrorMessage>();
        ShowError(message.errorCode + "");
    }

    public void ShowError(String error) {
        this.error.SetActive(true);
        errorMsg.text = "Hiba: " + error;
    }

    public void HideError() {
        error.SetActive(false);
    }
    
    public void HostServer() {
        FindObjectOfType<CubeHUD>().SetIP(ip.text);
        FindObjectOfType<CubeHUD>().SetPort(int.Parse(port.text));
        FindObjectOfType<CubeHUD>().HostServer();
    }

    public void JoinServer() {
        FindObjectOfType<CubeHUD>().SetIP(ip.text);
        FindObjectOfType<CubeHUD>().SetPort(int.Parse(port.text));
        FindObjectOfType<CubeHUD>().SetHandler(OnDisconnect);
        FindObjectOfType<CubeHUD>().JoinServer();
    }
/*
    private void RefreshServersContent() {
        content.sizeDelta = new Vector2(content.sizeDelta.x, 90);
        foreach(Transform child in content) {
            print("Child : " + child.gameObject.name);
            Destroy(child.gameObject);
        }
    }

    private void AddServer(String ip, int port, int players) {
        CubeServer cs = Instantiate(serverListServer);
        cs.currentPlayers = players;
        cs.ip = ip;
        cs.port = port;
        cs.UpdateInfo();
        GameObject go = cs.gameObject;
        go.transform.SetParent(serverList.transform, false);
        content.sizeDelta = new Vector2(content.sizeDelta.x, content.sizeDelta.y + 90);
    }*/
}
