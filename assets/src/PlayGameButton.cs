using UnityEngine;
using System.Collections;

public class PlayGameButton : MonoBehaviour {

	void OnMouseUp() {
		Application.LoadLevel("game");
	}
}
