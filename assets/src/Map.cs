using System.Collections.Generic;
using UnityEngine;

public class Map {

	public float width;
	public float height;
	public const float MAX_VERTEX_WIDTH = 11;
	public const float MAX_VERTEX_HEIGHT = 11;
	public int vertices_per_row;

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

		resize_vertices(100);

		Light.lights.Add(Light.create(10, 0, 1, 1, 1, 0, 0, 1));
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