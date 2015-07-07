using UnityEngine;
using System.Collections;

public class Intro : MonoBehaviour {

	public Vector3 cam_pos;

	void Start() {
		cam_pos = transform.position;
	}

	void Update() {
		cam_pos.y += Mathf.Sin(Time.timeSinceLevelLoad * 2.0f) / 60.0f;
		transform.position = cam_pos;
	}
}
