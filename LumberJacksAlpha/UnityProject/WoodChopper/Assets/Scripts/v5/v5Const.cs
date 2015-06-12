using UnityEngine;
using System.Collections;

[System.Serializable]
public class Prefabs {
	public string _GameController;
	public string _Player;
	public string _Tree;
}
[System.Serializable]
public class ConnectionSettings {
	public string _GameVersion;
	public string _LobbyName;
	public int _MaxPlayerPerRoom;
	public LobbyType _LobbyType;
}

public class v5Const : MonoBehaviour
{
	public Prefabs prefabNames;
	public ConnectionSettings settings;
	
	private static v5Const _instance;
	public static v5Const Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(v5Const)) as v5Const;
				if ( _instance == null ) throw new UnityException("Object of type v5Const not found");
			}
			return _instance;
		}
	}
	
}

