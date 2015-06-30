using System;
using System.Collections.Generic;
using UnityEngine;

public class Light {

	//light members and functions
	public bool modified = false;				//defines whether any of the lights attribs have been changed and are ready for an update
	public bool first_update = true;			//defines whether the light has had it's first update
	public bool on_screen = false;				//defines whether or not the light is on screen or not
	public bool n_test = false;

	private Color colour;						//r, g, b, a colour of the light
	private Color pp_attribs;					//per pixel encoded attribs of the light to send to the per pixel light shader
	private Color pp_pos;						//per pixel encoded position of the light to send to the per pixel light shader

	private Vector3 v_pos_min;					//contains the minimum position of the light
	private Vector3 v_pos_max;					//contains the maximum position of the light
	private Vector3 v_pos;						//contains the position of the light
	private Vector3 vcam_pos_min;				//contains the minimum negated position of the light used for camera screen space
	private Vector3 vcam_pos_max;				//contains the maximum negated position of the light used for camera screen space
	private float attrib_size;					//light size
	private float attrib_intensity;				//light intensity
	private LightType type;						//type of light: vertex or per pixel rendering

	private Color prev_colour;					//last updated colour of the light
	private float prev_attrib_size;				//last updated attrib size of the light
	private float prev_attrib_intensity;		//last updated attrib intensity of the light
	private Vector3 prev_v_pos;					//last updated position of the light
	private Vector3 prev_map_offset;

	private bool updated_prev_colour = false;	//defines whether prev colours have been calculated already or not
	private bool updated_prev_attribs = false;	//defines whether prev attribs have been calculated already or not
	private bool updated_prev_pos = false;		//defines whether prev positions have been calculated already or not
	private bool set_to_remove = false;			//defines whether the light is to be cleared and then removed or not

	public void set_colour(float r, float g, float b, float a) {
		if (!updated_prev_colour) {
			prev_colour = colour;
			updated_prev_colour = true;
		}
		colour.r = r;
		colour.g = g;
		colour.b = b;
		colour.a = a;
		if (first_update) prev_colour = colour;
	}
	
	public void set_colour(Color c) {
		if (!updated_prev_colour) {
			prev_colour = colour;
			updated_prev_colour = true;
		}
		colour = c;
		if (first_update) prev_colour = colour;
		modified = true;
	}

	public void set_attribs(float size, float intensity) {
		if (!updated_prev_attribs) {
			prev_attrib_size = attrib_size;
			prev_attrib_intensity = attrib_intensity;
			updated_prev_attribs = true;
		}

		attrib_size = size;
		attrib_intensity = intensity;

		if (first_update) {
			prev_attrib_size = attrib_size;
			prev_attrib_intensity = attrib_intensity;
		}

		calculate_min_max();

		if (type == Light.LightType.PER_PIXEL) {
			Vector3 cam_pos = Camera.main.transform.position;
			cam_pos.y = 0;
			//calculates the light size in uv coordinates by calculating max - min position in relation to the screen
			float uv_size = (Camera.main.WorldToViewportPoint(cam_pos - (vcam_pos_max + cam_pos)) - 
							 Camera.main.WorldToViewportPoint(cam_pos - (vcam_pos_min + cam_pos))).x;
			//calculates high byte and low byte for the uv_size
			int val = (int)(((uv_size / 2.0f) * 255) * (Map.MAX_VERTEX_WIDTH / 2.0f));
			pp_attribs.r = (val / 255) / 255.0f;
			pp_attribs.g = (val % 255) / 255.0f;
			pp_attribs.b = intensity / 64.0f;
		}

		modified = true;
	}

	public void set_pos(float x, float y) {
		if (type == Light.LightType.PER_PIXEL) {
			//converts x, y to uv coordinates of the map and then multiplies by 256 to be able to get the high and low bytes
			//the high byte and low bytes are then put in r, g, b, a respectively as a 0-1 range of 255 possible values per channel
			//this allows 65536 possible uv coordinates and can overflow if the coordinate is very far away (but unlikely)
			//the shader then decodes these values into uv coordinates

			float wave_scale = Glb.map.material.GetFloat("wave_scale") * 10.0f;

			int uv_x = (int)((x / (Glb.map.width / wave_scale)) * 256) + (256 * 127);
			pp_pos.r = (uv_x >> 8) / 255.0f;
			pp_pos.g = (uv_x % 256) / 255.0f;

			int uv_y = (int)((y / (Glb.map.height / wave_scale)) * 256) + (256 * 127);
			pp_pos.b = (uv_y >> 8) / 255.0f;
			pp_pos.a = (uv_y % 256) / 255.0f;
		}

		if (!updated_prev_pos) {
			prev_v_pos.x = v_pos.x;
			prev_v_pos.z = v_pos.z;
			updated_prev_pos = true;
		}

		v_pos.x = x;
		v_pos.z = y;

		calculate_min_max();

		if (first_update) {
			prev_v_pos.x = v_pos.x;
			prev_v_pos.z = v_pos.z;
		}

		modified = true;
	}

