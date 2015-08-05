using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class p2Player : Photon.MonoBehaviour, AbsInputListener, AbsServerObserver, AbsGuiListener {
	
	readonly Quaternion q01 = Quaternion.Euler(0,0,0);
	readonly Quaternion q0_1 = Quaternion.Euler(0,180,0);
	readonly Quaternion q10 = Quaternion.Euler(0,90,0);
	readonly Quaternion q_10 = Quaternion.Euler(0,-90,0);

	p2Map localMap;

	int moveAC;
	int plantAC;

	[HideInInspector] public p2Cell currentCell;

	int currentAC;
	int additionalPerActionCostAC;
	int additionalPerTurnCostAC;

	enum TurnState { MyTurn, Background, OpponentsTurn }
	TurnState state;

	int turn_id;

	void Start () {
		TouchInput.Instance.AddListener(this);
		p2Gui.Instance.AddListener(this);
		localMap = p2Map.Instance;

		var host = localMap[0,0];
		var client = localMap[localMap.total_x-1,0];
		if ( PhotonNetwork.isMasterClient ) {
			currentCell = photonView.isMine ? host : client;
			transform.rotation = photonView.isMine ? q01 : q0_1;
			//fz =  photonView.isMine ? 1 : -1;
			//myTurnState = photonView.isMine ? TurnState.P1: TurnState.P2;
		} else {
			currentCell = photonView.isMine ? client : host;
			transform.rotation = photonView.isMine ? q0_1 : q01;
			//fz =  photonView.isMine ? -1 : 1;
			//myTurnState = photonView.isMine ?  TurnState.P2 : TurnState.P1;
		}
		currentCell.OnPlayerMoveIn(this);

		p2Server.Instance.OnPlayerReady(this);
	}

	public void OnGameStart () {
	}

	public void OnGameEnd () {
	}

	public void OnTreeSelected (p2GuiTree t) {
		guiSelectedTree = t;
	}

	public void OnBackgroundStart ()
	{
		state = TurnState.Background;
	}

	public void OnTurnStart (int turn_nb)
	{
		if ( turn_nb % 2 == turn_id ) {
			state = TurnState.MyTurn;
		} else {
			state = TurnState.OpponentsTurn;
		}
	}

	void PreMove ( int x, int z, bool interpolate ) {
		localMap.ApplyPerActionBuffs(this, currentCell);
		var enoughAC = currentAC - (moveAC + additionalPerActionCostAC ) >= 0; 
		if ( enoughAC ) {
			photonView.RPC("MoveTo", PhotonTargets.All, x,z,interpolate);
		}
	}

	[RPC] void MoveTo ( int x, int z, bool interpolate ) {
		var cell = localMap[x,z];
		if ( !interpolate) {
			currentCell.OnPlayerMoveOut();
			cell.OnPlayerMoveIn(this);
		} else {
		}
	}

	void PrePlant ( int x, int z, byte treeType ) {
		localMap.ApplyPerActionBuffs(this,currentCell);
		var enoughAC = currentAC - (plantAC + additionalPerActionCostAC ) >= 0; 
		if ( enoughAC ) {
			photonView.RPC("Plant",PhotonTargets.All, x,z, treeType);
		}
	}

	[RPC] void Plant ( int x, int z, byte treeType ) {
		var cell = localMap[x,z];
		var t = p2TreePool.Instance.Get((p2TreeType) treeType);
		cell.AddTree(t);
	}

	void PreChop ( int x, int z ) {
		localMap.ApplyPerActionBuffs(this,currentCell);
		var enoughAC = currentAC - (moveAC + additionalPerActionCostAC ) >= 0; 
		if ( enoughAC ) {
			photonView.RPC("Chop", PhotonTargets.All, x,z );
		}
	}

	[RPC] void Chop ( int x, int z ) {
		var cell = localMap[x,z];
		cell.OnChop();
	}

	public bool IsPassable () {
		return false;
	}
	public void OnApprove ()
	{
		Debug.Log("OnApprove");
	}

	public void OnCancel ()
	{
		Debug.Log("OnCancel");
	}

	public void OnControlZoneTouchMove (Vector2 delta)
	{
		Debug.Log("OnControlZoneTouchMove");
	}

	p2Cell lastPointedCell;

	public void OnMapZoneTouchMove (Vector2 pos)
	{
		Debug.Log(pos);
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
		var cell = localMap.GetPointedCell(pos);
		if ( cell == null ) return;

		if ( cell.player == this ) return;

		if ( cell.CanChop () ) {

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
