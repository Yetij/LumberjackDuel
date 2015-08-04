using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class p1Prefabs {
	public string _GameController;
	public string _Player;
}
[System.Serializable]
public class p1ConnectionSettings {
	public string _GameVersion;
	public string _LobbyName;
	public int _MaxPlayerPerRoom;
	public LobbyType _LobbyType;
}

[System.Serializable]
public class p1GridMapSettings {
	public GameObject cell;
	public byte total_x, total_z;
	public float offset_x, offset_z;
	public Vector3 root;
}

[System.Serializable]
public class p1GameplaySettings {
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
public class p1PlayerSettings {
	public KeyCode chopKey = KeyCode.O;
	public KeyCode plantKey = KeyCode.P;
	public float moveSpeed = 6;
	public float rotateAngleSpeed = 720;
}

[System.Serializable]
public class p1TreeSettings {
	public float dominoDelay = 0.4f;
	public float additiveDisapearDelay = 1.1f;
}

[System.Serializable]
public class p1CellSettings {
	public Color reservedForPlayer = Color.red;
	public Color free = Color.white;
	public Color reservedForTree = Color.green;
	public Color availableInSelectMode = Color.grey;
	public Color selectedInSelectMode = Color.yellow;
}
[System.Serializable]
public class p1UiSetUp {
	public Toggle chopButtonSlot;
	public Toggle[] treeButtonSlots;
	public p1ConfirmButton confirmButtonSlot;
}

public class p1Const : MonoBehaviour
{
	public p1GameplaySettings gameplaySettings;
	public p1PlayerSettings playerSettings;
	public p1TreeSettings treeSettings;
	public p1CellSettings cellSettings;
	public p1GridMapSettings gridSettings;
	public p1Prefabs prefabNames;
	public p1UiSetUp uiSetUp;
	public p1ConnectionSettings netConnectionSettings;
	
	private static p1Const _instance;
	public static p1Const Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p1Const)) as p1Const;
				if ( _instance == null ) throw new UnityException("Object of type p1Const not found");
			}
			return _instance;
		}
	}
	
}