	public void set_type(LightType t) {
		type = t;
		set_attribs(attrib_size, attrib_intensity);
		set_pos(v_pos.x, v_pos.z);
	}

	private void calculate_min_max() {
		v_pos_min.x = v_pos.x - attrib_size;
		v_pos_min.z = v_pos.z - attrib_size;
		v_pos_max.x = v_pos.x + attrib_size;
		v_pos_max.z = v_pos.z + attrib_size;

		vcam_pos_min.x = -v_pos.x - attrib_size;
		vcam_pos_min.z = -v_pos.z - attrib_size;
		vcam_pos_max.x = -v_pos.x + attrib_size;
		vcam_pos_max.z = -v_pos.z + attrib_size;
	}

	public void remove() {
		set_to_remove = true;
		modified = true;
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
	public static int num_vertex_lights = 0;
	public static int num_pixel_lights = 0;

	private static float unique_light_id = 100;
	private const float UNIQUE_ID_DIF = 1;
	private static Color[] map_colours;

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

		map_colours = Glb.map.mesh.colors;

		Glb.map.material.SetFloat("next_light_uv", 1.0f / (int)MAX_NUM_PIXELS);
		update_all();
	}

	public static Light create(float x, float y, float size, float intensity, float r, float g, float b, float a, LightType type = LightType.VERTEX) {
		Light l = new Light();
		l.set_colour(r, g, b, a);
		l.attrib_size = size;
		l.set_pos(x, y);
		l.set_attribs(size, intensity);
		l.set_type(type);

		return l;
	}

	private static void draw_vertex_circle(Color colour, float size, float intensity, Vector3 pos, Vector3 map_offset, bool negate_colour = false) {
		//convert light world position + map offset to a center vertex position
		float wave_scale = Glb.map.material.GetFloat("wave_scale") / 10.0f;
		float v_x = (((((pos.x * (1 - wave_scale)) + map_offset.x) / Glb.map.width) * 
						Glb.map.vertices_per_row)) + (Glb.map.vertices_per_row / 2.0f);
		float v_z = (((((pos.z * (1 - wave_scale)) + map_offset.z) / Glb.map.height) * 
						Glb.map.vertices_per_row)) + (Glb.map.vertices_per_row / 2.0f);

		//divide dist by vertices / 2 / 10 to make draw results the same over different amounts of vertices on the map
		float dist_modifier = Glb.map.vertices_per_row / 20.0f;
		float vertex_size = size / (Map.MAX_VERTEX_WIDTH / 2.0f);

		//draw a circle and fill it in while applying colours to the vertex points the circle encounters
		int r = 1;
		float radius = 0;
		bool drawn = false;
		while (!drawn) {
			float p = 360.0f / r;
			for (int n = 0; n < r; ++n) {
				//calculate the vertex x and y from the angle of the current rotation and radius
				int x = (int)Mathf.Round((Mathf.Cos((p * n) / (180.0f / Mathf.PI)) * radius) + v_x);
				int y = (int)Mathf.Round((Mathf.Sin((p * n) / (180.0f / Mathf.PI)) * radius) + v_z);
				int index = (y * Glb.map.vertices_per_row) + x;

				//calculate distance from the current position to the light position
				float dist = Mathf.Sqrt(Mathf.Pow(x - v_x, 2) + Mathf.Pow(y - v_z, 2)) / dist_modifier;
				if (dist > vertex_size) { drawn = true; continue; }
				//if index out of bounds or the colour has been visited before during drawing, continue
				if (x < 0 || x > Glb.map.vertices_per_row - 1 || y < 0 || y > Glb.map.vertices_per_row - 1 || map_colours[index].a == unique_light_id) continue;

				//use custom light formula to calculate the r, g, b, a light values with the light size, intensity and dist from center
				dist = Mathf.Clamp(intensity - (dist / (vertex_size / intensity)), 0, intensity);
				//dist *= Mathf.Clamp((Mathf.Abs(x - v_x) + Mathf.Abs(y - v_z)) / 4.0f, 1.0f, 2.0f);
				if (negate_colour) dist = -dist;
				map_colours[index].r += (dist * colour.r) * colour.a;
				map_colours[index].g += (dist * colour.g) * colour.a;
				map_colours[index].b += (dist * colour.b) * colour.a;
				map_colours[index].a = unique_light_id;
			}
			r += 8;
			radius += .8f;
		}
		unique_light_id -= UNIQUE_ID_DIF;
	}

