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

	public const float MAX_ENERGY = 400;
	private float energy = MAX_ENERGY;
	private float light_size = 0;
	private float light_intensity = 0;

	private bool mouse_touched = false;
	private Vector3 mouse_touch_point;

	private float max_speed = .15f;
	private float accel_speed = 1;
	private float max_rota = 2.0f;
	private float max_speed_init;
	private float accel_speed_init;
	private float max_rota_init;

	public const float FRICTION = .92f;
	public const float ROTA_ACCEL_SPEED = .1f;
	public const float ROTA_FRICTION = .95f;

	private float angle_offset = 0;
	private float last_angle = 0;

	public Light light;
	public const float INIT_LIGHT_SIZE = 18;
	public const float INIT_LIGHT_INTENSITY = 1.5f;

	public bool dashing = false;
	private int dash_timer = 0;
	private int dash_length = 50;
	private float dash_angle = 0;

	public void init() {
		player = GameObject.Find("player");
		pos = player.transform.position;
		pos.y = -2.0f;
		rota = player.transform.rotation;

		max_speed_init = max_speed;
		accel_speed_init = accel_speed;
		max_rota_init = max_rota;

		light = Light.create(0, 0, INIT_LIGHT_SIZE, INIT_LIGHT_INTENSITY, .6f, .7f, 1, 1, Light.LightType.VERTEX);
		Light.lights.Add(light);

		for (int n = 0; n < 0; ++n) {
			Light.lights.Add(Light.create(Random.Range(-20.0f, 20.0f), Random.Range(-20.0f, 20.0f), 
				Random.Range(5.0f, 15.0f), Random.Range(.2f, .75f), 
				Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f, Light.LightType.VERTEX));
		}
	}

	public void update() {
		light.set_pos(player.transform.position.x, player.transform.position.z);
		light_size -= (light_size - ((energy / MAX_ENERGY) * INIT_LIGHT_SIZE)) / 20.0f;
		light_intensity -= (light_intensity - ((energy / MAX_ENERGY) * INIT_LIGHT_INTENSITY)) / 20.0f;
		light.set_attribs(light_size, light_intensity);

		//if (Input.GetKey(KeyCode.W)) {
		accel.x -= Mathf.Cos(angle * Math.RADIAN) * Math.RADIAN * accel_speed;
		accel.y -= Mathf.Sin(angle * Math.RADIAN) * Math.RADIAN * accel_speed;
		accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
		accel.y = Mathf.Clamp(accel.y, -max_speed, max_speed);
		accel *= FRICTION;
		pos.x += accel.x;
		pos.z += accel.y;

		if (!dashing) {
			if (Input.GetKey(KeyCode.A)) {
				angle_accel += ROTA_ACCEL_SPEED;
			}else if (Input.GetKey(KeyCode.D)) {
				angle_accel -= ROTA_ACCEL_SPEED;
			}
		}else {
			calc_dash_angle();
			set_energy(energy - .5f);
		}
		angle_accel = Mathf.Clamp(angle_accel, -max_rota, max_rota);
		angle_accel *= ROTA_FRICTION;
		angle += angle_accel;
		if (dashing)
			cam_angle -= (cam_angle - angle) / 10.0f;
		else
			cam_angle -= (cam_angle - angle) / 25.0f;
		Glb.cam.cam_rota.y = -cam_angle - 90;

		rota.eulerAngles = rota_euler;
		rota_euler.x -= angle_accel;
		rota_euler.x = Mathf.Clamp(rota_euler.x, -45, 45);
		rota_euler.x -= (rota_euler.x) / 20.0f;
		rota_euler.y = -angle;
		rota_euler.z = 0;

		player.transform.position = pos;
		player.transform.rotation = rota;

		if (Input.GetMouseButton(0)) {
			mouse_touched = true;
			float dist_x = Mathf.Clamp((Input.mousePosition.x - (Screen.width / 2)) / 100.0f, -max_rota, max_rota);
			float dist_y = Mathf.Clamp((Input.mousePosition.y - (Screen.height / 2)) / 200.0f, 0, 1);
			if (!dashing) angle_accel = -dist_x;

			accel.x -= (Mathf.Cos(angle * Math.RADIAN) * Math.RADIAN) * dist_y;
			accel.y -= (Mathf.Sin(angle * Math.RADIAN) * Math.RADIAN) * dist_y;
			accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
			accel.y = Mathf.Clamp(accel.y, -max_speed, max_speed);
		}else if (Input.GetMouseButtonUp(0)) {
			mouse_touched = false;
		}

		if (Input.GetKeyDown(KeyCode.Space)) {
			if (!dashing) {
				dash();
			}
		}
		if (dashing) {
			++dash_timer;
			if (dash_timer > dash_length) {
				dashing = false;
				max_speed = max_speed_init;
				accel_speed = accel_speed_init;
				max_rota = max_rota_init;
			}
		}
	}

	public void dash() {
		dashing = true;
		dash_timer = 0;
		max_speed *= 2;
		accel_speed *= 2;
		max_rota *= 5;
		calc_dash_angle();
	}

	public void calc_dash_angle() {
		float min_dist = 9999;
		Enemy closest_e = null;
		float closest_e_angle = 0;
		foreach (Enemy e in Glb.em.enemies) {
			if (!e.blurred_enemy && !e.blood_state && !e.to_be_removed && e.player_dist < 5 && e.player_dist < min_dist) { 
				float e_angle = Mathf.Atan2(pos.z - e.gobj.transform.position.z, pos.x - e.gobj.transform.position.x) / Math.RADIAN;
				float a = (angle % 360) - e_angle;
				if (a > -67.0f && a < 67.0f) {
					closest_e = e; min_dist = e.player_dist; closest_e_angle = angle - a;
				}
			}
		}
		if (closest_e != null) {
			angle = closest_e_angle;
			dash_angle = angle;
			angle_accel = 0;
		}
	}

	public void set_energy(float e) {
		energy = Mathf.Clamp(e, 0, MAX_ENERGY);
	}

	public float get_energy() { return energy; }
}