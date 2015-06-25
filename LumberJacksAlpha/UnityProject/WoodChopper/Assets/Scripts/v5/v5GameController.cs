using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void OnReadyHandle();

[RequireComponent(typeof(PhotonView))]
public class v5GameController : MonoBehaviour
{
	private static v5GameController _instance;
	public static v5GameController Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(v5GameController)) as v5GameController;
				if ( _instance == null ) throw new UnityException("Object of type v5GameController not found");
			}
			return _instance;
		}
	}
	
	public v5Grid grid;
	public int startTreeNb=5;
	public int treeGenMin=1;
	public int treeGenMax=4;
	public float genTreeInterval=1f;
	v5Cell[,] cells;
	PhotonView netview;
	v5Const _const;
	v5TreePool pool;

	
	public List<v5Cell > free;

	public List<v5Player> players;
	
	public v5Cell Get(int x,int z ) {
		return cells[x,z];
	}
	
	public bool ValidIndex(int x,int z ) {
		return x >= 0 & x < grid.total_x & z >=0 & z < grid.total_z;
	}

	void Awake () {
		Debug.Log("Game controler Awake");
		netview = GetComponent<PhotonView>();
		cells = grid.GenCells();
		_const = v5Const.Instance;
		pool = v5TreePool.Instance;
		//pool.Initialize();

		free = new List<v5Cell>();
		foreach( v5Cell c in cells ) {
			free.Add(c);
		}

		players = new List<v5Player>(2);
		if ( PhotonNetwork.isMasterClient) 
			netview.RPC("OnGameControllerInstantiated",PhotonTargets.AllBufferedViaServer);
		if ( PhotonNetwork.isMasterClient ) StartCoroutine(_Update());
	}
	
	IEnumerator _Update () {
		yield return null;
		while ( !gameStarted ) yield return null;
		while ( true ) {
			yield return new WaitForSeconds(genTreeInterval);
			_GenTree(Random.Range(treeGenMin,treeGenMax+1));
		}

	}
	
	[RPC] void OnGameControllerInstantiated () {
		PhotonNetwork.Instantiate(v5Const.Instance.prefabNames._Player,new Vector3(0,0,0),Quaternion.identity,0);
	}
	
	
	public void OnPlayerReady () {
		if (PhotonNetwork.isMasterClient ) {
			if ( PhotonNetwork.room.playerCount == PhotonNetwork.room.maxPlayers  ) {
				netview.RPC("__MesToAll",PhotonTargets.All,new object[] { "Loading...." } );
				//_GenTree(startTreeNb);
				netview.RPC("__MesToAll",PhotonTargets.All,new object[] { "Resources are loaded" } );
				netview.RPC("__OnGameStart",PhotonTargets.AllViaServer);
			}
		}
	}

	Dictionary<int, Vec2Int> log = new Dictionary<int,Vec2Int >();

	public void RegMove (int id, int x, int z  ){
		Vec2Int k;
		if ( !log.TryGetValue(id,out k) ) {
			Vec2Int v = new Vec2Int();
			v.x = x;
			v.z = z;
			log.Add(id,v);
		} else {
			k.x = x;
			k.z = z;
		}
	}

	public Vec2Int GetLastReg (int id ) {
		Vec2Int k;
		if ( !log.TryGetValue(id,out k) ) {
			throw new UnityException("not found with id="+id);
		} else {
			return k;
		}
	}

	[RPC] void __OnGameStart(){ 
		var l = GameObject.FindObjectsOfType(typeof(v5Player)) as v5Player[];
		players.AddRange(l);
		foreach(var p in l ) {
			p.OnGameStart();
		}
		gameStarted = true;
		xTime.Instance.OnStart();
		Debug.Log("Game starts !");
	}

	[RPC] void __MesToAll(string s ) {
		Debug.Log(s);
	}

	public void OnTreeAttachToCell ( int x,int z ) {
		netview.RPC("__TreeToCell",PhotonTargets.OthersBuffered, new object[]{x,z, xTime.Instance.time});
	}	

	[RPC] void __TreeToCell(int x,int z , double t) {
		var c = cells[x,z];
		if ( c.locked == -1 ) {
			if ( c.tree != null ) {
				Debug.Log("Warrning: found a tree at "+x+","+z +" though it shouldnt be there");
			}
			else pool.Get().AttachToCell(c,t);
		} else {
			if ( t < c.lock_time ) {
				pool.Get().AttachToCell(c,t);
			}
		}
	}

	void _GenTree(int tree_nb) {
		if ( free.Count == 0 ) return;
		for(int i=0; i < tree_nb ; i++ ) {
			v5Cell c = free[Random.Range(0,free.Count)];
			netview.RPC("__TreeToCell",PhotonTargets.All, new object[]{c.x,c.z, xTime.Instance.time});
		}
	}
	public void SetTreeBeingCut (int x, int z , bool isBeingCut,int id ) {
		if ( !ValidIndex(x,z) ) return;
		netview.RPC("__TreeCut",PhotonTargets.All, new object[]{x,z,isBeingCut,id});
	}
	[RPC] void __TreeCut(int x,int z, bool s ,int id) {
		var c = cells[x,z];
		if ( c.tree != null ) {
			c.tree.SetBeingCut(s,id);
		} 
	}
	public void OnTreeFall (int x, int z, int dx, int dz ) {
		if ( !ValidIndex(x,z)) return;
		foreach ( v5Player p  in players ) {
			if ( p.isOnCell(x,z) ) {
				p.OnTreeFallOn();
			}
		}
		netview.RPC("__TreeF",PhotonTargets.All, new object[]{x,z,dx,dz, xTime.Instance.time});
	}
	public float domonoDelay=0.4f;
	[RPC] IEnumerator __TreeF(int x, int z, int dx, int dz, double t ) {
		var c = cells[x,z];
		if ( c.tree != null ) {
			c.tree.Fall(dx,dz,t);
			yield return new WaitForSeconds(domonoDelay);
			if( PhotonNetwork.isMasterClient ) OnTreeFall(x+dx,z +dz,dx,dz);
		} 
	}
	public v5Player GetPlayer(int id ) {
		foreach ( v5Player p in players ) {
			if ( p.netID == id ) return p;
		}
		throw new UnityException("id = " + id + " no player found !");
	}

	public void SyncTree ( int x, int z, int p) {
		netview.RPC("__SyncTr", PhotonTargets.Others,new object[]{x,z,p });
	}
	[RPC] void __SyncTr(int x, int z, int p) {
		if ( cells[x,z].tree != null ) cells[x,z].tree.SyncGrowProcess(p);
	}

	bool gameStarted=false;
	
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

