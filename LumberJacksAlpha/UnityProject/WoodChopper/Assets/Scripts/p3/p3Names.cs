using UnityEngine;
using System.Collections;

[System.Serializable]
public class Prefabs {
	public GameObject player;
	public GameObject scene;
}

[System.Serializable]
public class StaticStrings {
	public string defaultLobbyName;
	public string defaultRoomName;
	public string defaultPlayerName;
	public string connectionSettings;
	public string version;
}



public class p3Names : MonoBehaviour
{
	public StaticStrings staticStrings;
	public Prefabs prefabs;

	void Awake () {
		DontDestroyOnLoad(gameObject);
	}

	#region singleton
	private static p3Names _instance;
	public static p3Names Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p3Names)) as p3Names;
				if ( _instance == null ) throw new UnityException("Object of type p3Names not found");
			}
			return _instance;
		}
	}
	#endregion
}

