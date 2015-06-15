using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	Vector3 pos;
	Quaternion rota;
	Vector3 rota_euler;
	Vector2 accel;
	float angle = 90;
	float angle_accel = 0;

	GameObject map;
	Mesh map_mesh;
	Vector3[] map_vertices;
	Rect map_rect;
	Material map_material;
	Texture2D light_data;

	Vector3 cam_pos;
	Camera cam;

	bool mouse_touched = false;
	Vector3 last_mouse_pos;

	const float radians = Mathf.PI / 180.0f;
	const float max_speed = .175f;
	const float friction = .98f;
	const float angle_accel_speed = .15f;
	const float angle_friction = .95f;
	const float max_angle_accel = 4;

	float angle_offset = 0;
	float last_angle = 0;

	void Start() {
		pos = transform.position;
		rota = transform.rotation;
		rota_euler.x = 90;
		rota_euler.y = -90;
		rota_euler.z = -90;
		cam_pos = Camera.main.transform.position;
		cam = Camera.main;

		map = GameObject.Find("map");
		map_mesh = map.GetComponent<MeshFilter>().mesh;
		map_rect.x = map.transform.position.x;
		map_rect.y = map.transform.position.z;
		map_rect.width = map.transform.localScale.x;
		map_rect.height = map.transform.localScale.z;

		//width must be power of 2 or the point filter mode will get slightly interpolated
		light_data = new Texture2D(64, 1);
		//disable interpolation for the light data
		light_data.filterMode = FilterMode.Point;
		//clamp texture so it doesn't repeat
		light_data.wrapMode = TextureWrapMode.Clamp;
		create_light(256, 0, 1, 2.5f, 1, 0, 1, 1);

		Color c = new Color();
		c.r = 1;
		c.g = .5f;
		c.b = 0;
		c.a = 1;
		light_data.SetPixel(2, 0, c);
		float x = 0;
		float y = 20;
		c.r = (x / ((map_mesh.bounds.size.x * map_rect.width) / 2)) * 2;
		c.g = (y / ((map_mesh.bounds.size.z * map_rect.height) / 2)) * 2;
		c.b = 1;
		c.a = 4 / 10.0f;
		light_data.SetPixel(3, 0, c);

		light_data.Apply();
		map_material = map.GetComponent<Renderer>().material;
		map_material.SetInt("num_lights", 2);
	}

	void create_light(float x, float y, float size, float intensity, float r, float g, float b, float a) {
		Color c = new Color();
		c.r = r;
		c.g = g;
		c.b = b;
		c.a = a;
		light_data.SetPixel(0, 0, c);
		c.r = (x / ((map_mesh.bounds.size.x * map_rect.width) / 2)) * 2;
		c.g = (y / ((map_mesh.bounds.size.z * map_rect.height) / 2)) * 2;
		Debug.Log(c.r + ", " + c.g);
		c.b = size;
		c.a = intensity / 10.0f;
		light_data.SetPixel(1, 0, c);
	}

	void move_map(float x, float y) {
		//loop through all vertices in mesh and move by specified x and y
		map_vertices = map_mesh.vertices;
		for (int i = 0; i < map_mesh.vertexCount; ++i) {
			map_vertices[i].x += x;
			map_vertices[i].z += y;
		}
		map_mesh.vertices = map_vertices;

		//move map rect x, y by the width/height scale
		map_rect.x += map_rect.width * x;
		map_rect.y += map_rect.height * y;

		//recalculate mesh bounds with new vertex positions
		map_mesh.RecalculateBounds();
	}

	void Update() {
		create_light(-transform.position.x, -transform.position.z, .75f, 2.5f, .15f, .5f, .75f, 1);
		light_data.Apply();
		map_material.SetTexture("light_data", light_data);

		if (Input.GetMouseButtonDown(0)) {
			mouse_touched = true;
			last_mouse_pos = Input.mousePosition;
		}else if (Input.GetMouseButtonUp(0)) {
			mouse_touched = false;
		}
			
		if (mouse_touched) {
			Vector3 b = cam.WorldToScreenPoint(cam_pos + (transform.position - cam_pos));
			float c_x = (Screen.width / 2) + transform.position.x, c_y = (Screen.height / 2) + transform.position.z;
			float a = Mathf.Atan2(b.y - Input.mousePosition.y, b.x - Input.mousePosition.x) + (180 * radians);
			float target = a / radians;
			if (target < 170 && last_angle > 190) angle_offset += 360;
			else if (target > 190 && last_angle < 170) angle_offset -= 360;
			last_angle = target;

			angle -= (angle - (target + angle_offset)) / 10.0f;

			accel.x -= Mathf.Cos(angle * radians) * .01f;
			accel.y -= Mathf.Sin(angle * radians) * .01f;
			accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
			accel.y = Mathf.Clamp(accel.y, -max_speed, max_speed);
		}

		if (Input.GetKey(KeyCode.W)) {
			accel.x -= Mathf.Cos(angle * radians) * .01f;
			accel.y -= Mathf.Sin(angle * radians) * .01f;
			accel.x = Mathf.Clamp(accel.x, -max_speed, max_speed);
			accel.y = Mathf.Clamp(accel.y, -max_speed, max_speed);
		}
		accel *= friction;
		pos.x += accel.x;
		pos.z += accel.y;

		if (accel.x > 0 && pos.x > map_rect.x) {
			move_map(1, 0);
		}else if (accel.x < 0 && pos.x < map_rect.x) {
			move_map(-1, 0);
		}else if (accel.y > 0 && pos.z > map_rect.y) {
			move_map(0, 1);
		}else if (accel.y < 0 && pos.z < map_rect.y) {
			move_map(0, -1);
		}

		if (Input.GetKey(KeyCode.A)) {
			angle_accel += angle_accel_speed;
		}else if (Input.GetKey(KeyCode.D)) {
			angle_accel -= angle_accel_speed;
		}
		angle_accel = Mathf.Clamp(angle_accel, -max_angle_accel, max_angle_accel);
		angle_accel *= angle_friction;
		angle += angle_accel;

		rota.eulerAngles = rota_euler;
		rota_euler.x -= angle_accel;
		rota_euler.x = Mathf.Clamp(rota_euler.x, 45, 135);
		rota_euler.x -= (rota_euler.x - 90) / 20.0f;
		rota_euler.y = -angle;

		transform.position = pos;
		transform.rotation = rota;

		cam_pos.x -= (cam_pos.x - pos.x) / 20.0f;
		cam_pos.z -= (cam_pos.z - pos.z) / 20.0f;
		cam.transform.position = cam_pos;
	}
}
