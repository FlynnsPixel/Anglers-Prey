using UnityEngine;
using System.Collections;

public class Player {

	public GameObject player;
	public Vector3 pos;
	public Vector2 accel;
	public Quaternion rota;
	public Vector3 rota_euler;
	private float angle = 90;
	public float angle_accel = 0;
	private float cam_angle = 90;

	private bool mouse_touched = false;
	private Vector3 mouse_touch_point;

	private float max_speed = .15f;
	private float accel_speed = 1;
	private float max_rota = 1;
	private float max_speed_init;
	private float accel_speed_init;
	private float max_rota_init;

	private const float FRICTION = .92f;
	private const float ROTA_ACCEL_SPEED = .1f;
	private const float ROTA_FRICTION = .95f;

	private float angle_offset = 0;
	private float last_angle = 0;

	private Light player_light;

	private bool dashing = false;
	private int dash_timer = 0;
	private int dash_rate = 200;

	public void init() {
		player = GameObject.Find("player");
		pos = player.transform.position;
		pos.y = -1.5f;
		rota = player.transform.rotation;

		max_speed_init = max_speed;
		accel_speed_init = accel_speed;
		max_rota_init = max_rota;

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
			accel.x -= Mathf.Cos(angle * Math.RADIAN) * Math.RADIAN * accel_speed;
			accel.y -= Mathf.Sin(angle * Math.RADIAN) * Math.RADIAN * accel_speed;
			accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
			accel.y = Mathf.Clamp(accel.y, -max_speed, max_speed);
		}
		accel *= FRICTION;
		pos.x += accel.x;
		pos.z += accel.y;

		if (Input.GetKey(KeyCode.A)) {
			angle_accel += ROTA_ACCEL_SPEED;
		}else if (Input.GetKey(KeyCode.D)) {
			angle_accel -= ROTA_ACCEL_SPEED;
		}
		angle_accel = Mathf.Clamp(angle_accel, -max_rota, max_rota);
		angle_accel *= ROTA_FRICTION;
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

		if (Input.GetMouseButton(0)) {
			mouse_touched = true;
			float dist_x = Mathf.Clamp((Input.mousePosition.x - (Screen.width / 2)) / 100.0f, -max_rota, max_rota);
			float dist_y = Mathf.Clamp((Input.mousePosition.y - (Screen.height / 2)) / 200.0f, 0, 1);
			angle_accel = -dist_x;

			accel.x -= (Mathf.Cos(angle * Math.RADIAN) * Math.RADIAN) * dist_y;
			accel.y -= (Mathf.Sin(angle * Math.RADIAN) * Math.RADIAN) * dist_y;
			accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
			accel.y = Mathf.Clamp(accel.y, -max_speed, max_speed);
		}else if (Input.GetMouseButtonUp(0)) {
			mouse_touched = false;
		}

		if (Input.GetKeyDown(KeyCode.Space)) {
			if (!dashing) {
				dashing = true;
				dash_timer = 0;
				max_speed *= 2;
				accel_speed *= 2;
				max_rota *= 5;
			}
		}
		if (dashing) {
			++dash_timer;
			if (dash_timer > dash_rate) {
				dashing = false;
				max_speed = max_speed_init;
				accel_speed = accel_speed_init;
				max_rota = max_rota_init;
			}
		}
	}
}