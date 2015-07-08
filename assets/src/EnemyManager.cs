using System.Collections.Generic;
using UnityEngine;

public class EnemyAsset {
	
	public GameObject gobj = null;
	public GameObject mesh = null;
	public float spawn_rate = 0;
	public float blurred_spawn_rate = 0;
	public Vector3 min_scale;
	public Vector3 max_scale;
	public float energy_gain;
}

public class EnemyManager {

	public EnemyAsset chimaera;
	public EnemyAsset bio_eel;
    public EnemyAsset gulper_eel;
    public EnemyAsset jellyfish;
	public List<EnemyAsset> assets = new List<EnemyAsset>();
	public List<Enemy> enemies = new List<Enemy>();
	private int spawn_timer = 0;
	private const int SPAWN_RATE = 10;
    private int max_enemies = 5;
    private int max_enemy_timer = 0;
    private const int MAX_PREDATORS = 2;
    private int predator_count = 0;
	private float total_spawn_rate = 0;
	private float spawn_radius;
    public int fish_eaten = 0;

	public void init() {
		load_asset(ref chimaera, "enemies/chimaera", .5f, .05f, new Vector3(.35f, .275f, .35f), new Vector3(1.5f, 1.275f, 1.5f), 30);
		load_asset(ref bio_eel, "enemies/bio_eel", .5f, .25f, new Vector3(.5f, .5f, .8f), new Vector3(2.4f, 2.4f, 3.6f), 40);
        load_asset(ref gulper_eel, "enemies/gulper_eel", .35f, .05f, new Vector3(2.0f, 2.0f, 2.0f), new Vector3(2.5f, 2.5f, 2.5f), 35);
        load_asset(ref jellyfish, "enemies/jellyfish", .35f, .05f, new Vector3(1.4f, 1.4f, 1.4f), new Vector3(2.8f, 2.8f, 2.8f), 15);
        spawn_radius = Glb.map.width / 1.5f;
	}

	private void load_asset(ref EnemyAsset obj, string name, float spawn_rate, float blurred_spawn_rate, Vector3 min_scale, Vector3 max_scale, float energy_gain) {
		obj = new EnemyAsset();
		obj.gobj = (GameObject)Resources.Load(name);
		foreach (Transform child in obj.gobj.transform) {
			if (child.name.IndexOf("mesh") != -1) { obj.mesh = child.gameObject; break; }
		}
		if (obj.mesh == null) Debug.Log("no mesh object could be found for enemy (" + name + ")");
		obj.min_scale = min_scale;
		obj.max_scale = max_scale;
		total_spawn_rate += spawn_rate;
		obj.spawn_rate = total_spawn_rate;
		obj.blurred_spawn_rate = blurred_spawn_rate;
		obj.energy_gain = energy_gain;
		assets.Add(obj);
	}

	public EnemyAsset get_rand_asset() {
		float rand = Random.Range(0.0f, total_spawn_rate);
		foreach (EnemyAsset obj in assets) {
			if (obj.spawn_rate >= rand) return obj;
		}
		return null;
	}

