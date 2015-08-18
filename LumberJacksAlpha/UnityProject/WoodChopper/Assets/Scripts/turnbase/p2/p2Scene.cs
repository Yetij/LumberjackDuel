using UnityEngine;
using System.Collections.Generic;
using StaticStructure;

delegate void _OnTurnStart (int turn_nb );
delegate void _OnBackgroundStart ();

[RequireComponent(typeof(PhotonView))]
public class p2Scene : Photon.MonoBehaviour
{
	public string playerPrefabName = "PlayerP2";
	public float globalDominoDelay = 0.25f;

	public bool _run { get; private set; } 
	public int currentTurnNb { get; private set; }
	p2Map localMap;
	p2TreePool localPool;

	void Start () {
		players = new List<p2Player>();
		localMap = p2Map.Instance;
		localPool = p2TreePool.Instance;

		GameObject g = new GameObject("Touch Input2", typeof(TouchInput));
		g.GetComponent<TouchInput>().minSwipeDistance = 20;

		probability = new int[treeWeightList.Length+1];
		probability[0] = 0;
		int k = 0;
		for(int i =0; i < treeWeightList.Length; i ++ ) {
			k += treeWeightList[i].weight;
			probability[i+1] = k;
		}
		if ( !PhotonNetwork.isMasterClient) photonView.RPC("SceneLoaded",PhotonTargets.MasterClient);
	}
	
	[RPC] void SceneLoaded () {
		photonView.RPC("CreatePlayers", PhotonTargets.All);
	}
	
	[RPC] void CreatePlayers () {
		PhotonNetwork.Instantiate(playerPrefabName,new Vector3(0,0,0),Quaternion.identity,0);
	}

	public List<p2Player> players { get; private set; }

	bool masterReady, nonMasterReady;

	int player_verf_count = 0;
	public void OnPlayerReady (p2Player p) {
		if ( !players.Contains(p) ) players.Add(p);

		player_verf_count ++;
		Debug.Log("OnPlayerReady : " + player_verf_count + " PhotonNetwork.room.maxPlayers");

		if ( !PhotonNetwork.isMasterClient &  player_verf_count >= PhotonNetwork.room.maxPlayers ) {
			Debug.Log("!PhotonNetwork.isMasterClient  ");
			photonView.RPC("NonMasterClientReady", PhotonTargets.MasterClient);
		}

		if ( PhotonNetwork.isMasterClient &  player_verf_count >= PhotonNetwork.room.maxPlayers ) {
			Debug.Log("PhotonNetwork.isMasterClient ");
			masterReady = true;
			if ( nonMasterReady & !_run ) {
				Debug.Log("OnGameStart ");
				photonView.RPC("OnGameStart",PhotonTargets.All,Random.Range(0,2));
			}
		}
	}


	[RPC] void NonMasterClientReady () {
		nonMasterReady = true;
		if ( masterReady & !_run ) photonView.RPC("OnGameStart",PhotonTargets.All,Random.Range(0,2));
	}

	[RPC] void OnGameStart (int start_turn) {
		_run = true;
		currentTurnNb = start_turn ;
		foreach ( var p in players ) {
			p.OnGameStart (start_turn);
		}
	}

	[RPC] void OnGameEnd (int winner) {
		_run = false;
		foreach ( var p in players ) {
			p.OnGameEnd (winner);
		}
	}
	
	public void OnVictoryConditionsReached (int winner_id) {
		photonView.RPC("OnGameEnd",PhotonTargets.All,winner_id);
	}

	public void OnBackgroundStart (p2Player invoker) {
		var t = RandomTree();
		var l = localMap.FreeCells();
		if ( l.Count > 0 ) { 
			var c = l[Random.Range(0,l.Count)];
			photonView.RPC("_OnBackgroundStart", PhotonTargets.All, (byte) t, c.x, c.z);
		} else {
			photonView.RPC("_OnBackgroundStart", PhotonTargets.All,  (byte) t, -1, -1);
		}
	}

	[HideInInspector] public List<AbsTree> treesInScene = new List<AbsTree>();

	p2Player GetPlayer ( int owner_id ) {
		foreach( var p in players ) {
			if ( p.photonView.owner.ID == owner_id ) return p;
		}
		throw new UnityException("Invalid owner id: "+ owner_id);
	}
	[RPC] void _OnBackgroundStart (byte t , int x, int z) {
		if( localMap[x,z] != null ) {
			var tree = localPool.Get((TreeType)t);
			localMap[x,z].AddTree(tree,null, 0);
		}
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
	[RPC] void _OnBackgroundEnd () {
		background_end_counter ++;
		if ( background_end_counter == 2 ) {
			currentTurnNb ++;
			photonView.RPC("_OnTurnStart", PhotonTargets.All,currentTurnNb);
		}
	}

	[RPC] void _OnTurnStart ( int turn_nb ) {
		background_end_counter = 0;
		currentTurnNb = turn_nb;
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
		localMap.OnRematch();
		player_verf_count = 0;
		turnToGen=0;
		masterReady = false;
		nonMasterReady = false;
		background_end_counter = 0;
		treesInScene.Clear();

		foreach ( var p in players ) {
			p.OnRematch();
		}
	}

	#region hide
	private static p2Scene _instance;
	public static p2Scene Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p2Scene)) as p2Scene;
			}
			return _instance;
		}
	}
	#endregion
}

