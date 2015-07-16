using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TurnState : byte{ P1=0, P2=1, None=2 }

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
	
	p0Grid grid;
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
		globalState = TurnState.None;
		_const = p0Const.Instance;
		grid = GetComponent<p0Grid>();

		Debug.Log("Cell controller created");
		if ( !PhotonNetwork.isMasterClient) netview.RPC("__ClientCellControllerOk",PhotonTargets.MasterClient);
	}

	/* INITIALIZATION : rpc as signals */
	#region rp
	[RPC] void __ClientCellControllerOk () {
		var grid = _const.gridSettings;
		var gameplay = _const.gameplaySettings;
		var player = _const.playerSettings;
		var tree = _const.treeSettings;
		netview.RPC("__SyncSettings",PhotonTargets.All,
		            grid.total_x, grid.total_z,
		            grid.offset_x, grid.offset_z,
		            grid.root.x,grid.root.y, grid.root.z,

		            gameplay.playerMaxHp, gameplay.treeFallDamage, gameplay.directChopDamage,
		            gameplay.pointsToWin, gameplay.actionPointsPerTurn, gameplay.timePerTurn,
		            gameplay.chopActionCost, gameplay.plantActionCost, gameplay.moveActionCost,

		            player.moveSpeed, player.rotateAngleSpeed,

		            tree.dominoDelay, tree.additiveDisapearDelay  );
	}

	[RPC] void __SyncSettings ( 
						           byte total_x, byte total_z,
						           float offset_x, float offset_z,
						           float rootx, float rooty, float rootz,

						           int playerMaxHp, int treeFallDamage, int directChopDamage,
						           int pointsToWin, int actionPointsPerTurn, float timePerTurn,
						           int chopActionCost, int plantActionCost, int moveActionCost,

						           float moveSpeed, float rotateAngleSpeed,

						           float dominoDelay , float additiveDisapearDelay  ) {


		grid.Init(total_x,total_z,offset_x,offset_z,rootx,rooty,rootz);

		var gameplay = _const.gameplaySettings;
		var player = _const.playerSettings;
		var tree = _const.treeSettings;

		gameplay.playerMaxHp = playerMaxHp;
		gameplay.treeFallDamage = treeFallDamage;
		gameplay.directChopDamage = directChopDamage;
		gameplay.pointsToWin= pointsToWin;
		gameplay.actionPointsPerTurn = actionPointsPerTurn; 
		gameplay.timePerTurn = timePerTurn;
		gameplay.chopActionCost = chopActionCost;
		gameplay.actionPointsPerTurn = actionPointsPerTurn;
		gameplay.timePerTurn = timePerTurn;
		gameplay.chopActionCost = chopActionCost;
		gameplay.plantActionCost = plantActionCost;
		gameplay.moveActionCost = moveActionCost;

		player.moveSpeed = moveSpeed;
		player.rotateAngleSpeed = rotateAngleSpeed;
		
		tree.dominoDelay = dominoDelay;
		tree.additiveDisapearDelay = additiveDisapearDelay;

		Debug.Log("Game settings sync-ed");

		if ( !PhotonNetwork.isMasterClient) netview.RPC("__ClientSyncSettingsOk",PhotonTargets.MasterClient);
	}
	
	[RPC] void __ClientSyncSettingsOk () {
		netview.RPC("__CreatePlayer",PhotonTargets.AllBufferedViaServer);
	}

	[RPC] void __CreatePlayer () {
		PhotonNetwork.Instantiate(p0Const.Instance.prefabNames._Player,new Vector3(0,0,0),Quaternion.identity,0);
		Debug.Log("Players created, waiting for game to start");
	}

	public p0Cell GetPointedCell(Vector3 point ) {
		int x = (int ) (point.x - grid.root.x / grid.offset_x);
		int z = (int ) (point.z - grid.root.z / grid.offset_z);
		return grid[x,z];
	}
	bool _run;

	public List<p0Player> players { get; private set; }	
	[RPC] void __StartGame () {
		Debug.Log("Game started");

		var l = GameObject.FindObjectsOfType(typeof(p0Player)) as p0Player[];
		players = new List<p0Player>(l);

		_run = true;
		foreach ( var p in l ) {
			p.OnGameStart();
		}

		if ( PhotonNetwork.isMasterClient) genTreeReservationList = new List<p0Cell>();

		if ( !PhotonNetwork.isMasterClient) netview.RPC("__AllStarted",PhotonTargets.MasterClient);
	}

	[RPC] void __AllStarted() {
		_run = true;
		Debug.Log("First turn begins");
		if ( PhotonNetwork.isMasterClient) {
			currentTurn = Random.Range(0,2);
			StartNewTurn();
		}
	}

	float _timer=0;
	float standardTurnTime=0;
	float currentTimeScale=0;

	static TurnState[] states = { TurnState.P1, TurnState.P2 };
	int currentTurn = 0;

	void Update () {
		if ( _run & PhotonNetwork.isMasterClient ) {
			_UpdateTurnState ();
		}
	}

	public int pointsToWin = 20;
	void _UpdateTurnState () {
		if ( _timer > standardTurnTime + 0.4f) {
			EndCurrentTurn ();
			currentTurn = ++currentTurn%2;
			StartNewTurn ();
		}
		_timer += Time.deltaTime;
	}

	public int startTreeNb=8;
	public int genTreeMin=2;
	public int genTreeMax=3;
	
	void _ReserveTree () {
		var f = grid.frees;
		if ( f.Count == 0 ) return;
		var tree_nb = Random.Range(genTreeMin,genTreeMax+1);

		for(int i=0; i < tree_nb ; i++ ) {
			var c = f[Random.Range(0,f.Count)];
			genTreeReservationList.Add (c  );
			netview.RPC("ReserveTree", PhotonTargets.All,c.x,c.z);
			if ( f.Count == 0 ) return;
		}
	}

	[RPC] void ReserveTree (int x, int z) {
		grid[x,z].OnGenTreeReserved();
	}

	List<p0Cell > genTreeReservationList;

	void _GenTree () {
		foreach ( var p in genTreeReservationList  ){
			netview.RPC("GenTree",PhotonTargets.All, p.x, p.z);
		}
		genTreeReservationList.Clear();
	}

	[RPC] void GenTree (int x, int z ) {
		grid[x,z ].PlantTree();
	}

	void EndCurrentTurn () {
		_GenTree(); 
		netview.RPC("_EndTurn",PhotonTargets.All,(byte) states[currentTurn]);
		foreach ( var p in players ) {
			if ( p.points >= pointsToWin ) {
				EndGame();
				return;
			}
		}
	}

	[RPC] void _EndTurn(byte state ) {
		var p = GetPlayer((TurnState) state);
		p.OnEndTurn();
	}

	void StartNewTurn () {
		_ReserveTree();
		netview.RPC("_StartTurn",PhotonTargets.All, (byte) states[currentTurn] );
	}

	int turn=0;
	[RPC] void _StartTurn(byte state ) {
		turn ++;
		globalState = (TurnState ) state;
		_timer = 0;
		var p = GetPlayer(globalState);
		standardTurnTime = _const.gameplaySettings.timePerTurn;
		p.OnStartTurn();
	}

	p0Player GetPlayer (TurnState state ) {
		foreach ( var p in players ) {
			if ( p.myTurnState == state ) return p;
		}
		throw new UnityException("error");
	}

	public void EndGame () {
		netview.RPC("__EndGame", PhotonTargets.All);
	}

	[RPC] void __EndGame () {
		Debug.LogError("GAME END!");
		_run = false;
		foreach ( var p in players ) {
			p.OnGameEnd();
		}
	}



	#endregion

	public TurnState globalState { get; private set; }


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

	public bool HasTree ( int x, int z ) {
		var c = grid[x,z];
		if ( c == null ) return false;
		if ( c.locked != -2 ) return false;
		return true;
	}

	public void OnPlayerRegMove (int id, int x, int z ) {
		var c = grid[x,z];
		if ( c.CanStepOn() ) {
			c.locked = id;
			if ( players != null ) GetPlayer(id).ConsumeActionPoint(_const.gameplaySettings.moveActionCost); /* first call from Start function wont take effect */
		}
	}

	public void OnPlayerChop (int id, int x ,int z , int fx, int fz, int tree_nb) {
		var c = grid[x,z];
		c.ChopTree(id,fx,fz,tree_nb);
		foreach ( var p in players ) {
			if ( p.IsOnCell(x,z) ) {
				Debug.Log("chop hit another player");
				p.OnLostHp(_const.gameplaySettings.directChopDamage);
			}
		}
	}

	public void OnPlayerUnRegMove (int x, int z ) {
		grid[x,z].locked = -1;
	}

	public void OnPlayerEndTurn (byte turn ) {
		if ( (TurnState) turn == globalState ) {
			_timer = standardTurnTime;
		}
	}

	public void OnPlayerPlant (int x, int z ) {
		if ( PhotonNetwork.isMasterClient ) {
			var c = grid[x,z];
			if ( c != null ) {
				if ( c.CanPlantTreeOn() ) {
					genTreeReservationList.Add (c  );
					netview.RPC("ReserveTree", PhotonTargets.All,c.x,c.z);
				}
			}
		}
	}

	#endregion


	#region debug gui
	public void _GUI () {
		if ( players != null ) {
			foreach ( var p in players ) {
				p._GUI();
			}
		}
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

