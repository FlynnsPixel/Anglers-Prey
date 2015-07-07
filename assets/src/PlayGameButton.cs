using UnityEngine;
using System.Collections;

public class PlayGameButton : MonoBehaviour {

	void OnMouseDown() {
		Application.LoadLevel("game");
	}
}
