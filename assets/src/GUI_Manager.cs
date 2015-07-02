using UnityEngine;
using System.Collections;

public class GUI_Manager {

	float d_time = 0;

	float avg_fps = 0;
	float avg_fps_result = 0;
	float avg_fps_count = 0;
	const int avg_fps_rate = 16;

	float avg_ms = 0;
	float avg_ms_result = 0;
	float avg_ms_count = 0;
	const int avg_ms_rate = 16;

	GUIStyle debug_style = new GUIStyle();
	Rect debug_rect;
	bool debug_stats_active = false;

	Rect time_gui_rect;
	Texture time_texture;

	Rect rstats_gui_rect;
	Texture rstats_texture;

	GUIStyle timer_style = new GUIStyle();
	Rect timer_rect;
	int timer = 0;

	GUIStyle rstats_style = new GUIStyle();
	Rect rstats_rect;
	int rstats = 0;
	float rstats_scale = 1;
	float rstats_dest_scale = 1;
	int rstats_font_size = 1;
	float rstats_scale_accel = 0;

	public void init() {
		time_texture = Resources.Load("textures/time_gui") as Texture;
		rstats_texture = Resources.Load("textures/rstats_gui") as Texture;

		float w = time_texture.width;
		time_gui_rect = new Rect(0, 0, w, time_texture.height);
		rstats_gui_rect = new Rect(Screen.width - w, 0, w, rstats_texture.height);

		int s_w = Screen.width, s_h = Screen.height;
		
		debug_rect = new Rect(0, time_texture.height, s_w, (s_h * 2) / 100);
		debug_style.alignment = TextAnchor.UpperLeft;
		debug_style.fontSize = (s_h * 2) / 80;
		debug_style.normal.textColor = new Color(1, 1, 1, 1);

		Font gui_font = Resources.Load("fonts/gui_font") as Font;

		timer_rect = new Rect((time_gui_rect.width / 2.0f) - 125, (time_gui_rect.height / 2.0f) - 40, timer_rect.width, timer_rect.height);
		timer_style.alignment = TextAnchor.UpperLeft;
		timer_style.fontSize = (int)(time_texture.height / 2.0f);
		timer_style.normal.textColor = new Color(1, 1, 1, 1);
		timer_style.font = gui_font;

		rstats_rect = new Rect(rstats_gui_rect.x + ((rstats_gui_rect.width / 2.0f) - 50), (rstats_gui_rect.height / 2.0f) - 40, rstats_rect.width, rstats_rect.height);
		rstats_style.alignment = TextAnchor.UpperLeft;
		rstats_style.fontSize = (rstats_font_size = (int)(rstats_texture.height / 2.0f));
		rstats_style.normal.textColor = new Color(1, 1, 1, 1);
		rstats_style.font = gui_font;
	}

	public void update() {
		d_time += (Time.deltaTime - d_time) * .1f;
		if (Input.GetKeyDown(KeyCode.H)) debug_stats_active = !debug_stats_active;
	}

	public void update_gui() {
		GUI.DrawTexture(time_gui_rect, time_texture);
		GUI.DrawTexture(rstats_gui_rect, rstats_texture);

		if (debug_stats_active) {
			//add all fps values calculated and average the result if greater than fps_rate
			float fps = 1.0f / d_time;
			++avg_fps_count;
			avg_fps += fps;
			if (avg_fps_count > avg_fps_rate) { avg_fps_result = (int)(avg_fps / avg_fps_count); avg_fps_count = 0; avg_fps = 0; }

			//add all ms values calculated and average the result if greater than the ms_rate
			float ms = d_time * 1000.0f;
			++avg_ms_count;
			avg_ms += ms;
			if (avg_ms_count > avg_ms_rate) { avg_ms_result = (int)(avg_ms / avg_ms_count); avg_ms_count = 0; avg_ms = 0; }

			string text = avg_ms_result + " ms (" + avg_fps_result + " fps)\n";
			text += "Pixel lights: " + Light.num_pixel_lights + ", Vertex lights: " + Light.num_vertex_lights + "\n";
			text += "Enable off screen lights (L): " + Light.enable_off_screen + "\n";
			text += "Leap enabled (P): " + LeapManager.enabled + "\n";
			text += "Leap connected: " + LeapManager.connected + "\n";
			GUI.Label(debug_rect, text, debug_style);
		}

		string timer_str = "";
		timer_str += (int)(Time.realtimeSinceStartup / 60.0f);
		timer_str += ":";
		string time_seconds = System.Convert.ToString((int)(Time.realtimeSinceStartup % 60));
		if (time_seconds.Length == 1) time_seconds = "0" + time_seconds;
		timer_str += time_seconds;
		timer_str += ":";
		string time_ms = System.Convert.ToString((int)((Time.realtimeSinceStartup % 1.0f) * 100));
		timer_str += time_ms;

		timer_style.normal.textColor = new Color(.5f + (Mathf.Sin(Time.timeSinceLevelLoad * 2.0f) / 2.0f), .5f, Mathf.Cos(Time.timeSinceLevelLoad) / 2.0f, 1);
		GUI.Label(timer_rect, timer_str, timer_style);

		string rstats_str = Glb.em.fish_eaten + "  eaten";
		rstats_style.normal.textColor = new Color(Mathf.Sin((Time.timeSinceLevelLoad + 40) / 2.0f), .5f, .5f + (Mathf.Cos((Time.timeSinceLevelLoad + 40) * 2.0f) / 2.0f), 1);
		rstats_scale_accel = Mathf.Clamp((rstats_scale - rstats_dest_scale) / 10.0f, 0, .1f);
		rstats_scale += rstats_scale_accel;
		if (rstats_scale >= rstats_dest_scale - .01f) rstats_dest_scale = 1;
		rstats_style.fontSize = (int)(rstats_font_size * rstats_scale);
		GUI.Label(rstats_rect, rstats_str, rstats_style);
	}

	public void scale_rstats() {
		rstats_dest_scale = 2;
	}
}
