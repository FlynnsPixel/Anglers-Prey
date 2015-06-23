﻿using UnityEngine;
using System.Collections;

public class StateManager : MonoBehaviour {

	enum State {
		TITLE_SCREEN, 
		GAME
	};

	State state;

	void Start() {
		Glb.map.init();
		Glb.env.init();
		Glb.em.init();
		Light.init();
		Glb.player.init();
		Glb.map.spawn_init_env();
	}

	void Update() {
		Glb.player.update();
		Glb.em.update();
		Light.update_all();
		Glb.map.update();
	}
}
