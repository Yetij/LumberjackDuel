using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class p2Player : Photon.MonoBehaviour, AbsInputListener, AbsServerObserver, AbsGuiListener {
	
	readonly Quaternion q01 = Quaternion.Euler(0,0,0);
	readonly Quaternion q0_1 = Quaternion.Euler(0,180,0);
	readonly Quaternion q10 = Quaternion.Euler(0,90,0);
	readonly Quaternion q_10 = Quaternion.Euler(0,-90,0);

	p2Map localMap;
	p2Scene globalScene;

	int turn_identity;

	int moveAC;
	int plantAC;

	public int ActionPointsPerTurn = 4;

	[HideInInspector] public p2Cell currentCell;

	int currentAC;
	int additionalPerActionCostAC = 0;
	int additionalPerTurnCostAC = 0;

	enum TurnState : byte { MyTurn, Background, OpponentsTurn , NetWait }
	[SerializeField] TurnState state;


	p2PlayerParameters basic;
	p2PlayerParameters actual;

	void Start () {
		localMap = p2Map.Instance;
		globalScene = p2Scene.Instance;
		
		if ( photonView.isMine ) {
			TouchInput.Instance.AddListener(this);
			p2Gui.Instance.AddListener(this);
		}

		var host = localMap[0,0];
		var client = localMap[localMap.total_x-1,0];

		state = TurnState.Background;

		if ( PhotonNetwork.isMasterClient ) {
			turn_identity = photonView.isMine ? 0 : 1;
			currentCell = photonView.isMine ? host : client;
			transform.rotation = photonView.isMine ? q01 : q0_1;
			//fz =  photonView.isMine ? 1 : -1;
			//myTurnState = photonView.isMine ? TurnState.P1: TurnState.P2;
		} else {
			turn_identity = photonView.isMine ? 1 : 0;
			currentCell = photonView.isMine ? client : host;
			transform.rotation = photonView.isMine ? q0_1 : q01;
			//fz =  photonView.isMine ? -1 : 1;
			//myTurnState = photonView.isMine ?  TurnState.P2 : TurnState.P1;
		}
		currentCell.OnPlayerMoveIn(this);

		p2Scene.Instance.OnPlayerReady(this);
	}
	[SerializeField] p2PlayerParameters parameters;
	float _timer;

	void Update () {
		if ( photonView.isMine & globalScene._run ) {
			switch ( state ) {
			case TurnState.MyTurn :
				_timer += Time.deltaTime;
				
				if ( _timer < parameters.turnTime ) {
				} else {
					_timer = 0;
					state = TurnState.NetWait;
					p2Scene.Instance.OnBackgroundStart ();
				}
				break;
			case TurnState.NetWait:
				Debug.Log("netwait");
				break;
			case TurnState.Background:
				state = TurnState.NetWait;
				Debug.Log("OnBackgroundEnd");
				p2Scene.Instance.OnBackgroundEnd();
				break;
			case TurnState.OpponentsTurn:
				Debug.Log("OpponentsTurn");
				break;
			}

		}
	}
	//------------------------ server messages ----------------------------------
	public void OnGameStart (int start_turn) {
		state = start_turn % 2 == turn_identity ? TurnState.MyTurn : TurnState.OpponentsTurn;
	}

	public void OnGameEnd () {

	}
	
	public void OnBackgroundStart ()
	{
		Debug.Log("OnBackgroundStart");
		state = TurnState.Background;
	}

	public void OnTurnStart (int turn_nb)
	{
		state = turn_nb % 2 == turn_identity? TurnState.MyTurn : TurnState.OpponentsTurn;
		Debug.Log("OnTurnStart : " + turn_nb + " state="+ state);
	}

	//------------------------ ui messages ---------------------------------------
	public void OnTreeSelected (p2GuiTree t) {
		guiSelectedTree = t;
	}
	
	void PreMove ( int x, int z, bool interpolate ) {
		Debug.Log("PreMove");
		localMap.ApplyPerActionBuffsBefore(this);
		var enoughAC = currentAC - (moveAC + additionalPerActionCostAC ) >= 0; 
		if ( enoughAC ) {
			photonView.RPC("MoveTo", PhotonTargets.All, x,z,interpolate);
		}
	}

	[RPC] void MoveTo ( int x, int z, bool interpolate ) {
		Debug.Log("MoveTo");

		var cell = localMap[x,z];
		if ( !interpolate) {
			currentCell.OnPlayerMoveOut();
			cell.OnPlayerMoveIn(this);
		} else {
		}
	}

	void PrePlant ( int x, int z, byte treeType ) {
		Debug.Log("PrePlant");

		localMap.ApplyPerActionBuffsBefore(this);
		var enoughAC = currentAC - (plantAC + additionalPerActionCostAC ) >= 0; 
		if ( enoughAC ) {
			photonView.RPC("Plant",PhotonTargets.All, x,z, treeType);
		}
	}

	[RPC] void Plant ( int x, int z, byte treeType ) {
		Debug.Log("Plant");

		var cell = localMap[x,z];
		var t = p2TreePool.Instance.Get((p2TreeType) treeType);
		cell.AddTree(t);
	}

	void PreChop ( int x, int z ) {
		Debug.Log("PreChop");

		localMap.ApplyPerActionBuffsBefore(this);
		var enoughAC = currentAC - (moveAC + additionalPerActionCostAC ) >= 0; 
		if ( enoughAC ) {
			photonView.RPC("Chop", PhotonTargets.All, x,z );
		}
	}

	[RPC] void Chop ( int x, int z ) {
		Debug.Log("Chop");

		var cell = localMap[x,z];
		localMap.OnPlayerChop(this,cell);
	}

	public bool IsPassable () {
		return false;
	}

	//------------------------------ Input Message ------------------
	public void OnSwipeUp ()
	{
		if ( state != TurnState.MyTurn ) return;
		Debug.Log("OnApprove");
	}

	public void OnSwipeDown ()
	{
		Debug.Log("OnCancel");
	}

	public void OnControlZoneTouchMove (Vector2 delta)
	{
		if ( state != TurnState.MyTurn ) return;
		Debug.Log("OnControlZoneTouchMove");
	}

	p2Cell lastPointedCell;

	public void OnMapZoneTouchMove (Vector2 pos)
	{
		if ( state != TurnState.MyTurn ) return;
		var cell = localMap.GetPointedCell(pos);

		if ( cell == null ) {
			if ( lastPointedCell != null  ) {
				lastPointedCell.SelectedOn(false);
				lastPointedCell = null;
			}
			return;
		}
		//if ( Mathf.Abs(cell.x - currentCell.x ) > 1 | Mathf.Abs(cell.z - currentCell.z) > 1 ) return;

		if ( lastPointedCell != null & lastPointedCell != cell ) {
			// turn off highlight
			lastPointedCell.SelectedOn(false);
		}
		lastPointedCell = cell;
		// turn on highlight
		lastPointedCell.SelectedOn(true);

	}
	p2GuiTree guiSelectedTree;

	public void OnMapZoneTap (Vector2 pos)
	{
		if ( state != TurnState.MyTurn ) return;
		var cell = localMap.GetPointedCell(pos);
		if ( cell == null ) return;

		if ( cell.player == this ) return;

		if ( cell.CanChop () ) {
			PreChop(cell.x,cell.z);
		} else if ( cell.CanPlant () & guiSelectedTree != null ) {
			/******
			 * need to add tree type 
			 */
			PrePlant(cell.x,cell.z,(byte)guiSelectedTree.type);
		} else if ( cell.CanMoveTo() ) {
			PreMove(cell.x,cell.z,false);
		}
	}
}
