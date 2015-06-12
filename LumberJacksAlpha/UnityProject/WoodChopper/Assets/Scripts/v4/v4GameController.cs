using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
public class v4GameController : MonoBehaviour
{
	private static v4GameController _instance;
	public static v4GameController Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(v4GameController)) as v4GameController;
				if ( _instance == null ) throw new UnityException("Object of type v4GameController not found");
			}
			return _instance;
		}
	}

	public Grid grid;
	public event OnReadyHandle OnReady;
	Cell[,] cells;
	v4Pool pool;
	public PhotonView netview;
	
	List<v4Player> players;

	public Cell Get(int x,int z ) {
		return cells[x,z];
	}

	public bool ValidIndex(int x,int z ) {
		return x >= 0 & x < grid.total_x & z >=0 & z < grid.total_z;
	}

	[RPC] public void SetTreeBeingChopped (int x,int z, bool isBeingChopped ) {
		if( ValidIndex(x,z) ) {
			v4Tree t = Get(x,z).tree;
			if ( t != null ) {
				if ( isBeingChopped ) t.OnBeingCutStart();
				else t.OnBeingCutEnd();
			}
		}
	}

	void Awake () {
		var l = GameObject.FindObjectsOfType(typeof(v4GameController));
		if ( l.Length > 1 ) {
			Destroy(l[l.Length -1]);
			return;
		}
		netview = GetComponent<PhotonView>();
	}

	void Start () {
		cells = grid.GenCells();
		pool = v4Pool.Instance;
		pool.Initialize();
		players = new List<v4Player>(2);
		if ( PhotonNetwork.isMasterClient) netview.RPC("OnGameControllerInstantiated",PhotonTargets.AllBufferedViaServer);
	}

	void Update () {
		if ( !gameStarted ) return;
	}

	[RPC] void OnGameControllerInstantiated () {
		PhotonNetwork.Instantiate(v4ConstValue.Instance.prefabNames._Player,new Vector3(0,0,0),Quaternion.identity,0);
	}


	[RPC] void OnPlayerReady () {
		if (PhotonNetwork.isMasterClient ) {
			if ( PhotonNetwork.room.playerCount == PhotonNetwork.room.maxPlayers  ) {
				var l = GameObject.FindObjectsOfType(typeof(v4Player) );
				foreach ( Object g in l ) {
					players.Add(((GameObject) g).GetComponent<v4Player>());
				}
				/* >> gen starting map */
				StartDaGame();
			}
		}
	}

	bool gameStarted=false;
	void StartDaGame () {
		gameStarted = true;
		if( OnReady != null ) {
			OnReady();
		}
	}

	public void UpdateCellProp ( int x,int z, bool canGrowTree, bool canStepOn) {
		var c = cells[x,z];
		if ( c.canGrowTree != canGrowTree ) netview.RPC("CellProp1",PhotonTargets.AllBuffered, new object[]{x,z,canGrowTree});
		if ( c.canStepOn != canStepOn ) netview.RPC("CellProp2",PhotonTargets.AllBuffered, new object[]{x,z,canStepOn} );
	}

	public void UpdateCellProp2 ( int x, int z , int player_id) {	
		if ( cells[x,z].locked != player_id ) netview.RPC("CellProp3",PhotonTargets.AllBufferedViaServer, new object[] {x,z, player_id });
	}

	[RPC] void CellProp1 (int x,int z, bool canGrowTree ) {
		cells[x,z].canGrowTree = canGrowTree;
	}

	[RPC] void CellProp2 (int x,int z, bool canStepOn ) {
		cells[x,z].canStepOn = canStepOn;
	}

	[RPC] void CellProp3 (int x, int z, int id ) {
		if ( id == -1 ) {
			cells[x,z].locked = -1;
			return;
		}
		if ( cells[x,z].locked != -1 ) return;
		cells[x,z].locked = id;
	}

	#if UNITY_EDITOR
	void OnDrawGizmos () {
		for(int i=0; i < grid.total_x; i++ ) {
			for(int j=0; j <  grid.total_z; j ++ ) {
				Gizmos.DrawSphere(new Vector3(j* grid.offset_x+grid.root.x, 0, i*grid.offset_z+grid.root.z ),0.05f);
			}
		}
	}
	#endif
}

