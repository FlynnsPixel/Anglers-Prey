using UnityEngine;
using System.Collections;

public class StateManager : MonoBehaviour {

	enum State {
		TITLE_SCREEN, 
		GAME
	};

	State state;

	void Start() {
		Glb.map.init();
		Glb.cam.init();
		Glb.env.init();
		Glb.player.init();
		Glb.em.init();
		Light.init();
		Glb.map.spawn_init_env();
		Glb.gui.init();
	}

	void Update() {
		Glb.cam.update();
		Glb.map.update();
		Glb.player.update();
		Glb.em.update();
		Light.update_all();
		LeapManager.update();
		Glb.gui.update();
	}

	void OnGUI() {
		Glb.gui.update_gui();
	}
}
