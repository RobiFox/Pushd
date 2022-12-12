using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeChanger : MonoBehaviour {
    void Start() {
        GetComponent<Slider>().onValueChanged.AddListener(ValueChange);
    }

    void ValueChange(float f) {
        FindObjectOfType<MusicManager>().SetVolume(f);
    }
}
