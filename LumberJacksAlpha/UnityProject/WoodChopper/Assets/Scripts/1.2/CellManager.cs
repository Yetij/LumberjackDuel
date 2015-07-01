using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellManager : MonoBehaviour
{
	#region hide
	private static CellManager _instance;
	public static CellManager Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(CellManager)) as CellManager;
			}
			return _instance;
		}
	}
	#endregion

	[SerializeField] Grid grid;
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
	v5Const _const;
	TreePool pool;
	[HideInInspector] public List<Cell> freeCells;
	List<Player> players;

	void Start () {
		netview = GetComponent<PhotonView>();
		var cells = grid.GetCells();

		_const = v5Const.Instance;
		pool = TreePool.Instance;
		
		freeCells = new List<Cell>();
		foreach( var c in cells ) {
			freeCells.Add(c);
		}
		
		players = new List<Player>(2);
		if ( !PhotonNetwork.isMasterClient) netview.RPC("__ClientCellManagerReady",PhotonTargets.MasterClient);
	}


	public Cell CellAt(int x, int z ) {
		return grid[x,z];
	}

	[RPC] void __ClientCellManagerReady () {
		netview.RPC("__CreatePlayer",PhotonTargets.AllViaServer);
	}

	[RPC] void __CreatePlayer () {
		PhotonNetwork.Instantiate(v5Const.Instance.prefabNames._Player,new Vector3(0,0,0),Quaternion.identity,0);
	}

	[RPC] void __PlantTree (int x,int z, double t) {
		var c = grid[x,z];
		if ( c != null ) {
			if ( t < c.lock_time & c.locked != -2 ) {
				TreePool.Instance.Get().AttachToCell(c,t);
			}
		}
	}

	int _count = 0;
	public void OnPlayerReady () {
		if ( !PhotonNetwork.isMasterClient ) return;
		_count ++;
		if ( _count == PhotonNetwork.room.maxPlayers  ) {
			netview.RPC("__StartGame",PhotonTargets.AllViaServer);
		}
		
	}

	[RPC] void __StartGame () {
		xTime.Instance.OnGameStart();
		TreeGenerator.Instance.OnGameStart();
		players.AddRange(GameObject.FindObjectsOfType(typeof(Player)) as Player[]);
		foreach(var p in players ) {
			p.OnGameStart();
		}
	}
	
	[RPC] void __EndGame () {
		Debug.LogError("GAME END!");
		TreeGenerator.Instance.OnGameEnd();
		foreach(var p in players ) {
			p.OnGameEnd();
		}
	}
	public void OnTreeStartFalling (int x, int z, int fx, int fz,bool isFromMaster ) {
		var c = grid[x,z];
		if ( c != null ) {
			if ( c.tree == null ) {
				Debug.LogError("tree cant be null, must check 22!!");
			}
			else {
				c.tree.OnRealFall(fx,fz,isFromMaster);
			}
		}
	}

	public void OnTreeVanish(int x,int z ) {
		netview.RPC("_TreeVasnish", PhotonTargets.All, new object[]{ x,z });
	}
	[RPC] void _TreeVasnish(int x, int z ) {
		var c = grid[x,z];
		if ( c != null ) {
			if ( c.tree == null ) {
				Debug.LogError("tree cant be null, must check !!");
			}
			else {
				c.tree.Vanish();
			}
		}
	}
	public void OnTreeCollide (int x, int z, int dx, int dz ,double t) {
		var p = GetPlayer(x+dx,z+dz);
		if ( p != null ) p.OnLostHp();
		if ( !PhotonNetwork.isMasterClient ) return;
		netview.RPC("_OnDamageTree", PhotonTargets.All, new object[]{ x+dx,z+dz,dx,dz,10000f,t , PhotonNetwork.isMasterClient});
	}

	public void OnTreeGrow (int x,int z, int growStage) {
		netview.RPC("_TreeGrow", PhotonTargets.All, new object[] { x,z,growStage});
	}
	[RPC] void _TreeGrow (int x, int z, int growStage ) {
		var c = grid[x,z];
		if ( c != null ) {
			if ( c.tree == null ) {
				//Debug.LogError("tree null");
				//pool.Get().AttachToCell(c,xTime.Instance.time);
			}
			else {
				c.tree.Grow(growStage);
			}
		}
	}

	#region player 
	public void OnPlayerDie () {
		netview.RPC("__EndGame",PhotonTargets.AllViaServer);
	}

	public void OnPlayerChop (int x, int z, int fx, int fz, float dmg,double time ) {
		if ( grid[x,z] == null ) return;
		netview.RPC("_OnDamageTree", PhotonTargets.All, new object[]{ x, z, fx, fz, dmg, time, PhotonNetwork.isMasterClient });
	}

	[RPC] void _OnDamageTree (int x, int z, int fx, int fz, float dmg,double time ,bool fromMaster ) {		
		var c = grid[x,z];
		if ( c == null ) return;
		if ( c.tree == null ) return;
		c.tree.OnBeingDamaged(fx,fz,dmg,time,fromMaster);
	}

	public void OnPlayerPlant(int id, int x,int z,int fx,int fz, double time, bool canFastPlant ) {
		if (grid[x,z] == null ) return;
		netview.RPC("_OnPlayerPlant",PhotonTargets.All,new object[]{id, x,z,fx,fz,time,canFastPlant});
	}

	public void OnGenTree(int tree_nb ) {
		if ( freeCells.Count == 0 ) return;
		for(int i=0; i < tree_nb ; i++ ) {
			var c = freeCells[Random.Range(0,freeCells.Count)];
			netview.RPC("_OnGenTree",PhotonTargets.All,new object[]{ c.x,c.z,xTime.Instance.time });
		}
	}
	[RPC] void _OnGenTree(int x,int z, double time ) {
		var c = grid[x,z];
		if ( c != null ) {
			if ( time < c.lock_time & c.locked != -2 ) {
				TreePool.Instance.Get().AttachToCell(c,time);
			}
		} 
	}
	[RPC] void _OnPlayerPlant(int id, int x,int z, int fx,int fz, double time, bool canFastPlant ) {
		var c = grid[x,z];
		if ( c != null ) {
			if ( (time < c.lock_time & c.locked != -2) | canFastPlant ) {
				TreePool.Instance.Get().AttachToCell(c,time);
				return;
			}
			if ( !canFastPlant ) {
				c = grid[x+fx,z+fz];
				if ( c != null ) {
					if ( time < c.lock_time & c.locked != -2  ) {
						TreePool.Instance.Get().AttachToCell(c,time);
						return;
					}
				}
			}
		} else {
			var p = GetPlayer(id);
			if ( p.netview.isMine ) p.OnPlantFailed();
		}
	}

	#endregion

	public Player GetPlayer(int id ) {
		foreach ( var p in players ) {
			if ( p.netID == id ) return p;
		}
		throw new UnityException("id = " + id + " no player found !");
	}

	public Player GetPlayer(int x, int z ) {
		var c = grid[x,z];
		if ( c == null ) return null;
		foreach ( var p in players ) {
			if ( (p.transform.position- c.position).magnitude < 0.1f ) return p;
		}
		return null;
	}
	#region hide
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

