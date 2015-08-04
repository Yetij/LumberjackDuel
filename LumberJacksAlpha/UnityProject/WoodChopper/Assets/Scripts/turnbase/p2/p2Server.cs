using UnityEngine;
using System.Collections;
using System.Collections.Generic;

delegate void _OnTurnStart (int turn_nb );
delegate void _OnBackgroundStart ();

[RequireComponent(typeof(PhotonView))]
public class p2Server : Photon.MonoBehaviour
{
	public float intervalBetweensTurns = 0.5f;

	event _OnTurnStart onTurnStart;
	event _OnBackgroundStart onBackgroundStart;
	
	enum TurnState { Background, InTurn };
	TurnState currentState;
	
	bool _run;
	int currentTurnNb;
	
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
}

