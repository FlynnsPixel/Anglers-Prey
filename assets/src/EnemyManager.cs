using System.Collections.Generic;
using UnityEngine;

public class EnemyAsset {
	
	public GameObject gobj;
	public float spawn_rate = 0;
	public Vector3 min_scale;
	public Vector3 max_scale;
}

public class EnemyManager {

	public EnemyAsset fish;
	public List<EnemyAsset> assets = new List<EnemyAsset>();
	public List<Enemy> enemies = new List<Enemy>();
	private int spawn_timer = 0;
	private const int SPAWN_RATE = 40;
	private const int MAX_ENEMIES = 8;
	private float total_spawn_rate = 0;
	private float spawn_radius;

	public void init() {
		load_asset(ref fish, "enemies/fish", .5f, new Vector3(1, 1, 1), new Vector3(2.75f, 2.75f, 2.75f));
		spawn_radius = Glb.map.width / 2;
	}

	private void load_asset(ref EnemyAsset obj, string name, float spawn_rate, Vector3 min_scale, Vector3 max_scale) {
		obj = new EnemyAsset();
		obj.gobj = (GameObject)Resources.Load(name);
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

		pos.y -= asset.gobj.GetComponent<MeshFilter>().sharedMesh.bounds.size.y * (new_enemy.gobj.transform.localScale.y / asset.gobj.transform.localScale.y);
		new_enemy.gobj.transform.position = pos;

		//new_enemy.gobj.transform.Rotate(0, Random.Range(0, 360), 0);

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
				GameObject.Destroy(enemy.gobj);
				enemies.RemoveAt(n);
				--n;
			}
		}
	}
}