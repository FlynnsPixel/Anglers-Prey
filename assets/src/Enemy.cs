using System.Collections.Generic;
using UnityEngine;

public class Enemy {

	public GameObject gobj;
	public Light light = null;
	public bool to_be_removed = false;
	public int ai_type;

	private Vector3 accel;
	private float angle = 0;
	private int angle_timer = 0;
	private const float max_speed = .5f;

	public bool blood_state = false;
	private float blood_size;
	private float blood_intensity;

	public const int AI_DEFENSIVE		= 1;
	public const int AI_AGGRESSIVE		= 1 << 1; 
	public const int AI_BATCH			= 1 << 2;

	public Enemy() {
		angle = Random.Range(0, Mathf.PI * 2);
	}

	public void create_blood_state() {
		blood_state = true;
		blood_size = 2;
		blood_intensity = .85f;
		light.remove();
		Light.lights.Add(light = Light.create(gobj.transform.position.x, gobj.transform.position.z, blood_size, blood_intensity, 1, .25f, .15f, 1));
		GameObject.Destroy(gobj);
		gobj = null;
	}

	public void update() {
		if (blood_state) {
			blood_size -= (blood_size - 22) / 250.0f;
			blood_intensity -= (blood_intensity) / 250.0f;
			light.set_attribs(blood_size, blood_intensity);
			if (blood_intensity <= 0) {
				to_be_removed = true;
			}
			return;
		}

		float dist = Mathf.Sqrt(Mathf.Pow(Glb.player.pos.x - gobj.transform.position.x, 2) + Mathf.Pow(Glb.player.pos.z - gobj.transform.position.z, 2));
		if (dist > Glb.map.width / 1.5f) { to_be_removed = true; return; }
		if (dist < 2) { create_blood_state(); return; }

		if (light != null) light.set_pos(gobj.transform.position.x, gobj.transform.position.z);

		++angle_timer;
		if (angle_timer >= 50) {
			angle_timer = 0;
			angle += Random.Range(-180.0f, 180.0f);
		}

		accel.x -= Mathf.Cos(angle * Math.RADIAN) * Math.RADIAN * .05f;
		accel.z -= Mathf.Sin(angle * Math.RADIAN) * Math.RADIAN * .05f;
		accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
		accel.z = Mathf.Clamp(accel.z, -max_speed, max_speed);
		gobj.transform.position += accel;

		if ((ai_type & AI_DEFENSIVE) == AI_DEFENSIVE) {

		}
		if ((ai_type & AI_AGGRESSIVE) == AI_AGGRESSIVE) {

		}
		if ((ai_type & AI_BATCH) == AI_BATCH) {

		}
	}
}