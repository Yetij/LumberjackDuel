using UnityEngine;
using System.Collections;

[System.Serializable]
public class p0Prefabs {
	public string _GameController;
	public string _Player;
	public string _Tree;
}
[System.Serializable]
public class p0ConnectionSettings {
	public string _GameVersion;
	public string _LobbyName;
	public int _MaxPlayerPerRoom;
	public LobbyType _LobbyType;
}
[System.Serializable]
public class p0KeyboardSettings {
	public KeyCode chop;
	public KeyCode plant;
}

[System.Serializable]
public class p0GridMapSettings {
	public GameObject cell;
	public byte total_x, total_z;
	public float offset_x, offset_z;
	public Vector3 root;
}
public class p0Const : MonoBehaviour
{
	public p0Prefabs prefabNames;
	public p0ConnectionSettings netConnectionSettings;
	public p0KeyboardSettings keyboardSettings;
	public p0GridMapSettings gridSettings;
	
	private static p0Const _instance;
	public static p0Const Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p0Const)) as p0Const;
				if ( _instance == null ) throw new UnityException("Object of type p0Const not found");
			}
			return _instance;
		}
	}

}

