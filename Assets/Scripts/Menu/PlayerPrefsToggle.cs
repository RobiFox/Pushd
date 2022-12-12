using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPrefsToggle : MonoBehaviour {

    public string prefsName;
    public int defaultValue;
        
    void Start() {
        GetComponent<Toggle>().onValueChanged.AddListener(ValueChanged);
        GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("Settings_" + prefsName, defaultValue) != 0;
    }

    void ValueChanged(bool value) {
        PlayerPrefs.SetInt("Settings_" + prefsName, value ? 1 : 0);
    }
}
