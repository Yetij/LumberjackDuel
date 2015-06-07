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

public class v4ConstValue : MonoBehaviour
{
	public Prefabs prefabNames;
	public ConnectionSettings settings;

	private static v4ConstValue _instance;
	public static v4ConstValue Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(v4ConstValue)) as v4ConstValue;
				if ( _instance == null ) throw new UnityException("Object of type v4ConstValue not found");
			}
			return _instance;
		}
	}

}

