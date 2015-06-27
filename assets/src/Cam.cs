using UnityEngine;
using System.Collections;

public class Cam {

	public Camera main;
	public Vector3 cam_pos;
	public Vector3 cam_init_pos;
	public Vector3 cam_rota;
	public Vector3 cam_init_rota;

	public void init() {
		main = Camera.main;
		cam_init_pos = Camera.main.transform.position;
		cam_pos = cam_init_pos;
		cam_init_rota = Camera.main.transform.localEulerAngles;
		cam_rota = cam_init_rota;
	}

	public void update() {
		cam_pos.x -= (cam_pos.x - Glb.player.pos.x) / 10.0f;
		cam_pos.z -= (cam_pos.z - Glb.player.pos.z) / 10.0f;
		main.transform.position = cam_pos;
		main.transform.localEulerAngles = cam_rota;
	}

	public void reset_pos() {
		main.transform.localEulerAngles = cam_init_pos;
	}

	public void reset_rota() {
		main.transform.localEulerAngles = cam_init_rota;
	}
}