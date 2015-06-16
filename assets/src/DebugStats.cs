using UnityEngine;
using System.Collections;

public class DebugStats : MonoBehaviour {

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

	void Start() {
		int s_w = Screen.width, s_h = Screen.height;
		
		debug_rect = new Rect(0, 0, s_w, (s_h * 2) / 100);
		debug_style.alignment = TextAnchor.UpperLeft;
		debug_style.fontSize = (s_h * 2) / 80;
		debug_style.normal.textColor = new Color(1, 1, 1, 1);
	}

	void Update() {
		d_time += (Time.deltaTime - d_time) * .1f;
	}

	void OnGUI() {
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

		string text = avg_ms_result + " ms (" + avg_fps_result + " fps)";
		GUI.Label(debug_rect, text, debug_style);
	}
}
