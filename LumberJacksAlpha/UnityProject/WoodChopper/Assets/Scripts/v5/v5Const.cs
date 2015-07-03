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
[System.Serializable]
public class KeyboardSettings {
	public KeyCode chop;
	public KeyCode plant;
}

[System.Serializable]
public class TreeGeneralSettings {
	public float[] maxHp = { 100, 150, 200 };
	public float[] additiveLifeTimeStage = { 0.2f, 0.3f, 0.5f };  /* sum must be 1 */
	public float[] sizeScale = { 0.45f, 0.7f, 1f };
}

[System.Serializable]
public class GridMapSettings {
	public byte total_x, total_z;
	public float offset_x, offset_z;
	public Vector3 root;
}
public class v5Const : MonoBehaviour
{
	public Prefabs prefabNames;
	public ConnectionSettings netConnectionSettings;
	public KeyboardSettings keyboardSettings;
	public TreeGeneralSettings treeGeneralSettings;
	public GridMapSettings gridSettings;
	
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

