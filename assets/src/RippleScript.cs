using UnityEngine;
using System.Collections;

public class RippleScript : MonoBehaviour {

    Material ripple_mat;

    float dmg_intensity = MAX_DMG_INTENSITY;
    float dmg_intensity_dest = MAX_DMG_INTENSITY;
    const float MAX_DMG_INTENSITY = 1.25f;

    float heal_intensity = MAX_HEAL_INTENSITY;
    float heal_intensity_dest = MAX_HEAL_INTENSITY;
    const float MAX_HEAL_INTENSITY = 1.25f;
    int last_eaten = 0;

    void Start() {
        ripple_mat = (Material)Resources.Load("ripple_mat");
    }

    void Update() {
        if (Glb.player.invincible) {
            if (Glb.player.invincible_timer > Player.INVINCIBLE_TIME / 1.2f) {
                dmg_intensity_dest = MAX_DMG_INTENSITY;
            }else {
                dmg_intensity_dest = .9f - ((Mathf.Cos(Time.timeSinceLevelLoad) + 1.0f) / 20.0f);
            }
        }else if (Glb.player.dashing) dmg_intensity_dest = 1.0f; else dmg_intensity_dest = MAX_DMG_INTENSITY;
        dmg_intensity -= (dmg_intensity - dmg_intensity_dest) / 20.0f;
        dmg_intensity = Mathf.Max(dmg_intensity, 1.0f);
        ripple_mat.SetFloat("dmg_intensity", dmg_intensity);

        if (last_eaten != Glb.em.fish_eaten) heal_intensity -= (Glb.em.fish_eaten - last_eaten) / 4.0f;
        heal_intensity = Mathf.Max(heal_intensity, 1.0f);
        last_eaten = Glb.em.fish_eaten;
        heal_intensity_dest -= (MAX_HEAL_INTENSITY - heal_intensity_dest) / 200.0f;
        heal_intensity -= (heal_intensity - heal_intensity_dest) / 20.0f;
        ripple_mat.SetFloat("heal_intensity", heal_intensity);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        //mat is the material containing your shader
        Graphics.Blit(source, destination, ripple_mat);
    }
}
