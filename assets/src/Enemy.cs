using System.Collections.Generic;
using UnityEngine;

public enum AIType {
	DEFENSIVE		= 1, 
	AGGRESSIVE		= 1 << 1, 
	BATCH			= 1 << 2
};

public class Enemy {

	public GameObject gobj;
	public Light light = null;
	public bool to_be_removed = false;
	public AIType ai_type;

	public void update() {
		float dist = Mathf.Sqrt(Mathf.Pow(Glb.player.pos.x - gobj.transform.position.x, 2) + Mathf.Pow(Glb.player.pos.z - gobj.transform.position.z, 2));
		if (dist > Glb.map.width) { to_be_removed = true; return; }

		if (ai_type & AIType.DEFENSIVE == AIType.DEFENSIVE) {

		}
		if (ai_type & AIType.AGGRESSIVE == AIType.AGGRESSIVE) {

		}
		if (ai_type & AIType.BATCH == AIType.BATCH) {

		}
	}
}