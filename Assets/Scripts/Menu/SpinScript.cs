using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinScript : MonoBehaviour {
	// Update is called once per frame
	void Update () {
		transform.Rotate(Vector3.up * Time.deltaTime * 5);
	}
}
