using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class p1Map : MonoBehaviour
{
	p1Grid grid;

	int currentTurnNb;

	[RPC] void OnTurnStart (int turn_nb) {
		currentTurnNb = turn_nb;
	}

	[RPC] void OnBackgroundProcess () {
	}


	[RPC] void OnGameStart () {
	}

	[RPC] void OnGameEnd () {
	}

	public p1Cell GetPointedCell(Vector3 point ) {
		int x = (int ) (point.x - grid.root.x / grid.offset_x);
		int z = (int ) (point.z - grid.root.z / grid.offset_z);
		return grid[x,z];
	}

	#region hide
	private static p1Map _instance;
	public static p1Map Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p1Map)) as p1Map;
			}
			return _instance;
		}
	}
	#endregion
}

