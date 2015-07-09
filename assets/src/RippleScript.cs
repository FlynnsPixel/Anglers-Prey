using UnityEngine;
using System.Collections;

public class RippleScript : MonoBehaviour {

    Material ripple_mat;

	void Start() {
        ripple_mat = (Material)Resources.Load("ripple_mat");
	}

    void Update() {
	
	}

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        //mat is the material containing your shader
        Graphics.Blit(source, destination, ripple_mat);
    }
}
