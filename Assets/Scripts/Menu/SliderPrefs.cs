using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderPrefs : MonoBehaviour {
    
    public string prefsName;
    public float defaultValue;
        
    void Start() {
        GetComponent<Slider>().onValueChanged.AddListener(ValueChanged);
        GetComponent<Slider>().value = PlayerPrefs.GetFloat("SettingsSlider_" + prefsName, defaultValue);
    }

    void ValueChanged(float value) {
        PlayerPrefs.SetFloat("SettingsSlider_" + prefsName, value);
    }
}
