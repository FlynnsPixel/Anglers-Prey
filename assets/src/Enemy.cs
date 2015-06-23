using System.Collections.Generic;
using UnityEngine;

public class Enemy {

	public GameObject gobj;
	public Light light = null;
	public bool to_be_removed = false;
	public int ai_type;

	public const int AI_DEFENSIVE		= 1;
	public const int AI_AGGRESSIVE		= 1 << 1; 
	public const int AI_BATCH			= 1 << 2;

	public void update() {
		float dist = Mathf.Sqrt(Mathf.Pow(Glb.player.pos.x - gobj.transform.position.x, 2) + Mathf.Pow(Glb.player.pos.z - gobj.transform.position.z, 2));
		if (dist > Glb.map.width) { to_be_removed = true; return; }

		if ((ai_type & AI_DEFENSIVE) == AI_DEFENSIVE) {

		}
		if ((ai_type & AI_AGGRESSIVE) == AI_AGGRESSIVE) {

		}
		if ((ai_type & AI_BATCH) == AI_BATCH) {

		}
	}
}