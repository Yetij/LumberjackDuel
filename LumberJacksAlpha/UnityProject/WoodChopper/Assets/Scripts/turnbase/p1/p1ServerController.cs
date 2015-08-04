using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class p1ServerController : MonoBehaviour
{
	public float intervalBetweensTurns = 0.5f;

	enum TurnState { Background, Phase };
	TurnState currentState;

	bool _run;

	float timer;
	float currentTurnTime;

	PhotonView netview;

	void Awake () {
		netview = GetComponent<PhotonView>();
	}

	void Update () {
		if ( _run ) {
			if ( netview.isMine ) UpdateState ();
		} else {
		}
	}

	int currentTurnNb;

	void UpdateState () {
		timer += Time.deltaTime;

		switch ( currentState ) {
		case TurnState.Background : 
			if ( timer > intervalBetweensTurns ) {
				currentState = TurnState.Phase;
				currentTurnNb ++;
				netview.RPC("OnTurnStart" , PhotonTargets.All,currentTurnNb);
			}
			break;
		case TurnState.Phase:
			if ( timer > currentTurnTime ) {
				currentState = TurnState.Background;
				netview.RPC("OnTurnEnd" , PhotonTargets.All);
			}
			break;
		}
	}
}

