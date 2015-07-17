using UnityEngine;
using System.Collections;

[System.Serializable]
public class p0Prefabs {
	public string _GameController;
	public string _Player;
}
[System.Serializable]
public class p0ConnectionSettings {
	public string _GameVersion;
	public string _LobbyName;
	public int _MaxPlayerPerRoom;
	public LobbyType _LobbyType;
}

[System.Serializable]
public class p0GridMapSettings {
	public GameObject cell;
	public byte total_x, total_z;
	public float offset_x, offset_z;
	public Vector3 root;
}

[System.Serializable]
public class p0GameplaySettings {
	public int playerMaxHp = 1;
	public int treeFallDamage = 1;
	public int directChopDamage = 1;
	public int pointsToWin = 20;
	public float timePerTurn = 10f;
	public int actionPointsPerTurn = 4;
	public int chopActionCost = 1;
	public int plantActionCost = 1;
	public int moveActionCost = 1;
	public int startTreeNb=8;
	public int genTreeMin=2;
	public int genTreeMax=3;
}

[System.Serializable]
public class p0PlayerSettings {
	public KeyCode chopKey = KeyCode.O;
	public KeyCode plantKey = KeyCode.P;
	public float moveSpeed = 6;
	public float rotateAngleSpeed = 720;
}

[System.Serializable]
public class p0TreeSettings {
	public float dominoDelay = 0.4f;
	public float additiveDisapearDelay = 1.1f;
}

[System.Serializable]
public class p0CellSettings {
	public Color reservedForPlayer = Color.red;
	public Color free = Color.white;
	public Color reservedForTree = Color.green;
	public Color availableInSelectMode = Color.grey;
	public Color selectedInSelectMode = Color.yellow;
}

public class p0Const : MonoBehaviour
{
	public p0GameplaySettings gameplaySettings;
	public p0PlayerSettings playerSettings;
	public p0TreeSettings treeSettings;
	public p0CellSettings cellSettings;
	public p0GridMapSettings gridSettings;
	public p0Prefabs prefabNames;
	public p0ConnectionSettings netConnectionSettings;
	
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

