using UnityEngine;
using System.Collections;

public class Player {

	public GameObject player;
	public Vector3 pos;
	private Quaternion rota;
	private Vector3 rota_euler;
	private Vector2 accel;
	private float angle = 90;
	private float angle_accel = 0;

	private Vector3 cam_pos;
	private Camera cam;

	private bool mouse_touched = false;
	private Vector3 mouse_touch_point;

	private const float radians = Mathf.PI / 180.0f;
	private const float max_speed = .5f;
	private const float friction = .98f;
	private const float angle_accel_speed = .4f;
	private const float angle_friction = .95f;
	private const float max_angle_accel = 4;

	private float angle_offset = 0;
	private float last_angle = 0;

	private Light player_light;

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

		player_light = Light.create(0, 0, 10, 1, .4f, .5f, 1, 1, Light.LightType.VERTEX);
		Light.lights.Add(player_light);

		for (int n = 0; n < 0; ++n) {
			Light.lights.Add(Light.create(Random.Range(-20.0f, 20.0f), Random.Range(-20.0f, 20.0f), 
				Random.Range(5.0f, 15.0f), Random.Range(.2f, .75f), 
				Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f, Light.LightType.VERTEX));
		}
	}

	public void update() {
		player_light.set_pos(player.transform.position.x, player.transform.position.z);

		if (Input.GetKey(KeyCode.W)) {
			accel.x -= Mathf.Cos(angle * radians) * radians;
			accel.y -= Mathf.Sin(angle * radians) * radians;
			accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
			accel.y = Mathf.Clamp(accel.y, -max_speed, max_speed);
		}
		accel *= friction;
		pos.x += accel.x;
		pos.z += accel.y;

		if (accel.x > 0 && pos.x > Glb.map.rect.x) {
			Glb.map.shift_map(1, 0);
		}else if (accel.x < 0 && pos.x < Glb.map.rect.x) {
			Glb.map.shift_map(-1, 0);
		}else if (accel.y > 0 && pos.z > Glb.map.rect.y) {
			Glb.map.shift_map(0, 1);
		}else if (accel.y < 0 && pos.z < Glb.map.rect.y) {
			Glb.map.shift_map(0, -1);
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

		if (Input.GetMouseButtonDown(0)) {
			mouse_touched = true;
			mouse_touch_point = Input.mousePosition;
		}else if (Input.GetMouseButtonUp(0)) {
			mouse_touched = false;
			GameObject cursor_target = GameObject.Find("cursor_target");
			cursor_target.transform.Translate(400, 0, 0);
		}
			
		if (mouse_touched) {
			float a = Mathf.Atan2(mouse_touch_point.y - Input.mousePosition.y, mouse_touch_point.x - Input.mousePosition.x) + (180 * radians);
			float target = a / radians;
			if (target < 170 && last_angle > 190) angle_offset += 360;
			else if (target > 190 && last_angle < 170) angle_offset -= 360;
			last_angle = target;

			angle -= (angle - (target + angle_offset)) / 20.0f;
			angle_accel = -(angle - (target + angle_offset)) / 20.0f;

			float dist = Mathf.Sqrt(Mathf.Pow(mouse_touch_point.y - Input.mousePosition.y, 2) + Mathf.Pow(mouse_touch_point.x - Input.mousePosition.x, 2));
			dist = Mathf.Clamp(dist / 150.0f, 0, 1);
			accel.x -= (Mathf.Cos(angle * radians) * radians) * dist;
			accel.y -= (Mathf.Sin(angle * radians) * radians) * dist;
			accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
			accel.y = Mathf.Clamp(accel.y, -max_speed, max_speed);

			GameObject cursor_target = GameObject.Find("cursor_target");
			mouse_touch_point.z = 10;
			cursor_target.transform.position = cam.ScreenToWorldPoint(mouse_touch_point);
		}
	}
}