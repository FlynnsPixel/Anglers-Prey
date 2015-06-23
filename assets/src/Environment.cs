using System.Collections.Generic;
using UnityEngine;

public class EnvAsset {
	
	public GameObject gobj;
	public float spawn_rate = 0;
	public Vector3 min_scale;
	public Vector3 max_scale;
}

public class EnvObj {

	public GameObject gobj;
	public Light light = null;
}

public class Environment {

	public EnvAsset rock;
	public EnvAsset coral;
	public List<EnvAsset> assets = new List<EnvAsset>();
	public List<EnvObj> spawned_objs = new List<EnvObj>();
	private float total_spawn_rate = 0;

	public void init() {
		load_obj(ref rock, "env/rock", .5f, new Vector3(1, 1, 1), new Vector3(2.75f, 2.75f, 2.75f));
		load_obj(ref coral, "env/coral", .2f, new Vector3(.4f, .4f, .4f), new Vector3(.5f, .5f, .5f));
	}

	private void load_obj(ref EnvAsset obj, string name, float spawn_rate, Vector3 min_scale, Vector3 max_scale) {
		obj = new EnvAsset();
		obj.gobj = (GameObject)Resources.Load(name);
		obj.min_scale = min_scale;
		obj.max_scale = max_scale;
		total_spawn_rate += spawn_rate;
		obj.spawn_rate = total_spawn_rate;
		assets.Add(obj);
	}

	public EnvAsset rand_obj() {
		float rand = Random.Range(0.0f, total_spawn_rate);
		foreach (EnvAsset obj in assets) {
			if (obj.spawn_rate >= rand) return obj;
		}
		return null;
	}

	public void create_obj(EnvAsset obj, Vector3 pos) {
		EnvObj new_obj = new EnvObj();
		new_obj.gobj = GameObject.Instantiate(obj.gobj);

		float m = Random.Range(0.0f, 1.0f);
		new_obj.gobj.transform.localScale = obj.min_scale + ((obj.max_scale - obj.min_scale) * m);

		pos.y -= obj.gobj.GetComponent<MeshFilter>().sharedMesh.bounds.size.y * (new_obj.gobj.transform.localScale.y / obj.gobj.transform.localScale.y);
		new_obj.gobj.transform.position = pos;

		//new_obj.gobj.transform.Rotate(0, Random.Range(0, 360), 0);

		spawned_objs.Add(new_obj);

		if (obj == coral) Light.lights.Add(new_obj.light = Light.create(pos.x, pos.z, 5, 1, .5f, .25f, 1, 1));
	}
}