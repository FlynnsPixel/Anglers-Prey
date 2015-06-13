using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Water {

    public class Water : MonoBehaviour {

        void Update() {
            if (!GetComponent<Renderer>()) return;
            Material mat = GetComponent<Renderer>().sharedMaterial;
            if (!mat) return;

            Vector4 waveSpeed = mat.GetVector("WaveSpeed");
            float waveScale = mat.GetFloat("_WaveScale");
            Vector4 waveScale4 = new Vector4(waveScale, waveScale, waveScale * 0.4f, waveScale * 0.45f);

            float t = (float)Time.timeSinceLevelLoad / 20.0f;
            mat.SetVector("_WaveOffset", new Vector4(
                waveSpeed.x * waveScale4.x * t,  
                waveSpeed.y * waveScale4.y * t, 
                waveSpeed.z * waveScale4.z * t, 
                waveSpeed.w * waveScale4.w * t
            ));
            mat.SetVector("_WaveScale4", waveScale4);
        }
    }
}