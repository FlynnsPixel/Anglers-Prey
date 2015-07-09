using System.Collections.Generic;
using UnityEngine;

public class Enemy {

	public GameObject gobj = null;
	public EnemyAsset asset = null;
    public Mesh mesh = null;
    public GameObject mesh_obj = null;
    public BoxCollider box_collider_body = null;
    public BoxCollider box_collider_head = null;
    public Light light = null;
	public bool light_removed = false;
	public bool to_be_removed = false;
	public int ai_type;
	public bool blurred_enemy = false;
    public float player_dist = 9999;
    public bool larger_fish = false;
    public float energy = 100;
    public Color colour;
    public bool predator = false;
    public const float MAX_ENERGY = 100;

	private Vector3 accel;
	private float angle_dest = 0;
	private float max_speed = .04f;
    private Vector3 init_pos;
    private float size_radius = 1;

	private float angle = 0;
	private int angle_timer = 0;
	private Vector3 rota_euler;
	private float angle_accel = 0;
	private float angle_offset = 0;
	private float last_angle = 0;
    private Vector3 init_rota;
    private bool always_follow = false;

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
        if (asset == Glb.em.chimaera) max_speed = .04f;
        else if (asset == Glb.em.bio_eel) max_speed = .03f;
        else if (asset == Glb.em.gulper_eel) max_speed = .1f;
        else if (asset == Glb.em.jellyfish) max_speed = .02f;
        else if (asset == Glb.em.hammerhead) max_speed = .14f;
        else if (asset == Glb.em.great_white) max_speed = .09f;

        //check if the created fish is larger than the player
        Vector3 s = Vector3.Scale(mesh.bounds.size, gobj.transform.localScale);
        Vector3 p_s = Vector3.Scale(Glb.player.mesh.bounds.size, Glb.player.player.transform.localScale);
        if ((s.x + s.z) / 2.0f > (p_s.x + p_s.z) / 2.0f) larger_fish = true;

        size_radius = Mathf.Max(s.x, s.z) / 4.0f;

        //ai type
        ai_type = AI_NONE;
        if (predator && asset != Glb.em.jellyfish) ai_type = AI_AGGRESSIVE;
        if (!larger_fish) ai_type = AI_DEFENSIVE;

