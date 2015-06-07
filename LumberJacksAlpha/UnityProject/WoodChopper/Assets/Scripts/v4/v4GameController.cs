using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Grid {
	public int total_x, total_z;
	public float offset_x, offset_z;
	public Vector3 root;

	Vector3 Locate ( int x, int z ) {
		return new Vector3(x*offset_x + offset_x/2 + root.x, root.y, z * offset_z + offset_z/2 + root.z);
	}
	public Cell[,] GenCells () {
		Debug.Log("GenCell total_x="+total_x+" total_z"+ total_z);
		Cell[,] r = new Cell[total_x,total_z];
		for(int _z = 0; _z < total_z; _z ++ ) {
			for ( int _x = 0; _x < total_x ; _x ++ ) {
				Cell c = new Cell();
				c.x = _x;
				c.z = _z;
				c.position = Locate(_x,_z);
				c.canStepOn = true;
				c.canGrowTree = true;
				c.locked = -1;
				if ( _x == 0 & _z == 0 ) Debug.Log("0-0 = " + c.position);
				r[_x,_z] = c;
			}
		}
		for(int _z = 0; _z < total_z; _z ++ ) {
			for ( int _x = 0; _x < total_x ; _x ++ ) {
				var c = r[_x,_z];
				c.up = _z + 1 < total_z? r[_x,_z+1] : null;
				c.down = _z - 1 >= 0 ? r[_x,_z-1] : null;
				c.right = _x + 1 < total_x ? r[_x+1,_z] : null;
				c.left = _x - 1 >= 0 ? r[_x-1, _z] : null;
			}
		}
		return r;
	}
}

[System.Serializable]
public class Cell {
	public int locked;
	public bool canStepOn,canGrowTree;
	public int x,z;
	public Vector3 position;
	public Cell left,right,up,down;
	public Cell Get(int x,int z ) {
		if ( x > 0 ) return right;
		if ( x < 0 ) return left;
		if ( z > 0 ) return up;
		if ( z < 0 ) return down;
		return this;
	}
}

public class RefSystem {
	Dictionary<string, MonoBehaviour > refs;
	public RefSystem (int cap ) { 
		refs = new Dictionary<string, MonoBehaviour >(cap);
	}
	public RefSystem () : this(4) {}

	public void Add (string s, MonoBehaviour mono ){
		refs.Add(s,mono);
	}
	public T GetRef<T> (string s ) where T: MonoBehaviour {
		MonoBehaviour m = null;
		refs.TryGetValue(s,out m);
		return (T)m;
	}
}

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

	public RefSystem refs;
	public Grid grid;

	Cell[,] cells;
	PhotonView netview;

	public Cell Get(int x,int z ) {
		return cells[x,z];
	}

	void Awake () {
		var l = GameObject.FindObjectsOfType(typeof(v4GameController));
		if ( l.Length > 1 ) {
			Destroy(l[l.Length -1]);
			return;
		}
		refs = new RefSystem();
		netview = GetComponent<PhotonView>();
	}

	void Start () {
		cells = grid.GenCells();
		if ( PhotonNetwork.isMasterClient) netview.RPC("OnGameControllerInstantiated",PhotonTargets.AllBufferedViaServer);
	}

	[RPC] void OnGameControllerInstantiated () {
		Debug.Log((PhotonNetwork.isMasterClient? "Master here !! : " : "Slave here !! : ")+" OnGameControllerInstantiated"); 
		PhotonNetwork.Instantiate(v4ConstValue.Instance.prefabNames._Player,new Vector3(0,0,0),Quaternion.identity,0);
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

