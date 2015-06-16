using UnityEngine;
using System.Collections;

public class DistortionScript : MonoBehaviour {

	int timer = 0;
	float rand;
	float speed = 0;

	void Start() {
		rand = Random.Range(0, Mathf.PI);
		GetComponent<Renderer>().material.SetFloat("rand", rand);
	}

	void Update() {

	}
}
