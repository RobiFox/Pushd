using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MusicManager : MonoBehaviour {

	public AudioClip[] musics;
	private AudioSource source;
	
	// Use this for initialization
	void Start () {
		Debug.Log(Resources.FindObjectsOfTypeAll<MusicManager>().Length + "");
		if (Resources.FindObjectsOfTypeAll<MusicManager>().Length > 1) {
			Destroy(this);
			return;
		}
		DontDestroyOnLoad(this);
		gameObject.SetActive(PlayerPrefs.GetInt("Settings_Music", 1) == 1);
		source = GetComponent<AudioSource>();
		SetVolume(PlayerPrefs.GetFloat("SettingsSlider_MusicVolume", 0.25f));
		StartCoroutine(PlayMusic());
	}

	private void OnDisable() {
		StopCoroutine(PlayMusic());
	}

	private void OnEnable() {
		if(source != null)
			StartCoroutine(PlayMusic());
	}

	IEnumerator PlayMusic() {
		source.clip = musics[Random.Range(0, musics.Length)];
		source.Play();
		yield return new WaitForSeconds(source.clip.length);
		StartCoroutine(PlayMusic());
	}

	public void SetVolume(float volume) {
		source.volume = Mathf.Clamp(volume, 0, 1);
	}
}