	public static void update_all() {
		//l button to toggle off screen light rendering
		if (Input.GetKeyDown(KeyCode.L)) Debug.Log("enable off screen lights: " + (enable_off_screen = !enable_off_screen));

		//check if the unique light id is near zero, and if it is then clear all vertex alpha colours
		//alpha colours are used to determine if a colour has been already calculated when drawing vertex lights
		unique_light_id = 100;
		for (int y = 0; y < Glb.map.vertices_per_row; ++y) {
			for (int x = 0; x < Glb.map.vertices_per_row; ++x) {
				map_colours[(y * Glb.map.vertices_per_row) + x] = Color.black;
			}
		}

		num_vertex_lights = 0;
		num_pixel_lights = 0;

		Glb.cam.reset_rota();
		Vector3 cam_pos = Glb.cam.cam_pos;
		cam_pos.y = 0;

		//loop through all the lights
		//send per pixel light data to the shader for per pixel lights
		//calculate vertex lights and apply them to the vertex colours of the map
		for (int i = 0; i < lights.Count; ++i) {
			Light light = lights[i];
			//if (light.get_type() == LightType.VERTEX && !light.modified && !light.set_to_remove) continue;

			//calculates the light position in relation to the screen so lights will not update if they are off screen
			Vector3 c1 = Camera.main.WorldToViewportPoint(cam_pos - (light.vcam_pos_min + cam_pos));
			Vector3 c2 = Camera.main.WorldToViewportPoint(cam_pos - (light.vcam_pos_max + cam_pos));
			if ((enable_off_screen || (c2.x > 0 && c1.x < 1 && c2.y > 0 && c1.y < 1)) && !light.set_to_remove) {
				draw_vertex_circle(light.colour, light.attrib_size, light.attrib_intensity, light.v_pos, Glb.map.offset, false);
				++num_vertex_lights;
			}
			if (light.set_to_remove) { lights.RemoveAt(i); light.set_to_remove = false; --i; }
			//fully uncommented code for static light optimisation
			//	if (light.get_type() == LightType.VERTEX) {
			//		if (light.modified) {
			//			//if any of the lights values have been modified, then draw a vertex circle with the lights attribs
			//			draw_vertex_circle(light.colour, light.attrib_size, light.attrib_intensity, light.v_pos, Glb.map.offset, false);
			//			//draw the previous drawn vertex light but instead of adding the colour, subtract it instead
			//			//this allows dynamic lights to be modified and static lights to be only modified once
			//			if (!light.first_update && light.on_screen) draw_vertex_circle(light.prev_colour, light.prev_attrib_size, 
			//																		   light.prev_attrib_intensity, light.prev_v_pos, 
			//																		   light.prev_map_offset, true);

			//			light.prev_map_offset = Glb.map.offset;
			//			light.modified = false;
			//			light.first_update = false;
			//			light.on_screen = true;
			//			light.updated_prev_colour = false;
			//			light.updated_prev_attribs = false;
			//			light.updated_prev_pos = false;
			//			++num_vertex_lights;
			//		}
			//	}else if (light.get_type() == LightType.PER_PIXEL) {
			//		//if the light is a per pixel light then set the light per pixel data
			//		//to the light data texture
			//		light_data.SetPixel((num_pixel_lights * PIXEL_DATA_PER_LIGHT), 0, light.colour);
			//		light_data.SetPixel((num_pixel_lights * PIXEL_DATA_PER_LIGHT) + 1, 0, light.pp_attribs);
			//		light_data.SetPixel((num_pixel_lights * PIXEL_DATA_PER_LIGHT) + 2, 0, light.pp_pos);
			//		++num_pixel_lights;
			//	}
			//}else {
			//	if (light.get_type() == LightType.VERTEX) {
			//		if (light.on_screen || light.set_to_remove) {
			//			//the light is now off screen, so remove it's previous data from the vertex map only once
			//			//until it is on screen again
			//			if (!light.first_update) draw_vertex_circle(light.prev_colour, light.prev_attrib_size, 
			//														light.prev_attrib_intensity, light.prev_v_pos, 
			//														light.prev_map_offset, true);

			//			light.modified = true;
			//			light.first_update = false;
			//			light.on_screen = false;
			//		}
			//	}
			//	if (light.set_to_remove) { lights.RemoveAt(i); light.set_to_remove = false; --i; }
			//}
		}
		Glb.map.mesh.colors = map_colours;

		Glb.map.material.SetInt("num_lights", num_pixel_lights);
		if (num_pixel_lights > 0) {
			light_data.Apply();
			Glb.map.material.SetTexture("light_data", light_data);
		}

		Glb.cam.update();
	}
}