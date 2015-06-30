using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class QuitHandler : MonoBehaviour {

	bool quit = false;		//doesn't actually do anything, but needed so compiler doesn't optimise out

	void Start() {
		quit = false;
	}

	void OnDestroy() {
		LeapManager.dispose();
		quit = true;
	}

	void OnApplicationQuit() {
		LeapManager.dispose();
		quit = true;
	}
}
