using UnityEngine;
using System.Collections;

public class Player {

	GameObject player;
	public Vector3 pos;
	Quaternion rota;
	Vector3 rota_euler;
	Vector2 accel;
	float angle = 90;
	float angle_accel = 0;

	Vector3 cam_pos;
	Camera cam;

	bool mouse_touched = false;
	Vector3 last_mouse_pos;

	const float radians = Mathf.PI / 180.0f;
	const float max_speed = .175f;
	const float friction = .98f;
	const float angle_accel_speed = .15f;
	const float angle_friction = .95f;
	const float max_angle_accel = 4;

	float angle_offset = 0;
	float last_angle = 0;

	Light player_light;

	public void init() {
		player = GameObject.Find("player");
		pos = player.transform.position;
		pos.y = -1.5f;
		rota = player.transform.rotation;
		rota_euler.x = 0;
		rota_euler.y = 0;
		rota_euler.z = 0;
		cam_pos = Camera.main.transform.position;
		cam = Camera.main;

		//player_light = Light.create(50, 0, 2, 2.5f, .4f, .5f, 1, 1);
		//Light.lights.Add(player_light);
		//Light.lights.Add(Light.create(-2.5f, -17, .75f, 1.5f, .5f, 0, .75f, 1));

		for (int n = 0; n < 0; ++n) {
			Light.lights.Add(Light.create(Random.Range(-25.0f, 25.0f), Random.Range(-25.0f, 25.0f), 
				Random.Range(.5f, 4), Random.Range(.2f, 2.0f), 
				Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f));
		}
	}

	public void update() {
		for (int i = 0; i < Light.lights.Count; ++i) {
			Light.lights[i].set_pos(Light.lights[i].get_pos().x, Light.lights[i].get_pos().z);
		}
		//player_light.set_pos(player.transform.position.x, player.transform.position.z);

		if (Input.GetMouseButtonDown(0)) {
			mouse_touched = true;
			last_mouse_pos = Input.mousePosition;
		}else if (Input.GetMouseButtonUp(0)) {
			mouse_touched = false;
		}
			
		if (mouse_touched) {
			Vector3 b = cam.WorldToScreenPoint(cam_pos + (player.transform.position - cam_pos));
			float c_x = (Screen.width / 2) + player.transform.position.x, c_y = (Screen.height / 2) + player.transform.position.z;
			float a = Mathf.Atan2(b.y - Input.mousePosition.y, b.x - Input.mousePosition.x) + (180 * radians);
			float target = a / radians;
			if (target < 170 && last_angle > 190) angle_offset += 360;
			else if (target > 190 && last_angle < 170) angle_offset -= 360;
			last_angle = target;

			angle -= (angle - (target + angle_offset)) / 20.0f;
			angle_accel = -(angle - (target + angle_offset)) / 20.0f;

			accel.x -= Mathf.Cos(angle * radians) * .01f;
			accel.y -= Mathf.Sin(angle * radians) * .01f;
			accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
			accel.y = Mathf.Clamp(accel.y, -max_speed, max_speed);
		}

		if (Input.GetKey(KeyCode.W)) {
			accel.x -= Mathf.Cos(angle * radians) * .01f;
			accel.y -= Mathf.Sin(angle * radians) * .01f;
			accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
			accel.y = Mathf.Clamp(accel.y, -max_speed, max_speed);
		}
		accel *= friction;
		pos.x += accel.x;
		pos.z += accel.y;

		if (accel.x > 0 && pos.x > Glb.map.rect.x) {
			//Glb.map.scroll_vertices(1, 0);
		}else if (accel.x < 0 && pos.x < Glb.map.rect.x) {
			//Glb.map.scroll_vertices(-1, 0);
		}else if (accel.y > 0 && pos.z > Glb.map.rect.y) {
			//Glb.map.scroll_vertices(0, 1);
		}else if (accel.y < 0 && pos.z < Glb.map.rect.y) {
			//Glb.map.scroll_vertices(0, -1);
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
		rota_euler.y = -angle;

		player.transform.position = pos;
		player.transform.rotation = rota;

		cam_pos.x -= (cam_pos.x - pos.x) / 20.0f;
		cam_pos.z -= (cam_pos.z - pos.z) / 20.0f;
		cam.transform.position = cam_pos;
	}
}