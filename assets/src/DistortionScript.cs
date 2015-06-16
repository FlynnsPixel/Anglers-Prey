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
		++timer;
		if (timer >= 200) {
			timer = 0;
			speed = Random.Range(-.05f, .05f);
		}
		rand += speed;
		GetComponent<Renderer>().material.SetFloat("rand", rand);
	}
}
