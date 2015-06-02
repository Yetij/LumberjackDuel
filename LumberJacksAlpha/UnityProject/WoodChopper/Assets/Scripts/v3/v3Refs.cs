using UnityEngine;
using System.Collections.Generic;

public class v3Refs : MonoBehaviour
{
	#region hiden
	private static v3Refs _instance;
	public static v3Refs Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(v3Refs)) as v3Refs;
				if ( _instance == null ) throw new UnityException("Object of type v2Refs not found");
			}
			return _instance;
		}
	}
	#endregion

	void Awake () {
		camera = Camera.main;
	}

	public GameObject game_manager;
	public GameObject player_pref;
	public GameObject tree_pref;
	public GameObject net_control_node;
	[HideInInspector] public Camera camera;
	[HideInInspector] public int level_net_prefix=0;
	[HideInInspector] public List<v3Player> players = new List<v3Player>(2);

}

