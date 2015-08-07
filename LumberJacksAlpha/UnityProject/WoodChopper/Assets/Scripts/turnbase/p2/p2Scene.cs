using UnityEngine;
using System.Collections;
using System.Collections.Generic;

delegate void _OnTurnStart (int turn_nb );
delegate void _OnBackgroundStart ();

[RequireComponent(typeof(PhotonView))]
public class p2Scene : Photon.MonoBehaviour
{
	public float intervalBetweensTurns = 0.5f;
	public string playerPrefabName = "PlayerP2";

	event _OnTurnStart onTurnStart;
	event _OnBackgroundStart onBackgroundStart;
	
	enum TurnState { Background, InTurn };
	TurnState currentState;
	
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

		probability = new int[weights.Length+1];
		probability[0] = 0;
		int k = 0;
		for(int i =0; i < weights.Length; i ++ ) {
			k += weights[i];
			probability[i+1] = k;
		}
		Debug.Log("p2Server Start ");
		if ( !PhotonNetwork.isMasterClient) photonView.RPC("SceneLoaded",PhotonTargets.MasterClient);
	}
	
	[RPC] void SceneLoaded () {
		photonView.RPC("CreatePlayers", PhotonTargets.AllViaServer);
	}
	
	[RPC] void CreatePlayers () {
		PhotonNetwork.Instantiate(playerPrefabName,new Vector3(0,0,0),Quaternion.identity,0);
	}

	List<p2Player> players;

	bool masterReady, nonMasterReady;

	public void OnPlayerReady (p2Player p) {
		players.Add(p);

		if ( !PhotonNetwork.isMasterClient &  players.Count >= PhotonNetwork.room.maxPlayers ) {
			photonView.RPC("NonMasterClientReady", PhotonTargets.MasterClient);
		}
		if ( PhotonNetwork.isMasterClient &  players.Count >= PhotonNetwork.room.maxPlayers ) {
			masterReady = true;
			if ( nonMasterReady & !_run ) {
				photonView.RPC("OnGameStart",PhotonTargets.AllViaServer,Random.Range(0,2));
			}
		}
	}

	[RPC] void NonMasterClientReady () {
		nonMasterReady = true;
		if ( masterReady & !_run ) photonView.RPC("OnGameStart",PhotonTargets.AllViaServer,Random.Range(0,2));
	}

	[RPC] void OnGameStart (int start_turn) {
		_run = true;
		currentTurnNb = start_turn ;
		foreach ( var p in players ) {
			p.OnGameStart (start_turn);
		}
	}

	[RPC] void OnGameEnd () {
		_run = false;
		foreach ( var p in players ) {
			p.OnGameEnd ();
		}
	}
	

	public void OnBackgroundStart () {
		var t = RandomTree();
		var l = localMap.FreeCells();
		if ( l.Count > 0 ) { 
			var c = l[Random.Range(0,l.Count)];
			photonView.RPC("_OnBackgroundStart", PhotonTargets.AllViaServer,(byte) t, c.x, c.z);
		} else {
			photonView.RPC("_OnBackgroundStart", PhotonTargets.AllViaServer,(byte) t, -1, -1);
		}
	}

	[RPC] void _OnBackgroundStart (byte t , int x, int z) {
		if( localMap[x,z] != null ) localMap[x,z].AddTree(localPool.Get((p2TreeType)t));
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
			photonView.RPC("_OnTurnStart", PhotonTargets.AllViaServer,currentTurnNb);
		}
	}

	[RPC] void _OnTurnStart ( int turn_nb ) {
		background_end_counter = 0;
		currentTurnNb = turn_nb;
		foreach ( var p in players ) {
			p.OnTurnStart (turn_nb);
		}
	}


	float timer;
	public float currentTurnTime = 10f;

	public AbsTree[] treeList;
	public int[] weights;
	int[] probability;

	int turnToGen=0;

	p2TreeType RandomTree () {
		if( weights.Length != treeList.Length ) throw new UnityException("weight list must have the same length as tree list");
		int max = 0;
		foreach ( var _v in weights ) {
			max += _v;
		}

		int r = Random.Range(0,max);
		for(int i=0; i < probability.Length - 1; i++ ) {
			if ( r >= probability[i] & r < probability[i+1] ) return treeList[i].TreeType();
		}
		throw new UnityException("code should not reach here: r="+ r);
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

