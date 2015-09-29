using UnityEngine;
using System.Collections.Generic;
using StaticStructure;

[RequireComponent(typeof(PhotonView))]
public class p3Scene : Photon.PunBehaviour, IControlable
{
	public float globalDominoDelay = 0.25f;
	
	public bool _run { get; private set; } 
	public int currentTurnNb { get; private set; }
	public int startTreeNumber=10;
	
	p3Map localMap;
	p3TreePool localPool;
	
	List<p3Player> _players = new List<p3Player>();
	public List<p3Player> players { 
		get {
			return _players;
		}
	}

	void Start () {
		p3Ui.Instance.ingamePanel.OnSceneLoaded();
	}

	void CalculateProbabilitiesForTreeInGenList () {
		probability = new int[treeWeightList.Length+1];
		probability[0] = 0;
		int k = 0;
		for(int i =0; i < treeWeightList.Length; i ++ ) {
			k += treeWeightList[i].weight;
			probability[i+1] = k;
		}
	}

	public void SetUp () {
		Debug.Log("p3 scene SetUp");

		_players.AddRange(GameObject.FindObjectsOfType(typeof(p3Player)) as p3Player[] );

		foreach ( var p in players ) {
			p.SetUp();
		}

		CalculateProbabilitiesForTreeInGenList();

		localMap = p3Map.Instance;
		localPool = p3TreePool.Instance;


		p3Ui.Instance.ingamePanel.OnSceneSetUpDone();
	}
	
	public void Initialize () {
		Debug.Log("p3 scene init");
		foreach ( var p in players ) {
			p.Initialize();
		}
	}

	public void Run () {
		Debug.Log("p3 scene Run");

		if ( PhotonNetwork.isMasterClient ) 
			photonView.RPC("OnGameStart",PhotonTargets.All,Random.Range(0,2));

	}

	[PunRPC] void OnGameStart (int start_turn) {
		currentTurnNb = start_turn ;
		
		foreach ( var p in players ) {
			p.Run();
			p.OnGameStart (start_turn);
		}
	}

	public void Stop () {
		Debug.Log("p3 scene Stop");
		foreach ( var p in players ) {
			p.Stop();
		}
	}

	public void ActivateTrees (TreeActivateTime time ) {
		foreach ( var t in treesInScene ) {
			if ( t.isActiveAndEnabled & t.activateTime == time ) {
				t.Activate();
			}
		}
	}
	

	
	[PunRPC] void OnGameEnd (int winner) {
		_run = false;
		foreach ( var p in players ) {
			p.OnGameEnd (winner);
		}
	}
	
	public void OnVictoryConditionsReached (int winner_id) {
		photonView.RPC("OnGameEnd",PhotonTargets.All,winner_id);
	}
	
	public void SceneGenTree(int number) {
		for(int k=0;  k < number ; k ++ ) {
			var t = RandomTree();
			var l = localMap.FreeCells();
			if ( l.Count > 0 ) { 
				var c = l[Random.Range(0,l.Count)];
				photonView.RPC("_BackGroundAddTree", PhotonTargets.All, (byte) t, c.x, c.z);
			} else {
				photonView.RPC("_BackGroundAddTree", PhotonTargets.All,  (byte) t, -1, -1);
			}
		}
	}
	public void OnBackgroundStart (p3Player invoker,int treenb) {
		SceneGenTree(treenb);
		photonView.RPC("_OnBackgroundStart", PhotonTargets.All);
	}
	
	[HideInInspector] public List<p3AbsTree> treesInScene = new List<p3AbsTree>();
	
	p3Player GetPlayer ( int owner_id ) {
		foreach( var p in players ) {
			if ( p.photonView.owner.ID == owner_id ) return p;
		}
		throw new UnityException("Invalid owner id: "+ owner_id);
	}
	
	[PunRPC] void _BackGroundAddTree (byte t , int x, int z) {
		if( localMap[x,z] != null ) {
			var tree = localPool.Get((TreeType)t);
			localMap[x,z].OnPlayerPlantTree(tree,null, 1);
		}
	}
	
	[PunRPC] void _OnBackgroundStart () {
		Debug.Log("_OnBackgroundStart");
		foreach ( var tree in treesInScene ) {
			tree.OnBackgroundUpdate (players);
		}
		foreach ( var p in players ) {
			p.OnBackgroundStart();
		}
	}
	
	public void OnBackgroundEnd () {
		photonView.RPC("_OnBackgroundEnd", PhotonTargets.MasterClient);
	}
	
	int background_end_counter = 0;
	[PunRPC] void _OnBackgroundEnd () {
		background_end_counter ++;
		if ( background_end_counter == 2 ) {
			currentTurnNb ++;
			photonView.RPC("_OnTurnStart", PhotonTargets.All,currentTurnNb);
		}
	}
	
	[PunRPC] void _OnTurnStart ( int turn_nb ) {
		background_end_counter = 0;
		currentTurnNb = turn_nb;
		foreach ( var t in treesInScene ) {
			t.OnTurnStart(turn_nb);
		}
		foreach ( var p in players ) {
			p.OnTurnStart (turn_nb);
		}
	}
	
	[SerializeField] TreeWeight[] treeWeightList;
	
	int[] probability;
	
	int turnToGen=0;
	
	TreeType RandomTree () {
		int max = 0;
		foreach ( var _v in treeWeightList ) {
			max += _v.weight;
		}
		
		int r = Random.Range(0,max);
		for(int i=0; i < probability.Length - 1; i++ ) {
			if ( r >= probability[i] & r < probability[i+1] ) return treeWeightList[i].type;
		}
		throw new UnityException("code should not reach here: r="+ r);
	}
	
	public void OnRematch () {
		Debug.Log("OnRematch");
		
		localMap.OnRematch();
		turnToGen=0;
		background_end_counter = 0;
		treesInScene.Clear();
		
		foreach ( var p in players ) {
			p.OnRematch();
		}

	}
	
	#region hide
	private static p3Scene _instance;
	public static p3Scene Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p3Scene)) as p3Scene;
			}
			return _instance;
		}
	}
	#endregion
}

