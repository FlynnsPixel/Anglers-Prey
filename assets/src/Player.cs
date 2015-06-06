using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	Vector3 pos;
	Vector2 accel;
	float angle = 90;

	const float radians = Mathf.PI / 180.0f;
	const float max_speed = .25f;
	const float friction = .98f;
	const float turn_speed = 2;

	void Start() {
		pos = transform.position;
	}

	void Update() {
		if (Input.GetKey(KeyCode.W)) {
			accel.x -= Mathf.Cos(angle * radians) * .01f;
			accel.y -= Mathf.Sin(angle * radians) * .01f;
			accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
			accel.y = Mathf.Clamp(accel.y, -max_speed, max_speed);
		}
		if (Input.GetKey(KeyCode.A)) {
			transform.Rotate(0, 0, turn_speed);
			angle += turn_speed;
		}else if (Input.GetKey(KeyCode.D)) {
			transform.Rotate(0, 0, -turn_speed);
			angle -= turn_speed;
		}
		accel *= friction;

		pos.x += accel.x;
		pos.z += accel.y;

		transform.position = pos;
	}
}
