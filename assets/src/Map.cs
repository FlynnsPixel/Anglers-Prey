using System.Collections.Generic;
using UnityEngine;

public class Map {

	public float width;
	public float height;

	public GameObject map;
	public Mesh mesh;
	public Material material;
	public Vector3[] vertices;
	public Rect rect;

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

		vertices = mesh.vertices;
		//float dist;
		//float angle;
		//for (int i = 0; i < mesh.vertexCount; ++i) {
		//	if (vertices[i].x == 5 || vertices[i].x == -5 || vertices[i].z == 5 || vertices[i].z == -5) continue;
		//	dist = Mathf.Sqrt(Mathf.Pow(vertices[i].x, 2) + Mathf.Pow(vertices[i].z, 2));
		//	if (dist == 0) continue;
		//	angle = Mathf.Atan2(vertices[i].z, vertices[i].x);
		//	Debug.Log("angle: " + (angle/ (Mathf.PI / 180.0f)) + ", dist: " + dist + ", x: " + vertices[i].x + ", y: " + vertices[i].z);
		//	vertices[i].x = Mathf.Cos(angle);
		//	vertices[i].z = Mathf.Sin(angle);
		//}
		int size = 40;
		vertices = new Vector3[size * size];
		for (int y = 0; y < size; ++y) {
			for (int x = 0; x < size; ++x) {
				int index = (y * size) + x;
				vertices[index].x = ((float)x - (size / 2.0f)) / (size / 11.0f);
				vertices[index].z = ((float)y - (size / 2.0f)) / (size / 11.0f);
			}
		}

		draw_vertex_light(0, 0, 1);
		draw_vertex_light(4, 0, 4);

		int num_indices = (((size - 1) * (size - 1)) + (size - 2)) * 6;
		int[] indices = new int[num_indices];
		string temp = "";
		for (int i = 0; i < num_indices; i += 6) {
			int index = i / 6;
			indices[i] = index;
			indices[i + 1] = index + size;
			indices[i + 2] = index + size + 1;
			indices[i + 3] = index;
			indices[i + 4] = index + size + 1;
			indices[i + 5] = index + 1;
		}
		mesh.vertices = vertices;
		mesh.SetIndices(indices, MeshTopology.Triangles, 0);

		Color[] colours = new Color[mesh.vertexCount];
		mesh.colors = colours;
	}

	public void draw_vertex_light(float v_x, float v_z, float radius) {
		int size = 40;
		for (int y = 0; y < size; ++y) {
			for (int x = 0; x < size; ++x) {
				int index = (y * size) + x;
				if (vertices[index].x < 5 && vertices[index].x > -5 && vertices[index].z < 5 && vertices[index].z > -5) {
					float dist = Mathf.Sqrt(Mathf.Pow(vertices[index].x + v_x, 2) + Mathf.Pow(vertices[index].z + v_z, 2));
					if (dist < radius) {
						float angle = Mathf.Atan2(vertices[index].z + v_z, vertices[index].x + v_x);
						vertices[index].x = Mathf.Clamp(-v_x + (Mathf.Cos(angle) * radius), -5, 5);
						vertices[index].z = Mathf.Clamp(-v_z + (Mathf.Sin(angle) * radius), -5, 5);
					}
				}
			}
		}
	}

	public void update() {
		Light.update_all();
	}

	public void scroll_vertices(float x, float y) {
		//loop through all vertices in mesh and move by specified x and y
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

		Random.seed = (int)((rect.x * 10) + rect.y);
		float rand = Random.Range(0, 100);

		if (rand >= .5f) {
			//Light.lights.Add(Light.create(0, 0, 1, 2, 1, .5f, .25f, 1));
		}
	}
}