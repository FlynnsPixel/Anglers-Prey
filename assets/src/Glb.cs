using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Glb : MonoBehaviour {

	public static Player player = new Player();
	public static Map map = new Map();
	public static Environment env = new Environment();
	public static EnemyManager em = new EnemyManager();
	public static Cam cam = new Cam();
	public static GUI_Manager gui = new GUI_Manager();

	void OnApplicationQuit() {
		LeapManager.dispose();
	}
}
