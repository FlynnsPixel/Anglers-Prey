using System.Collections.Generic;
using UnityEngine;

public class EnemyAsset {
	
	public GameObject gobj = null;
	public GameObject mesh = null;
	public float spawn_rate = 0;
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
		load_asset(ref chimaera, "enemies/chimaera", .5f, new Vector3(.35f, .275f, .35f), new Vector3(.5f, .425f, .5f));
		load_asset(ref bio_eel, "enemies/bio_eel", .5f, new Vector3(.5f, .5f, .8f), new Vector3(.8f, .8f, 1.2f));
		load_asset(ref gulper_eel, "enemies/gulper_eel", .5f, new Vector3(.7f, .7f, .7f), new Vector3(1, 1, 1));
		spawn_radius = Glb.map.width / 2;
	}

	private void load_asset(ref EnemyAsset obj, string name, float spawn_rate, Vector3 min_scale, Vector3 max_scale) {
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
		Enemy new_enemy = new Enemy();
		new_enemy.gobj = GameObject.Instantiate(asset.gobj);

		int type = Random.Range(1, 2);
		if (Random.Range(0.0f, 1.0f) >= .5f) type = type | Enemy.AI_BATCH;
		new_enemy.ai_type = type;

		float m = Random.Range(0.0f, 1.0f);
		new_enemy.gobj.transform.localScale = asset.min_scale + ((asset.max_scale - asset.min_scale) * m);

		GameObject mesh_obj = null;
		foreach (Transform child in new_enemy.gobj.transform) {
			if (child.name.IndexOf("mesh") != -1) { mesh_obj = child.gameObject; break; }
		}
		new_enemy.mesh = mesh_obj.GetComponent<SkinnedMeshRenderer>().sharedMesh;
		new_enemy.mesh.RecalculateBounds();
		Bounds bounds = new_enemy.mesh.bounds;

		if (asset == gulper_eel) Debug.Log(bounds.size.y);
		pos.y -= bounds.size.y + 1;
		new_enemy.gobj.transform.position = pos;

		new_enemy.init();

		if (asset == chimaera)
			Light.lights.Add(new_enemy.light = Light.create(pos.x, pos.z, 6, .75f, .1f, 1, .5f, 1));
		else if (asset == bio_eel)
			Light.lights.Add(new_enemy.light = Light.create(pos.x, pos.z, 6, .75f, 1, .1f, .5f, 1));
		else if (asset == gulper_eel)
			Light.lights.Add(new_enemy.light = Light.create(pos.x, pos.z, 6, .75f, .2f, .75f, .7f, 1));

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