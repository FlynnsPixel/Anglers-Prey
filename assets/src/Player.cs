using UnityEngine;
using System.Collections;

public class Player {

	public GameObject player;
	public Vector3 pos;
	public Vector2 accel;
	public Quaternion rota;
	public Vector3 rota_euler;
	private float angle = 90;
	private float angle_accel = 0;
	private float cam_angle = 90;

	private bool mouse_touched = false;
	private Vector3 mouse_touch_point;

	private const float max_speed = .15f;
	private const float friction = .92f;
	private const float angle_accel_speed = .1f;
	private const float angle_friction = .95f;
	private const float max_angle_accel = 1;

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

		player_light = Light.create(0, 0, 15, 1.5f, .6f, .7f, 1, 1, Light.LightType.VERTEX);
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
			accel.x -= Mathf.Cos(angle * Math.RADIAN) * Math.RADIAN;
			accel.y -= Mathf.Sin(angle * Math.RADIAN) * Math.RADIAN;
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
		cam_angle -= (cam_angle - angle) / 25.0f;
		Glb.cam.cam_rota.y = -cam_angle - 90;

		rota.eulerAngles = rota_euler;
		rota_euler.x -= angle_accel;
		rota_euler.x = Mathf.Clamp(rota_euler.x, 45, 135);
		rota_euler.x -= (rota_euler.x - 90) / 20.0f;
		rota_euler.y = -angle;

		player.transform.position = pos;
		player.transform.rotation = rota;

		if (Input.GetMouseButtonDown(0)) {
			mouse_touched = true;
			mouse_touch_point.x = Screen.width / 2;
			mouse_touch_point.y = Screen.height / 2;
		}else if (Input.GetMouseButtonUp(0)) {
			mouse_touched = false;
		}
			
		if (mouse_touched) {
			float dist_x = Mathf.Clamp((Input.mousePosition.x - (Screen.width / 2)) / 100.0f, -max_angle_accel, max_angle_accel);
			float dist_y = Mathf.Clamp((Input.mousePosition.y - (Screen.height / 2)) / 200.0f, 0, 1);
			angle_accel = -dist_x;

			accel.x -= (Mathf.Cos(angle * Math.RADIAN) * Math.RADIAN) * dist_y;
			accel.y -= (Mathf.Sin(angle * Math.RADIAN) * Math.RADIAN) * dist_y;
			accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
			accel.y = Mathf.Clamp(accel.y, -max_speed, max_speed);
		}
	}
}