    public void create_enemy(EnemyAsset asset, Vector3 pos) {
        if (predator_count >= MAX_PREDATORS) return;

        //create enemy class and object
        Enemy new_enemy = new Enemy();
        new_enemy.gobj = GameObject.Instantiate(asset.gobj);
        new_enemy.asset = asset;
        BoxCollider[] colliders = new_enemy.gobj.GetComponents<BoxCollider>();
        if (colliders.Length >= 1) new_enemy.box_collider_body = colliders[0];
        if (colliders.Length >= 2) new_enemy.box_collider_head = colliders[1];

        //scale enemy
        bool blurred_enemy = Random.value < asset.blurred_spawn_rate;

        float m = Random.Range(0.0f, .2f);
        if (Random.value < .5f) m += Random.Range(.3f, .5f);
        if (blurred_enemy) new_enemy.gobj.transform.localScale = asset.min_scale * Random.Range(10.0f, 12.0f);
        else new_enemy.gobj.transform.localScale = asset.min_scale + ((asset.max_scale - asset.min_scale) * m);

        GameObject mesh_obj = null;
        foreach (Transform child in new_enemy.gobj.transform) {
            if (child.name.IndexOf("mesh") != -1) { mesh_obj = child.gameObject; break; }
        }
        new_enemy.mesh = mesh_obj.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        new_enemy.mesh_obj = mesh_obj;
        Vector3 s = Vector3.Scale(new_enemy.box_collider_body.bounds.size, new_enemy.gobj.transform.localScale);

        if (blurred_enemy) { mesh_obj.layer = 8; new_enemy.blurred_enemy = true; }
        if (asset == gulper_eel) ++predator_count;

        //gen 0-1 value in exactly 2 channels
        Vector3 colour_vec = Vector3.zero;
        float rand = Random.Range(0, 3);

        float c_intensity = 1;
        if (asset == jellyfish) c_intensity = .4f;
        if (rand == 0) colour_vec = new Vector3(0.0f, 0.0f, c_intensity);
        else if (rand == 1) colour_vec = new Vector3(0.0f, c_intensity, 0.0f);
        else if (rand == 2) colour_vec = new Vector3(c_intensity, c_intensity, 0.0f);
        if (!blurred_enemy && (asset == gulper_eel || asset == jellyfish)) { colour_vec = new Vector3(1.0f, 0.0f, 0.0f); new_enemy.predator = true; }

        Color colour = new Color(colour_vec.x, colour_vec.y, colour_vec.z);
        new_enemy.colour = colour;
        new_enemy.set_emission_colour(colour);

        //position enemy
        pos.y -= s.z + 1;
        if (blurred_enemy) pos.y -= 2;
        new_enemy.gobj.transform.position = pos;

        //init and apply light
        new_enemy.init();

        float size = Mathf.Max(s.x, s.z);
        float intensity = .75f;
        if (blurred_enemy) { intensity = .55f; size /= 3.0f; }

        Light.lights.Add(new_enemy.light = Light.create(pos.x, pos.z, size, intensity, colour_vec.x, colour_vec.y, colour_vec.z, 1));

        enemies.Add(new_enemy);
	}

	public void update() {
        ++max_enemy_timer;
        if (max_enemy_timer >= 200) { max_enemy_timer = 0; if (max_enemies < 15) ++max_enemies; }
		++spawn_timer;
		if (spawn_timer >= SPAWN_RATE) {
			spawn_timer = 0;
			if (enemies.Count < max_enemies) {
				float a = Random.Range(0, Mathf.PI * 2);
				Vector3 pos = new Vector3(Glb.map.rect.x + (Mathf.Cos(a) * spawn_radius), 0,
										  Glb.map.rect.y + (Mathf.Sin(a) * spawn_radius));
				create_enemy(get_rand_asset(), pos);
			}
		}

		for (int n = 0; n < enemies.Count; ++n) {
			Enemy enemy = enemies[n];
			enemy.update();
			if (enemy.to_be_removed) {
				if (enemy.light != null && !enemy.light_removed) enemy.light.remove();
				if (enemy.gobj != null) GameObject.Destroy(enemy.gobj);
				enemies.RemoveAt(n);
				--n;
                if (enemy.asset == gulper_eel) --predator_count;
            }
		}

		if (Input.GetKeyDown(KeyCode.E)) {
			for (int n = 0; n < enemies.Count; ++n) {
				Enemy enemy = enemies[n];
				if (enemy.light != null && !enemy.light_removed) enemy.light.remove();
				if (enemy.gobj != null) GameObject.Destroy(enemy.gobj);
				enemies.RemoveAt(n);
                --n;
                if (enemy.asset == gulper_eel) --predator_count;
			}
		}
	}
}