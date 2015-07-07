using UnityEngine;
using System.Collections;

public class Intro : MonoBehaviour {

	public Vector3 cam_pos;

	void Start() {
		cam_pos = transform.position;
	}

	void Update() {
		cam_pos.x -= (cam_pos.x - 12.6f) / 80.0f;
		cam_pos.y -= (cam_pos.y - 7.5f) / 80.0f;
		cam_pos.z -= (cam_pos.z - -14.2f) / 200.0f;
		transform.position = cam_pos;

		cam_pos.y += Mathf.Sin(Time.timeSinceLevelLoad / 1.5f) / 60.0f;
	}
}
