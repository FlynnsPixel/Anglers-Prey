﻿using UnityEngine;
using System.Collections;

public class Player {

    public GameObject player;
    public SkinnedMeshRenderer mesh;
    public BoxCollider box_collider_body;
    public BoxCollider box_collider_head;
	public Vector3 pos;
	public Vector2 accel;
	public Quaternion rota;
	public Vector3 rota_euler;
	public float angle = 90;
	public float angle_accel = 0;
	public float cam_angle = 90;

	public const float MAX_ENERGY = 400;
	private float energy = MAX_ENERGY;
	public float light_size = 0;
	public float light_intensity = 0;

	private bool mouse_touched = false;
	private Vector3 mouse_touch_point;

	public float max_speed = 4;
	private float accel_speed = .1f;
	private float max_rota = 2.0f;
	private float max_speed_init;
	private float accel_speed_init;
    private float max_rota_init;
    public float spin_angle_accel = 0;
    public bool spinning = false;

	public const float FRICTION = .98f;
	public const float ROTA_ACCEL_SPEED = .2f;
    public const float ROTA_FRICTION = .95f;
    public const float SPIN_ROTA_FRICTION = .88f;

	private float angle_offset = 0;
	private float last_angle = 0;

	public Light light;
	public const float INIT_LIGHT_SIZE = 18;
	public const float INIT_LIGHT_INTENSITY = 1.5f;
	public bool light_off = false;

	public bool dashing = false;
	private int dash_timer = 0;
	private int dash_length = 50;
    private float dash_angle = 0;
    public int invincible_timer = 0;
    public const int INVINCIBLE_TIME = 120;
    public bool invincible = false;

    public AudioClip[] bloopies = new AudioClip[5];

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

		Animation ani = null;
		foreach (Transform child in player.transform) {
            if (child.name.IndexOf("ani") != -1) { ani = child.gameObject.GetComponent<Animation>(); }
            if (child.name.IndexOf("mesh") != -1) { mesh = child.gameObject.GetComponent<SkinnedMeshRenderer>(); }
            foreach (Transform c in child.transform) {
                if (c.name.IndexOf("ani") != -1) { ani = c.gameObject.GetComponent<Animation>(); }
                if (c.name.IndexOf("mesh") != -1) { mesh = c.gameObject.GetComponent<SkinnedMeshRenderer>(); }
            }
        }
        ani["swim"].speed = 5;
        box_collider_body = player.GetComponents<BoxCollider>()[0];
        box_collider_head = player.GetComponents<BoxCollider>()[1];

        bloopies[0] = (AudioClip)Resources.Load("Bloop1");
        bloopies[1] = (AudioClip)Resources.Load("Bloop2");
        bloopies[2] = (AudioClip)Resources.Load("Bloop3");
        bloopies[3] = (AudioClip)Resources.Load("Bloop4");
        bloopies[4] = (AudioClip)Resources.Load("Bloop5");
    }

	public void update() {
		light.set_pos(player.transform.position.x, player.transform.position.z);
		if (light_off) {
			light_size -= (light_size - 5) / 2.0f;
			light_intensity -= (light_intensity - .1f) / 2.0f;
		}else {
			light_size -= (light_size - ((energy / MAX_ENERGY) * INIT_LIGHT_SIZE)) / 20.0f;
			light_intensity -= (light_intensity - ((energy / MAX_ENERGY) * INIT_LIGHT_INTENSITY)) / 20.0f;
		}
        light_size = Mathf.Max(light_size, 8);
        light_intensity = Mathf.Max(light_intensity, .2f);
		light.set_attribs(light_size, light_intensity);

		//if (Input.GetKey(KeyCode.W)) {
		accel.x -= Mathf.Cos(angle * Math.RADIAN) * Math.RADIAN * accel_speed;
		accel.y -= Mathf.Sin(angle * Math.RADIAN) * Math.RADIAN * accel_speed;
		accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
		accel.y = Mathf.Clamp(accel.y, -max_speed, max_speed);
		accel *= FRICTION;
		pos.x += accel.x;
		pos.z += accel.y;

		if (Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(2)) light_off = !light_off;

		if (!dashing) {
			if (Input.GetKey(KeyCode.A)) {
				angle_accel += ROTA_ACCEL_SPEED;
			}else if (Input.GetKey(KeyCode.D)) {
				angle_accel -= ROTA_ACCEL_SPEED;
			}
		}else {
			calc_dash_angle();
			set_energy(energy - .4f);
		}
        angle_accel = Mathf.Clamp(angle_accel, -max_rota, max_rota);
        angle_accel += spin_angle_accel;
        spin_angle_accel *= SPIN_ROTA_FRICTION;
        spinning = spin_angle_accel > .01f;
        angle_accel *= ROTA_FRICTION;
		angle += angle_accel;
        if (dashing)
            cam_angle = Math.smooth_angle(cam_angle, angle, 10.0f);
        else
            cam_angle = Math.smooth_angle(cam_angle, angle, 25.0f);
        Glb.cam.cam_rota.y = -cam_angle - 90;

		rota.eulerAngles = rota_euler;
		rota_euler.x -= angle_accel;
		rota_euler.x = Mathf.Clamp(rota_euler.x, -45, 45);
        rota_euler.x -= (rota_euler.x) / 20.0f;
        //wrap angle from 0-360
        angle = Math.smooth_angle(angle, angle, 1.0f);
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

		if (spin_angle_accel <= .1f && Input.GetKeyDown(KeyCode.Space)) {
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
        if (invincible) {
            ++invincible_timer;
            float i = (Mathf.Cos(Time.timeSinceLevelLoad * 10.0f) / 2.0f) + .5f;
            if (invincible_timer > INVINCIBLE_TIME) {
                invincible_timer = 0;
                invincible = false;
                i = .5f;
            }
            Color c = new Color(i, i, i);
            mesh.material.SetColor("_Color", c);
            mesh.material.SetColor("_EmissionColor", c);
        }
    }

	public void dash() {
        int which_bloopie = Random.Range(0, 4);
        AudioSource audio = Glb.cam.main.GetComponent<AudioSource>();
        audio.PlayOneShot(bloopies[which_bloopie]);
        dashing = true;
        dash_timer = 0;
		max_speed *= 8;
		accel_speed *= 8;
		max_rota *= 5;
		calc_dash_angle();
	}

	public void calc_dash_angle() {
		float min_dist = 9999;
		Enemy closest_e = null;
		float closest_e_angle = 0;
		foreach (Enemy e in Glb.em.enemies) {
			if (!e.larger_fish && !e.blurred_enemy && !e.blood_state && !e.to_be_removed && e.player_dist < 8 && e.player_dist < min_dist) { 
				float e_angle = Mathf.Atan2(pos.z - e.gobj.transform.position.z, pos.x - e.gobj.transform.position.x) / Math.RADIAN;
				float a = (angle % 360) - e_angle;
				if (a > -90 && a < 90) {
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

    public void get_hit(float energy_loss, float spin_angle) {
        if (!invincible) {
            set_energy(get_energy() - energy_loss);
            spin_angle_accel += spin_angle;
            invincible = true;
            invincible_timer = 0;
        }
    }
}