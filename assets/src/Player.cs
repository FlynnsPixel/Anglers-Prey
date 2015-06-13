using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	Vector3 pos;
	Quaternion rota;
	Vector3 rota_euler;
	Vector2 accel;
	float angle = 90;
	float angle_accel = 0;

	GameObject map;
	Mesh map_mesh;
	Vector3[] map_vertices;
	Rect map_bounds;

	Vector3 cam_pos;

	const float radians = Mathf.PI / 180.0f;
	const float max_speed = .175f;
	const float friction = .98f;
	const float angle_accel_speed = .15f;
	const float angle_friction = .95f;
	const float max_angle_accel = 4;

	int timer = 0;

	void Start() {
		pos = transform.position;
		rota = transform.rotation;
		rota_euler.x = 90;
		rota_euler.y = -90;
		rota_euler.z = -90;
		cam_pos = Camera.main.transform.position;

		map = GameObject.Find("map");
		map_mesh = map.GetComponent<MeshFilter>().mesh;
		map_bounds.x = map.transform.position.x;
		map_bounds.y = map.transform.position.z;
		map_bounds.width = map.transform.localScale.x;
		map_bounds.height = map.transform.localScale.z;
	}

	void move_map(float x, float y) {
		map_vertices = map_mesh.vertices;
		for (int i = 0; i < map_mesh.vertexCount; ++i) {
			map_vertices[i].x += x;
			map_vertices[i].z += y;
		}
		map_mesh.vertices = map_vertices;
		map_bounds.x += map_bounds.width * x;
		map_bounds.y += map_bounds.height * y;
	}

	void Update() {
		if (Input.GetKey(KeyCode.W)) {
			accel.x -= Mathf.Cos(angle * radians) * .01f;
			accel.y -= Mathf.Sin(angle * radians) * .01f;
			accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
			accel.y = Mathf.Clamp(accel.y, -max_speed, max_speed);
		}
		accel *= friction;
		pos.x += accel.x;
		pos.z += accel.y;

		Debug.Log(map_bounds);
		if (accel.x > 0 && pos.x > map_bounds.x) {
			move_map(1, 0);
		}else if (accel.x < 0 && pos.x < map_bounds.x) {
			move_map(-1, 0);
		}else if (accel.y > 0 && pos.z > map_bounds.y) {
			move_map(0, 1);
		}else if (accel.y < 0 && pos.z < map_bounds.y) {
			move_map(0, -1);
		}

		if (Input.GetKey(KeyCode.A)) {
			angle_accel += angle_accel_speed;
		}else if (Input.GetKey(KeyCode.D)) {
			angle_accel -= angle_accel_speed;
		}
		angle_accel = Mathf.Clamp(angle_accel, -max_angle_accel, max_angle_accel);
		angle_accel *= angle_friction;
		angle += angle_accel;
		rota.eulerAngles = rota_euler;
		rota_euler.x -= angle_accel;
		rota_euler.x = Mathf.Clamp(rota_euler.x, 45, 135);
		rota_euler.x -= (rota_euler.x - 90) / 20.0f;
		rota_euler.y -= angle_accel;

		transform.position = pos;
		transform.rotation = rota;

		cam_pos.x -= (cam_pos.x - pos.x) / 20.0f;
		cam_pos.z -= (cam_pos.z - pos.z) / 20.0f;
		Camera.main.transform.position = cam_pos;
	}
}
