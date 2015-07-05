using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TurnState : byte{ P1=0, P2=1, None=2 , Both=3 }

public class p0CellController : MonoBehaviour
{
	#region hide
	private static p0CellController _instance;
	public static p0CellController Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p0CellController)) as p0CellController;
			}
			return _instance;
		}
	}
	#endregion
	
	[SerializeField] p0Grid grid;
	public byte gridX {
		get {
			return grid.total_x;
		}
	}
	public byte gridZ {
		get {
			return grid.total_z;
		}
	}
	[HideInInspector] public PhotonView netview;
	p0Const _const;

	void Start () {
		netview = GetComponent<PhotonView>();
		
		_const = p0Const.Instance;
	
		if ( !PhotonNetwork.isMasterClient) netview.RPC("__ClientCellControllerOk",PhotonTargets.MasterClient);
	}

	/* INITIALIZATION : rpc as signals */
	#region rpc
	[RPC] void __ClientCellControllerOk () {
		netview.RPC("__CreateGridMap",PhotonTargets.AllBuffered,new object[] { 
			_const.gridSettings.total_x, 
			_const.gridSettings.total_z,
			_const.gridSettings.offset_x,
			_const.gridSettings.offset_z,
			_const.gridSettings.root.x,
			_const.gridSettings.root.y,
			_const.gridSettings.root.z });
	}

	[RPC] void __CreateGridMap (byte x,byte z, float offx, float offz, float rootx, float rooty, float rootz) {
		grid = new p0Grid(x,z,offx,offz,rootx,rooty,rootz);
		var cells = grid.GetCells();
		if ( !PhotonNetwork.isMasterClient) netview.RPC("__ClientGridMapOk",PhotonTargets.MasterClient);
	}

	[RPC] void __ClientGridMapOk () {
		netview.RPC("__CreatePlayer",PhotonTargets.AllBufferedViaServer);
	}

	[RPC] void __CreatePlayer () {
		PhotonNetwork.Instantiate(p0Const.Instance.prefabNames._Player,new Vector3(0,0,0),Quaternion.identity,0);
	}

	List<p0Player> players;	
	[RPC] void __StartGame () {
		Debug.Log("Game started");
		xTime.Instance.OnGameStart();
		var l = GameObject.FindObjectsOfType(typeof(p0Player)) as p0Player[];
		players.AddRange(l);
		foreach ( var p in l ) {
			p.OnGameStart();
		}
	}
	
	[RPC] void __EndGame () {
		Debug.LogError("GAME END!");

	}

	#endregion

	public TurnState globalState { get; private set; }
	void SetTurn(TurnState state ) {
		netview.RPC("_SetTurn",PhotonTargets.All, (byte) state);
	}

	[RPC] void _SetTurn(byte state ) {
		globalState = (TurnState ) state;
	}


	/* public functions */
	#region public functions
	public p0Cell CellAt(int x, int z) {
		return grid[x,z];
	}

	public p0Player GetPlayer(int id ) {
		foreach ( var p in players ) {
			if ( p.netview.ownerId == id ) return p;
		}
		throw new UnityException("id = " + id + " no player found !");
	}
	
	public p0Player GetPlayer(int x, int z ) {
		var c = grid[x,z];
		if ( c == null ) return null;
		foreach ( var p in players ) {
			if ( (p.transform.position- c.position).magnitude < 0.1f ) return p;
		}
		return null;
	}
	#endregion

	/* public events */
	#region public events
	int _count = 0;
	public void OnPlayerReady () {
		_count ++;
		if ( _count == PhotonNetwork.room.maxPlayers  ) {
			netview.RPC("__StartGame",PhotonTargets.AllViaServer);
		}
	}
	#endregion


	#region debug gui
	public void _GUI () {

	}
	#if UNITY_EDITOR
	void OnDrawGizmos () {
		if ( grid == null ) return;
		for(int i=0; i < grid.total_x; i++ ) {
			for(int j=0; j <  grid.total_z; j ++ ) {
				Gizmos.DrawSphere(new Vector3(j* grid.offset_x+grid.root.x, 0, i*grid.offset_z+grid.root.z ),0.05f);
			}
		}
		
	}
	#endif
	#endregion
}

