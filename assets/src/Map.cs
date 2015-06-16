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