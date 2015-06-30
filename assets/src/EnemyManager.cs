using System.Collections.Generic;
using UnityEngine;

public class EnemyAsset {
	
	public GameObject gobj = null;
	public GameObject mesh = null;
	public float spawn_rate = 0;
	public float blurred_spawn_rate = 0;
	public Vector3 min_scale;
	public Vector3 max_scale;
}

public class EnemyManager {

	public EnemyAsset chimaera;
	public EnemyAsset bio_eel;
	public EnemyAsset gulper_eel;
	public List<EnemyAsset> assets = new List<EnemyAsset>();
	public List<Enemy> enemies = new List<Enemy>();
	private int spawn_timer = 0;
	private const int SPAWN_RATE = 10;
	private const int MAX_ENEMIES = 20;
	private float total_spawn_rate = 0;
	private float spawn_radius;

	public void init() {
		load_asset(ref chimaera, "enemies/chimaera", .5f, .05f, new Vector3(.35f, .275f, .35f), new Vector3(.5f, .425f, .5f));
		load_asset(ref bio_eel, "enemies/bio_eel", .5f, .25f, new Vector3(.5f, .5f, .8f), new Vector3(.8f, .8f, 1.2f));
		load_asset(ref gulper_eel, "enemies/gulper_eel", .5f, .05f, new Vector3(.7f, .7f, .7f), new Vector3(1, 1, 1));
		spawn_radius = Glb.map.width / 2;
	}

	private void load_asset(ref EnemyAsset obj, string name, float spawn_rate, float blurred_spawn_rate, Vector3 min_scale, Vector3 max_scale) {
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
		//create enemy class and object
		Enemy new_enemy = new Enemy();
		new_enemy.gobj = GameObject.Instantiate(asset.gobj);

		//ai type
		int type = Random.Range(1, 2);
		if (Random.Range(0.0f, 1.0f) >= .5f) type = type | Enemy.AI_BATCH;
		new_enemy.ai_type = type;

		//scale enemy
		bool blurred_enemy = Random.value < asset.blurred_spawn_rate;

		float m = Random.Range(0.0f, 1.0f);
		if (blurred_enemy) m = Random.Range(15.0f, 25.0f);
		new_enemy.gobj.transform.localScale = asset.min_scale + ((asset.max_scale - asset.min_scale) * m);

		GameObject mesh_obj = null;
		foreach (Transform child in new_enemy.gobj.transform) {
			if (child.name.IndexOf("mesh") != -1) { mesh_obj = child.gameObject; break; }
		}
		new_enemy.mesh = mesh_obj.GetComponent<SkinnedMeshRenderer>().sharedMesh;
		Vector3 s = Vector3.Scale(new_enemy.mesh.bounds.size, new_enemy.gobj.transform.localScale);

		if (blurred_enemy) { mesh_obj.layer = 8; new_enemy.blurred_enemy = true; }

		//gen 0-1 value in exactly 2 channels
		Vector3 colour_vec = Vector3.zero;
		float rand = Random.Range(0, 3);

		if (rand == 0) colour_vec.x = Random.Range(0.0f, 1.0f); 
		else if (rand == 1) colour_vec.y = Random.Range(0.0f, 1.0f); 
		else if (rand == 2) colour_vec.z = Random.Range(0.0f, 1.0f);

		rand += Random.Range(1, 3);
		rand = rand % 3;

		if (rand == 0) colour_vec.x = Random.Range(0.0f, 1.0f); 
		else if (rand == 1) colour_vec.y = Random.Range(0.0f, 1.0f); 
		else if (rand == 2) colour_vec.z = Random.Range(0.0f, 1.0f);

		Color colour = new Color(colour_vec.x * 255.0f, colour_vec.y * 255.0f, colour_vec.z * 255.0f);
		mesh_obj.GetComponent<Renderer>().material.SetColor("_EmissionColor", colour);

		//position enemy
		pos.y -= s.z;
		if (blurred_enemy) pos.y -= 2;
		new_enemy.gobj.transform.position = pos;

		//init and apply light
		new_enemy.init();

		float size = Mathf.Max(s.x, s.y);
		float intensity = .75f;
		if (blurred_enemy) { intensity = .75f; size = size / 2.0f; }

		if (asset == chimaera)
			Light.lights.Add(new_enemy.light = Light.create(pos.x, pos.z, size, intensity, colour_vec.x, colour_vec.y, colour_vec.z, 1));
		else if (asset == bio_eel)
			Light.lights.Add(new_enemy.light = Light.create(pos.x, pos.z, size, intensity, colour_vec.x, colour_vec.y, colour_vec.z, 1));
		else if (asset == gulper_eel)
			Light.lights.Add(new_enemy.light = Light.create(pos.x, pos.z, size, intensity, colour_vec.x, colour_vec.y, colour_vec.z, 1));

		enemies.Add(new_enemy);
	}

	public void update() {
		++spawn_timer;
		if (spawn_timer >= SPAWN_RATE) {
			spawn_timer = 0;
			if (enemies.Count < MAX_ENEMIES) {
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
			}
		}

		if (Input.GetKeyDown(KeyCode.E)) {
			for (int n = 0; n < enemies.Count; ++n) {
				Enemy enemy = enemies[n];
				if (enemy.light != null && !enemy.light_removed) enemy.light.remove();
				if (enemy.gobj != null) GameObject.Destroy(enemy.gobj);
				enemies.RemoveAt(n);
				--n;
			}
		}
	}
}