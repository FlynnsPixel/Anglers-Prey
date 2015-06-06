using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	Vector3 pos;
	Quaternion rota;
	Vector2 accel;
	float angle = 90;
	float angle_accel = 0;

	Vector3 cam_pos;

	const float radians = Mathf.PI / 180.0f;
	const float max_speed = .25f;
	const float friction = .98f;
	const float angle_accel_speed = .15f;
	const float angle_friction = .95f;
	const float max_angle_accel = 4;

	void Start() {
		pos = transform.position;
		rota = transform.rotation;
		cam_pos = Camera.main.transform.position;
	}

	void Update() {
		Debug.Log(angle_accel);
		if (Input.GetKey(KeyCode.W)) {
			accel.x -= Mathf.Cos(angle * radians) * .01f;
			accel.y -= Mathf.Sin(angle * radians) * .01f;
			accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
			accel.y = Mathf.Clamp(accel.y, -max_speed, max_speed);
		}
		accel *= friction;
		pos.x += accel.x;
		pos.z += accel.y;

		if (Input.GetKey(KeyCode.A)) {
			angle_accel += angle_accel_speed;
		}else if (Input.GetKey(KeyCode.D)) {
			angle_accel -= angle_accel_speed;
		}
		angle_accel = Mathf.Clamp(angle_accel, -max_angle_accel, max_angle_accel);
		angle_accel *= angle_friction;
		angle += angle_accel;

		transform.position = pos;
		transform.Rotate(0, 0, angle_accel);

		cam_pos.x -= (cam_pos.x - pos.x) / 10.0f;
		cam_pos.z -= (cam_pos.z - pos.z) / 10.0f;
		Camera.main.transform.position = cam_pos;
	}
}