        //apply changes to speed, animations, ect based on scale
        float sc = Mathf.Max(gobj.transform.localScale.x, gobj.transform.localScale.z) / Mathf.Max(asset.max_scale.x, asset.max_scale.z);
        Animation ani = gobj.GetComponent<Animation>();
        if (blurred_enemy) { ai_type = AI_NONE; ani["swim"].speed = .2f; turn_speed = 200.0f;
        }else {
            max_speed -= Mathf.Clamp(sc / 50.0f, 0, .02f);
            ani["swim"].speed = 1 - (sc / 1.5f);
            turn_speed += (sc * 80.0f);
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

        if (energy < 100) {
            energy += .05f;
            float energy_c = (MAX_ENERGY / 500.0f) - (energy / 500.0f);
            Color c = colour;
            c.r = Mathf.Clamp(c.r - energy_c, .4f, 1.0f);
            c.g = Mathf.Clamp(c.g - energy_c, .4f, 1.0f);
            c.b = Mathf.Clamp(c.b - energy_c, .4f, 1.0f);
            set_emission_colour(c);
            light.set_colour(c);
        }

        player_dist = Mathf.Sqrt(Mathf.Pow(Glb.player.pos.x - gobj.transform.position.x, 2) + Mathf.Pow(Glb.player.pos.z - gobj.transform.position.z, 2));
		if (player_dist > Glb.map.width / 1.25f) { to_be_removed = true; return; }
		if (!blurred_enemy && !Glb.player.spinning && !Glb.player.invincible) {
            if (box_collider_head != null && (box_collider_head.bounds.Intersects(Glb.player.box_collider_head.bounds) || 
                                              box_collider_head.bounds.Intersects(Glb.player.box_collider_body.bounds))) {
                if (larger_fish) {
                    //get hit by enemy
                    Glb.player.get_hit(asset.energy_gain, 0);
                    //push back player
                    Glb.player.accel.x = accel.x * 4.0f;
                    Glb.player.accel.y = accel.y * 4.0f;
                    //push back enemy
                    accel.x = -Mathf.Cos(Glb.player.angle * Math.RADIAN);
                    accel.z = -Mathf.Sin(Glb.player.angle * Math.RADIAN);

                    always_follow = true;
                }else {
                    create_blood_state();
                    return;
                }
            }else if (box_collider_body.bounds.Intersects(Glb.player.box_collider_body.bounds)) {
                if (asset == Glb.em.jellyfish) Glb.player.get_hit(asset.energy_gain, 0.0f);

                //push back player
                Glb.player.accel.x = Mathf.Cos(Glb.player.angle * Math.RADIAN) / (Glb.player.max_speed / (Glb.player.dashing ? 16 : 1));
                Glb.player.accel.y = Mathf.Sin(Glb.player.angle * Math.RADIAN) / (Glb.player.max_speed / (Glb.player.dashing ? 16 : 1));
                //push back enemy
                accel.x = -Mathf.Cos(Glb.player.angle * Math.RADIAN) * 5.0f;
                accel.z = -Mathf.Sin(Glb.player.angle * Math.RADIAN) * 5.0f;
            }else if (box_collider_body.bounds.Intersects(Glb.player.box_collider_head.bounds)) {
                if (larger_fish || asset == Glb.em.jellyfish) {
                    if (asset != Glb.em.jellyfish) {
                        if (Glb.player.dashing) energy -= 90; else energy -= 40;
                        if (energy < 0) {
                            create_blood_state();
                            return;
                        }
                    }else {
                        Glb.player.get_hit(asset.energy_gain, .4f);
                    }
                    //push back player
                    Glb.player.accel.x = Mathf.Cos(Glb.player.angle * Math.RADIAN) / (Glb.player.max_speed / (Glb.player.dashing ? 16 : 1));
                    Glb.player.accel.y = Mathf.Sin(Glb.player.angle * Math.RADIAN) / (Glb.player.max_speed / (Glb.player.dashing ? 16 : 1));
                    //push back enemy
                    accel.x = -Mathf.Cos(Glb.player.angle * Math.RADIAN) * 5.0f;
                    accel.z = -Mathf.Sin(Glb.player.angle * Math.RADIAN) * 5.0f;

                    if (predator && asset != Glb.em.jellyfish) always_follow = true; else ai_type = (ai_type ^ AI_AGGRESSIVE) | AI_DEFENSIVE;
                }else {
                    create_blood_state();
                    return;
                }
            }
		}

		if (light != null) light.set_pos(gobj.transform.position.x, gobj.transform.position.z);

		if ((Glb.player.light_size > Player.INIT_LIGHT_SIZE / 2.0f || !larger_fish) && (ai_type & AI_DEFENSIVE) == AI_DEFENSIVE) {
            if (player_dist <= Glb.player.light_size * .9f) {
                angle_dest = Mathf.Atan2(Glb.player.pos.z - gobj.transform.position.z, Glb.player.pos.x - gobj.transform.position.x);
            }
        }
		if (Glb.player.light_size <= Player.INIT_LIGHT_SIZE / 2.0f || (ai_type & AI_AGGRESSIVE) == AI_AGGRESSIVE) {
            if (Glb.player.light_size <= Player.INIT_LIGHT_SIZE / 2.0f || always_follow || player_dist <= Glb.player.light_size * 1.5f) {
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

        if (asset != Glb.em.jellyfish) {
            rota_euler.x -= angle_accel;
            rota_euler.x -= rota_euler.x / 80.0f;
            rota_euler.x = Mathf.Clamp(rota_euler.x, init_rota.x - 35.0f, init_rota.x + 35.0f);
        }
        rota_euler.y = -angle + 180.0f;

		gobj.transform.localEulerAngles = rota_euler + init_rota;

		accel.x -= Mathf.Cos(angle * Math.RADIAN) * Math.RADIAN;
		accel.z -= Mathf.Sin(angle * Math.RADIAN) * Math.RADIAN;
		accel *= .9f;
		accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
		accel.z = Mathf.Clamp(accel.z, -max_speed, max_speed);
        gobj.transform.position += accel;
	}

    public void set_emission_colour(Color c) {
        Material mat;
        if (asset == Glb.em.gulper_eel) mat = mesh_obj.GetComponent<Renderer>().materials[3];
        else mat = mesh_obj.GetComponent<Renderer>().material;
        mat.SetColor("_EmissionColor", c);
    }
}