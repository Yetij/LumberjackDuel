using UnityEngine;
using System.Collections;
using System.Collections.Generic;

delegate void _OnTurnStart (int turn_nb );
delegate void _OnBackgroundStart ();

[RequireComponent(typeof(PhotonView))]
public class p2Server : Photon.MonoBehaviour
{
	public float intervalBetweensTurns = 0.5f;
	public string playerPrefabName = "PlayerP2";

	event _OnTurnStart onTurnStart;
	event _OnBackgroundStart onBackgroundStart;
	
	enum TurnState { Background, InTurn };
	TurnState currentState;
	
	bool _run;
	int currentTurnNb;

	void Start () {
		players = new List<p2Player>();
		Debug.Log("p2Server Start ");
		if ( !PhotonNetwork.isMasterClient) photonView.RPC("ServerLoaded",PhotonTargets.MasterClient);
	}
	
	[RPC] void ServerLoaded () {
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
				photonView.RPC("OnGameStart",PhotonTargets.AllViaServer);
			}
		}
	}

	[RPC] void NonMasterClientReady () {
		nonMasterReady = true;
		if ( masterReady & !_run ) photonView.RPC("OnGameStart",PhotonTargets.AllViaServer);
	}

	[RPC] void OnGameStart () {
		_run = true;
		foreach ( var p in players ) {
			p.OnGameStart ();
		}
	}

	[RPC] void OnGameEnd () {
		_run = false;
		foreach ( var p in players ) {
			p.OnGameEnd ();
		}
	}

	void Update () {
		if ( _run & PhotonNetwork.isMasterClient & photonView.isMine) {
			UpdateState ();
		} 
	}
	
	public void AddObserver ( AbsServerObserver listener ) {
		onTurnStart += listener.OnTurnStart;
		onBackgroundStart += listener.OnBackgroundStart;
	}

	public void RemoveObserver ( AbsServerObserver listener) {
		onTurnStart -= listener.OnTurnStart;
		onBackgroundStart -= listener.OnBackgroundStart;
	}

	[RPC] void OnTurnStart ( int turn_nb ) {
		if ( onTurnStart != null ) onTurnStart(turn_nb);
	}
	
	[RPC] void OnBackgroundStart ( ) {
		if ( onBackgroundStart != null ) onBackgroundStart();
	}
	
	float timer;
	float currentTurnTime;

	void UpdateState () {
		timer += Time.deltaTime;
		
		switch ( currentState ) {
		case TurnState.Background : 
			if ( timer > intervalBetweensTurns ) {
				currentState = TurnState.InTurn;
				currentTurnNb ++;
				photonView.RPC("OnTurnStart" , PhotonTargets.All,currentTurnNb);
			}
			break;
		case TurnState.InTurn:
			if ( timer > currentTurnTime ) {
				currentState = TurnState.Background;
				photonView.RPC("OnBackgroundStart" , PhotonTargets.All);
			}
			break;
		}
	}

	#region hide
	private static p2Server _instance;
	public static p2Server Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p2Server)) as p2Server;
			}
			return _instance;
		}
	}
	#endregion
}

