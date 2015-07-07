using System.Collections.Generic;
using UnityEngine;

public class Enemy {

	public GameObject gobj = null;
	public EnemyAsset asset = null;
	public Mesh mesh = null;
	public Light light = null;
	public bool light_removed = false;
	public bool to_be_removed = false;
	public int ai_type;
	public bool blurred_enemy = false;
    public float player_dist = 9999;
    public bool larger_fish = false;

	private Vector3 accel;
	private float angle_dest = 0;
	private float max_speed = .04f;
	private Vector3 init_pos;

	private float angle = 0;
	private int angle_timer = 0;
	private Vector3 rota_euler;
	private float angle_accel = 0;
	private float angle_offset = 0;
	private float last_angle = 0;
	private Vector3 init_rota;

	private const float max_angle_accel = 4;
    private const float angle_friction = .8f;
    private float turn_speed = 20.0f;

	public bool blood_state = false;
	private Light blood_light;
	private float blood_size;
    private float blood_intensity;

    public const int AI_NONE            = 0;
	public const int AI_DEFENSIVE		= 1;
	public const int AI_AGGRESSIVE		= 1 << 1; 
	public const int AI_BATCH			= 1 << 2;

	public void init() {
        //initial values
		angle_dest = Random.Range(0, Mathf.PI * 2);
		init_rota = gobj.transform.localEulerAngles;
        init_pos = gobj.transform.position;

        //initial speeds for enemies
        if (asset == Glb.em.chimaera) max_speed = .035f;
        else if (asset == Glb.em.bio_eel) max_speed = .03f;
        else if (asset == Glb.em.gulper_eel) max_speed = .05f;

        //check if the created fish is larger than the player
        Vector3 s = Vector3.Scale(mesh.bounds.size, gobj.transform.localScale);
        Vector3 p_s = Vector3.Scale(Glb.player.mesh.bounds.size, Glb.player.player.transform.localScale);
        if (s.x > p_s.x || s.z > p_s.z) larger_fish = true;

        //ai type
        int type = Random.Range(1, 3);
        if (larger_fish) type = 2;
        if (Random.Range(0.0f, 1.0f) >= .5f) type = type | Enemy.AI_BATCH;
        ai_type = type;

        //apply changes to speed, animations, ect based on scale
        float sc = Mathf.Max(gobj.transform.localScale.x, gobj.transform.localScale.z) / Mathf.Max(asset.max_scale.x, asset.max_scale.z);
        Animation ani = gobj.GetComponent<Animation>();
        if (blurred_enemy) { ai_type = AI_NONE; ani["swim"].speed = .2f; turn_speed = 55.0f;
        }else {
            max_speed -= Mathf.Clamp(sc / 50.0f, 0, .02f);
            ani["swim"].speed = 1 - (sc / 1.5f);
            turn_speed += (sc * 50.0f);
        }
    }

    public void create_blood_state() {
		blood_state = true;
		blood_size = 2;
		blood_intensity = .85f;
		Light.lights.Add(blood_light = Light.create(gobj.transform.position.x, gobj.transform.position.z, blood_size, blood_intensity, 1, .25f, .15f, 1));
		GameObject.Destroy(gobj);
		gobj = null;
		light_removed = false;

		Glb.player.set_energy(Glb.player.get_energy() + asset.energy_gain);
		++Glb.em.fish_eaten;
		Glb.gui.scale_rstats();
	}

	public void update() {
		if (blood_state) {
			blood_size -= (blood_size - 22) / 250.0f;
			blood_intensity -= (blood_intensity) / 250.0f;
			blood_light.set_attribs(blood_size, blood_intensity);
			light.set_attribs(light.get_size(), light.get_intensity() - .1f);
			if (!light_removed && light.get_intensity() <= 0) {
				light_removed = true;
				light.remove();
			}
			if (blood_intensity <= 0) {
				to_be_removed = true;
				blood_light.remove();
			}
			return;
		}
		
		player_dist = Mathf.Sqrt(Mathf.Pow(Glb.player.pos.x - gobj.transform.position.x, 2) + Mathf.Pow(Glb.player.pos.z - gobj.transform.position.z, 2));
		if (player_dist > Glb.map.width / 1.5f) { to_be_removed = true; return; }
		if (!blurred_enemy && player_dist < 3) {
            float e_angle = -(Mathf.Atan2(Glb.player.pos.z - gobj.transform.position.z, Glb.player.pos.x - gobj.transform.position.x) / Math.RADIAN) + 180;
			float a = Glb.player.angle - e_angle;
			if (a > -45 && a < 45) {
				create_blood_state();
				return;
			}else {
				Glb.player.accel = -Glb.player.accel;
			}
		}

		if (light != null) light.set_pos(gobj.transform.position.x, gobj.transform.position.z);

		if ((ai_type & AI_DEFENSIVE) == AI_DEFENSIVE) {
            if (player_dist <= Glb.player.light_size * .9f) {
                angle_dest = Mathf.Atan2(Glb.player.pos.z - gobj.transform.position.z, Glb.player.pos.x - gobj.transform.position.x);
            }
        }
		if ((ai_type & AI_AGGRESSIVE) == AI_AGGRESSIVE) {
            if (player_dist <= Glb.player.light_size * .9f) {
                angle_dest = Mathf.Atan2(Glb.player.pos.z - gobj.transform.position.z, Glb.player.pos.x - gobj.transform.position.x) + Mathf.PI;
            }
		}
		if ((ai_type & AI_BATCH) == AI_BATCH) {

		}

		++angle_timer;
		if (angle_timer >= 20) {
			angle_timer = 0;
			angle_dest += Random.Range(-Mathf.PI / 8.0f, Mathf.PI / 8.0f);
		}

        angle = Math.smooth_angle(angle, angle_dest / Math.RADIAN, turn_speed);
        angle_accel = Math.smooth_angle(angle, angle_dest / Math.RADIAN, turn_speed / 3);
		angle_accel = Mathf.Clamp(angle_accel, -max_angle_accel, max_angle_accel);
		angle_accel *= angle_friction;

		rota_euler.x -= angle_accel * 4;
		rota_euler.x -= rota_euler.x / 80.0f;
	    rota_euler.x = Mathf.Clamp(rota_euler.x, -35.0f, 35.0f);
        rota_euler.y = -angle + 180.0f;

		gobj.transform.localEulerAngles = rota_euler + init_rota;

		accel.x -= Mathf.Cos(angle * Math.RADIAN) * Math.RADIAN;
		accel.z -= Mathf.Sin(angle * Math.RADIAN) * Math.RADIAN;
		accel *= .9f;
		accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
		accel.z = Mathf.Clamp(accel.z, -max_speed, max_speed);
        gobj.transform.position += accel;
	}
}