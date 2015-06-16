using System;
using System.Collections.Generic;
using UnityEngine;

public class MapAnimate : MonoBehaviour {

    void Update() {
        if (!GetComponent<Renderer>()) return;
        Material mat = GetComponent<Renderer>().sharedMaterial;
        if (!mat) return;

        Vector4 wave_speed = mat.GetVector("wave_speed");
        float wave_scale = mat.GetFloat("wave_scale");
        
        float t = (float)Time.timeSinceLevelLoad / 20.0f;
        mat.SetVector("wave_offset", new Vector4(
            wave_speed.x * wave_scale * t,  
            wave_speed.y * wave_scale * t, 
            wave_speed.z * (wave_scale * 0.4f) * t, 
            wave_speed.w * (wave_scale * 0.45f) * t
        ));
    }
}