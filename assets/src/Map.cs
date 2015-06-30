using System.Collections.Generic;
using UnityEngine;

public class Map {

	public float width;
	public float height;
	public float half_width;
	public float half_height;
	public const float MAX_VERTEX_WIDTH = 11;
	public const float MAX_VERTEX_HEIGHT = 11;
	public int vertices_per_row;

	public GameObject map;
	public Mesh mesh;
	public Material material;
	public Vector3[] vertices;
	public Rect rect;
	public Vector3 offset;					//offset for when the map is shifted

	public void init() {
		map = GameObject.Find("map");
		mesh = map.GetComponent<MeshFilter>().mesh;
		rect.x = map.transform.position.x;
		rect.y = map.transform.position.z;
		rect.width = map.transform.localScale.x;
		rect.height = map.transform.localScale.z;

		material = map.GetComponent<Renderer>().material;

		width = mesh.bounds.size.x * rect.width;
		height = mesh.bounds.size.z * rect.height;
		half_width = width / 2.0f;
		half_height = height / 2.0f;

		resize_vertices(55);
	}

	public void spawn_init_env() {
		int num_env_objs = Random.Range(15, 40);
		for (int n = 0; n < num_env_objs; ++n) {
			Vector3 pos = new Vector3(Random.Range(-half_width, half_width), 0, Random.Range(-half_height, half_height));
			Glb.env.create_obj(Glb.env.get_rand_asset(), pos);
		}
	}

	public void resize_vertices(int row_size) {
		vertices_per_row = row_size;

		vertices = new Vector3[row_size * row_size];
		for (int y = 0; y < row_size; ++y) {
			for (int x = 0; x < row_size; ++x) {
				int index = (y * row_size) + x;
				vertices[index].x = ((float)x - (row_size / 2.0f)) / (row_size / 11.0f);
				vertices[index].z = ((float)y - (row_size / 2.0f)) / (row_size / 11.0f);
			}
		}

		mesh.vertices = vertices;
		Color[] colours = new Color[row_size * row_size];
		mesh.colors = colours;

		for (int y = 0; y < row_size; ++y) {
			for (int x = 0; x < row_size; ++x) {
				colours[(y * row_size) + x] = Color.black;
			}
		}

		int num_indices = (((row_size - 1) * (row_size - 1)) + (row_size - 2)) * 6;
		int[] indices = new int[num_indices];
		string temp = "";
		for (int i = 0; i < num_indices; i += 6) {
			int index = i / 6;
			indices[i] = index;
			indices[i + 1] = index + row_size;
			indices[i + 2] = index + row_size + 1;
			indices[i + 3] = index;
			indices[i + 4] = index + row_size + 1;
			indices[i + 5] = index + 1;
		}
		mesh.vertices = vertices;
		mesh.SetIndices(indices, MeshTopology.Triangles, 0);

		mesh.RecalculateBounds();
	}

	public void update() {
		if (Glb.player.accel.x > 0 && Glb.player.pos.x > rect.x) {
			shift_map(1, 0);
		}else if (Glb.player.accel.x < 0 && Glb.player.pos.x < rect.x) {
			shift_map(-1, 0);
		}else if (Glb.player.accel.y > 0 && Glb.player.pos.z > rect.y) {
			shift_map(0, 1);
		}else if (Glb.player.accel.y < 0 && Glb.player.pos.z < rect.y) {
			shift_map(0, -1);
		}
	}

	public void shift_map(float x, float y) {
		//loop through all vertices in mesh and move by specified x and y
		//this can be uncommented if need be, but for now moving the map's position works fine
		vertices = mesh.vertices;
		for (int i = 0; i < mesh.vertexCount; ++i) {
			vertices[i].x += x;
			vertices[i].z += y;
		}
		mesh.vertices = vertices;

		//move map rect x, y by the width/height scale
		rect.x += rect.width * x;
		rect.y += rect.height * y;

		//recalculate mesh bounds with new vertex positions
		mesh.RecalculateBounds();

		offset.x -= (x / mesh.bounds.size.x) * width;
		offset.z -= (y / mesh.bounds.size.z) * height;
		foreach (Light light in Light.lights) {
			light.modified = true;
		}

		for (int i = 0; i < Glb.env.spawned_objs.Count; ++i) {
			Vector3 pos = Glb.env.spawned_objs[i].gobj.transform.position;
			pos.x -= rect.x;
			pos.z -= rect.y;
			if (pos.x < -half_width || pos.x > half_width || pos.z < -half_height || pos.z > half_height) {
				GameObject.Destroy(Glb.env.spawned_objs[i].gobj);
				if (Glb.env.spawned_objs[i].light != null) Glb.env.spawned_objs[i].light.remove();
				Glb.env.spawned_objs.RemoveAt(i);
				--i;
			}
		}

		Random.seed = (int)((rect.x * 10) + rect.y);
		int num_env_objs = Random.Range(1, 10);
		for (int n = 0; n < num_env_objs; ++n) {
			EnvAsset obj = Glb.env.get_rand_asset();
			Vector3 pos = new Vector3((x * half_width) + rect.x + (y * Random.Range(-half_width, half_width)), 0, 
									  (y * half_height) + rect.y + (x * Random.Range(-half_height, half_height)));
			Glb.env.create_obj(obj, pos);
		}
	}
}