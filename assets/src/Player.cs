using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	void Start() {
        Debug.Log("player start");
	}

	void Update() {
		transform.Translate(Input.GetKey(KeyCode.D) ? .01f : Input.GetKey(KeyCode.A) ? -.01f : 0, 0, 0);
		transform.Translate(0, 0, Input.GetKey(KeyCode.W) ? .01f : Input.GetKey(KeyCode.S) ? -.01f : 0);
	}
}
