using System;
using System.Collections.Generic;
using UnityEngine;

public class Light {

	//light members and functions
	private Color colour;
	private Color attribs;
	private Color pos;
	private Vector3 v_pos_min;
	private Vector3 v_pos_max;
	private Vector3 v_pos;
	private float attrib_size;
	private float attrib_intensity;
	private LightType type;
	public bool modified = false;

	public void set_colour(float r, float g, float b, float a) {
		colour.r = r;
		colour.g = g;
		colour.b = b;
		colour.a = a;
	}
	
	public void set_colour(Color c) {
		colour = c;
	}

	public void set_attribs(float size, float intensity) {
		attribs.r = size;
		attribs.g = intensity / 64.0f;
		attrib_size = size;
		attrib_intensity = intensity;
	}
	
	public void set_pos(float x, float y) {
		//converts x, y to uv coordinates of the map and then multiplies by 256 to be able to get the high and low bytes
		//the high byte and low bytes are then put in r, g, b, a respectively as a 0-1 range of 255 possible values per channel
		//this allows 65536 possible uv coordinates and can overflow if the coordinate is very far away (but unlikely)
		//the shader then decodes these values into uv coordinates

		float wave_scale = Glb.map.material.GetFloat("wave_scale") * 10.0f;

		int uv_x = (int)((x / (Glb.map.width / wave_scale)) * 256) + (256 * 127);
		pos.r = (uv_x >> 8) / 255.0f;
		pos.g = (uv_x % 256) / 255.0f;

		int uv_y = (int)((y / (Glb.map.height / wave_scale)) * 256) + (256 * 127);
		pos.b = (uv_y >> 8) / 255.0f;
		pos.a = (uv_y % 256) / 255.0f;

		v_pos.x = x;
		v_pos.z = y;
		v_pos_min.x = x - ((attrib_size * (Glb.map.width / 2)) / 2);
		v_pos_min.z = y - ((attrib_size * (Glb.map.height / 2)) / 2);
		v_pos_max.x = x + ((attrib_size * (Glb.map.width / 2)) / 2);
		v_pos_max.z = y + ((attrib_size * (Glb.map.height / 2)) / 2);
	}

	public void set_type(LightType t) {
		type = t;
		set_attribs(attrib_size, attrib_intensity);
		set_pos(v_pos.x, v_pos.z);
	}

	public Color get_colour() { return colour; }
	public Vector3 get_pos() { return v_pos; }
	public Vector3 get_min_pos() { return v_pos_min; }
	public Vector3 get_max_pos() { return v_pos_max; }
	public float get_size() { return attrib_size; }
	public float get_intensity() { return attrib_intensity; }
	public LightType get_type() { return type; }

	//static variables and functions
	public static List<Light> lights = new List<Light>();	//a list of lights to be updated and rendered
	public static Texture2D light_data;						//the texture data to send to the shader that contains all light information
	public const int PIXEL_DATA_PER_LIGHT = 3;				//the number of pixels used per light
	public const float MAX_NUM_PIXELS = 64.0f;				//the maximum number of pixels in the light texture
	public const float MAX_NUM_LIGHTS = MAX_NUM_PIXELS / PIXEL_DATA_PER_LIGHT;	//the maximum number of lights that can be created
	public static bool enable_off_screen = false;
	
	public enum LightType {
		PER_PIXEL, 
		VERTEX
	};

	public static void init() {
		//width must be power of 2 or the point filter mode will get slightly interpolated
		light_data = new Texture2D((int)MAX_NUM_PIXELS, 1);
		//disable interpolation for the light data
		light_data.filterMode = FilterMode.Point;
		//clamp texture so it doesn't repeat
		light_data.wrapMode = TextureWrapMode.Clamp;

		Glb.map.material.SetFloat("next_light_uv", 1.0f / (int)MAX_NUM_PIXELS);
		update_all();
	}

	public static Light create(float x, float y, float size, float intensity, float r, float g, float b, float a, LightType type = LightType.VERTEX) {
		Light l = new Light();
		l.set_colour(r, g, b, a);
		l.set_attribs(size, intensity);
		l.set_pos(x, y);
		l.set_type(type);

		return l;
	}

	public static void update_all() {
		//l button to toggle off screen light rendering
		if (Input.GetKeyDown(KeyCode.L)) Debug.Log("enable off screen lights: " + (enable_off_screen = !enable_off_screen));

		int offset_x = 0;
		int render_count = 0;
		int num_vertex_lights = 0;
		int num_pixel_lights = 0;
		Vector3 cam_pos = Camera.main.transform.position;
		cam_pos.y = 0;
		foreach (Light light in lights) {
			Vector3 c1 = Camera.main.WorldToViewportPoint(cam_pos - (light.get_min_pos() + cam_pos));
			Vector3 c2 = Camera.main.WorldToViewportPoint(cam_pos - (light.get_max_pos() + cam_pos));
			if (enable_off_screen || (c2.x > 0 && c1.x < 1 && c2.y > 0 && c1.y < 1)) {
				if (light.get_type() == LightType.VERTEX) {
					Color[] colours = Glb.map.mesh.colors;
					float v_x = -(light.v_pos.x / Glb.map.width) * Map.MAX_VERTEX_WIDTH;
					float v_z = -(light.v_pos.z / Glb.map.height) * Map.MAX_VERTEX_HEIGHT;

					for (int y = 0; y < Glb.map.vertices_per_row; ++y) {
						for (int x = 0; x < Glb.map.vertices_per_row; ++x) {
							int index = (y * Glb.map.vertices_per_row) + x;
							if (num_vertex_lights == 0) colours[index] = Color.black;

							float dist = Mathf.Sqrt(Mathf.Pow(Glb.map.vertices[index].x + v_x, 2) + Mathf.Pow(Glb.map.vertices[index].z + v_z, 2));
							if (dist < light.attrib_size) {
								if (Glb.map.vertices[index].x < 5 && Glb.map.vertices[index].x > -5 && 
									Glb.map.vertices[index].z < 5 && Glb.map.vertices[index].z > -5) {
									dist = Mathf.Clamp(light.attrib_intensity - 
														(dist / (light.attrib_size / light.attrib_intensity)), 0, light.attrib_intensity);
									//float v = Mathf.Clamp((1.0f / dist) / 10.0f, 0, .95f) * light.attrib_size;
									colours[index].r += dist * light.colour.r * light.colour.a;
									colours[index].g += dist * light.colour.g * light.colour.a;
									colours[index].b += dist * light.colour.b * light.colour.a;
								}
							}
						}
					}
					Glb.map.mesh.colors = colours;
					++num_vertex_lights;
				}else if (light.get_type() == LightType.PER_PIXEL) {
					light_data.SetPixel(offset_x, 0, light.colour);
					light_data.SetPixel(offset_x + 1, 0, light.attribs);
					light_data.SetPixel(offset_x + 2, 0, light.pos);
					offset_x += PIXEL_DATA_PER_LIGHT;
					++num_pixel_lights;
				}
			}
		}

		Glb.map.material.SetInt("num_lights", num_pixel_lights);
		if (num_pixel_lights > 0) {
			light_data.Apply();
			Glb.map.material.SetTexture("light_data", light_data);
		}
	}
}