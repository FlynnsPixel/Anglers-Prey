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
		Light.init();
		Glb.player.init();
	}

	void Update() {
		Glb.player.update();
		Glb.map.update();
	}
}
