using System;
using System.Collections.Generic;
using UnityEngine;

public class Light {

	//light members and functions
	private Color colour;
	private Color attribs;
	private Color pos;
		
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
	}

	//static variables and functions
	public static List<Light> lights = new List<Light>();	//a list of lights to be updated and rendered
	public static Texture2D light_data;						//the texture data to send to the shader that contains all light information
	public const int PIXEL_DATA_PER_LIGHT = 3;				//the number of pixels used per light
	public const float MAX_NUM_PIXELS = 64.0f;				//the maximum number of pixels in the light texture
	public const float MAX_NUM_LIGHTS = MAX_NUM_PIXELS / PIXEL_DATA_PER_LIGHT;	//the maximum number of lights that can be created

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

	public static Light create(float x, float y, float size, float intensity, float r, float g, float b, float a) {
		Light l = new Light();
		l.set_colour(r, g, b, a);
		l.set_attribs(size, intensity);
		l.set_pos(x, y);

		return l;
	}

	public static void update_all() {
		for (int n = 0; n < lights.Count; ++n) {
			light_data.SetPixel(n * PIXEL_DATA_PER_LIGHT, 0, lights[n].colour);
			light_data.SetPixel((n * PIXEL_DATA_PER_LIGHT) + 1, 0, lights[n].attribs);
			light_data.SetPixel((n * PIXEL_DATA_PER_LIGHT) + 2, 0, lights[n].pos);
		}
		light_data.Apply();
		Glb.map.material.SetInt("num_lights", lights.Count);
		Glb.map.material.SetTexture("light_data", light_data);
	}
}