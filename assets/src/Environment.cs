using System.Collections.Generic;
using UnityEngine;

public class EnvObj {
	
	public GameObject gobj;
	public float spawn_rate = 0;
	public Vector3 min_scale;
	public Vector3 max_scale;
}

public class Environment {

	public EnvObj rock;
	public EnvObj coral;
	public List<EnvObj> all_objs = new List<EnvObj>();
	public List<GameObject> spawned_objs = new List<GameObject>();
	private float total_spawn_rate = 0;

	public void init() {
		load_obj(ref rock, "rock", .5f, new Vector3(.5f, .5f, .5f), new Vector3(2, 2, 2));
		load_obj(ref coral, "coral", .2f, new Vector3(.4f, .4f, .4f), new Vector3(.7f, .7f, .7f));
	}

	private void load_obj(ref EnvObj obj, string name, float spawn_rate, Vector3 min_scale, Vector3 max_scale) {
		obj = new EnvObj();
		obj.gobj = (GameObject)Resources.Load(name);
		obj.min_scale = min_scale;
		obj.max_scale = max_scale;
		total_spawn_rate += spawn_rate;
		obj.spawn_rate = total_spawn_rate;
		all_objs.Add(obj);
	}

	public EnvObj rand_obj() {
		float rand = Random.Range(0.0f, total_spawn_rate);
		foreach (EnvObj obj in all_objs) {
			if (obj.spawn_rate >= rand) return obj;
		}
		return null;
	}

	public void create_obj(EnvObj obj, Vector3 pos) {
		GameObject new_obj = GameObject.Instantiate(obj.gobj);
		new_obj.transform.position = pos;
		float m = Random.Range(0.0f, 1.0f);
		new_obj.transform.localScale = new Vector3(obj.min_scale.x + ((obj.max_scale.x - obj.min_scale.x) * m),  
												   obj.min_scale.y + ((obj.max_scale.y - obj.min_scale.y) * m),  
												   obj.min_scale.z + ((obj.max_scale.z - obj.min_scale.z) * m));
		new_obj.transform.Rotate(0, Random.Range(0, 360), 0);
		spawned_objs.Add(new_obj);
	}
}