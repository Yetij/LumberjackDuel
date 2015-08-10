using UnityEngine;
using System.Collections;
using StaticStructure;

[RequireComponent(typeof(PhotonView))]
public class p2Player : Photon.MonoBehaviour, AbsInputListener, AbsServerObserver, AbsGuiListener {
	
	readonly Quaternion q01 = Quaternion.Euler(0,0,0);
	readonly Quaternion q0_1 = Quaternion.Euler(0,180,0);
	readonly Quaternion q10 = Quaternion.Euler(0,90,0);
	readonly Quaternion q_10 = Quaternion.Euler(0,-90,0);

	p2Map localMap;
	p2Scene globalScene;
	p2Gui gui;

	int turn_identity;

	int moveAC;
	int plantAC;

	public int ActionPointsPerTurn = 4;

	[HideInInspector] public p2Cell currentCell;


	enum TurnState : byte { MyTurn, Background, OpponentsTurn , NetWait }
	[SerializeField] TurnState state;



	void Start () {
		localMap = p2Map.Instance;
		globalScene = p2Scene.Instance;
		gui = p2Gui.Instance;
		
		if ( photonView.isMine ) {
			TouchInput.Instance.AddListener(this);
			gui.AddListener(this);
		}

		var host = localMap[0,0];
		var client = localMap[localMap.total_x-1,0];

		state = TurnState.Background;

		if ( PhotonNetwork.isMasterClient ) {
			turn_identity = photonView.isMine ? 0 : 1;
			currentCell = photonView.isMine ? host : client;
			transform.rotation = photonView.isMine ? q01 : q0_1;
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
	
	[HideInInspector] public p2PlayerParameters bonus;
	public p2PlayerParameters basic;
	float _timer;

	public void MoveHighlight (bool on) {
		p2Cell c;
		if ( (c = currentCell.Get(0,1 )) != null ) {
			if ( c.CanMoveTo() ) c.HighLightOn(on);
			else c.HighLightOn(false);
		}
		if ( (c = currentCell.Get(0,-1 )) != null ) {
			if ( c.CanMoveTo() ) c.HighLightOn(on);
			else c.HighLightOn(false);
		}
		if ( (c = currentCell.Get(1,0 )) != null ) {
			if ( c.CanMoveTo() ) c.HighLightOn(on);
			else c.HighLightOn(false);
		}
		if ( (c = currentCell.Get(-1,0 )) != null ) {
			if ( c.CanMoveTo() ) c.HighLightOn(on);
			else c.HighLightOn(false);
		}
	}
	
	public void PlantHighlight (bool on) {
		var m = currentCell.map;
		foreach( var c in m ) {
			if ( c == currentCell | c == null ) continue;
			if ( c.CanPlant() ) c.HighLightOn(on);
			else c.HighLightOn(false);
		}
	}

	void Update () {
		if ( photonView.isMine & globalScene._run ) {
			switch ( state ) {
			case TurnState.MyTurn :
				_timer += Time.deltaTime;
				
				if ( _timer < basic.turnTime ) {
					gui.timer.text = _timer.ToString("0.00");
				} else {
					_timer = 0;
					gui.timer.text = _timer.ToString("0.00");
					state = TurnState.NetWait;
					p2Scene.Instance.OnBackgroundStart (this);
					gui.Reset();
					if ( lastPointedCell != null ) {
						lastPointedCell.SelectedOn(false);
						lastPointedCell = null;
					}
					PlantHighlight(false);
					ActivateTree(TreeActivateTime.AfterTurn);
				}
				break;
			case TurnState.NetWait:
				break;
			case TurnState.Background:
				state = TurnState.NetWait;

				p2Scene.Instance.OnBackgroundEnd();
				break;
			case TurnState.OpponentsTurn:
				break;
			}

		}
	}
	//------------------------ server messages ----------------------------------
	public void OnGameStart (int start_turn) {
		if ( photonView.isMine ) {
			basic.actionPoints = ActionPointsPerTurn;
			gui.ac.text = "AC:"+ActionPointsPerTurn;
		}
		OnTurnStart (start_turn);
	}

	public void OnGameEnd () {

	}
	
	public void OnBackgroundStart ()
	{
		state = TurnState.Background;

	}

	void ZeroBonus () {
		bonus.actionPoints  = 0;
		bonus.chopCost = 0;
		bonus.moveCost = 0;
		bonus.plantCost = 0;
		bonus.hp = 0;
		bonus.turnTime = 0;
	}
	public void OnTurnStart (int turn_nb)
	{
		state = turn_nb % 2 == turn_identity? TurnState.MyTurn : TurnState.OpponentsTurn;
		if ( photonView.isMine ) {
			if ( state == TurnState.MyTurn ) { 
				gui.SetColor( Color.green);
				ZeroBonus();
				basic.actionPoints = ActionPointsPerTurn;
				bonus.actionPoints = 0;
				MoveHighlight(true);
				ActivateTree(TreeActivateTime.BeforeTurn);
				gui.hp.text = "HP:"+basic.hp.ToString();
			}
			else {
				gui.SetColor( Color.yellow);
			}
		}
	}

	//------------------------ ui messages ---------------------------------------
	public void OnTreeSelected (p2GuiTree t) {
		guiSelectedTree = t;
		if ( state != TurnState.MyTurn ) return;
		if ( t == null ) {
			PlantHighlight(false);
			MoveHighlight(true);
		}
		else {
			MoveHighlight(false);
			PlantHighlight(true);
		}
	}


	//----------------------- player core behaviors -------------------------------
	void ActivateTree ( TreeActivateTime time ) {
		var l = globalScene.treesInScene;
		foreach ( var t in l ) {
			if ( t.isActiveAndEnabled & t.activateTime == time ) {
				t.Activate();
			}
		}
	}

	void PreMove ( int x, int z, bool teleport ) {
		if ( !teleport & !ValidateRange(x,z) ) return;
		var acLeft = ( basic.actionPoints + bonus.actionPoints ) - (basic.moveCost + bonus.moveCost ); 
		if ( acLeft >= 0 ) {
			photonView.RPC("MoveTo", PhotonTargets.All, x,z, basic.moveCost + bonus.moveCost);
		}
	}

	[RPC] void MoveTo ( int x, int z, int acCost ) {
		var cell = localMap[x,z];

		if ( photonView.isMine ) MoveHighlight(false);
		currentCell.OnPlayerMoveOut();
		cell.OnPlayerMoveIn(this);
		if ( photonView.isMine ) {
			MoveHighlight(true);
			while ( acCost > 0 ) {
				if( bonus.actionPoints > 0 ) bonus.actionPoints--;
				else basic.actionPoints --;
				acCost --;
			}
			gui.ac.text = "AC:"+(basic.actionPoints + bonus.actionPoints).ToString();
		}
	}

	bool ValidateRange ( int x, int z ) {
		return Mathf.Abs(x-currentCell.x ) < 2 & Mathf.Abs(z-currentCell.z) < 2;
	}
	void PrePlant ( int x, int z, byte treeType ) {
		if ( !ValidateRange(x,z) ) return;
		var acLeft = ( basic.actionPoints + bonus.actionPoints ) - (basic.plantCost + bonus.plantCost ); 
		if ( acLeft >= 0 ) {
			photonView.RPC("Plant", PhotonTargets.All, x,z,treeType, basic.plantCost + bonus.plantCost);
		}
	}

	[RPC] void Plant ( int x, int z, byte treeType,int acCost ) {
		
		if ( !ValidateRange(x,z) ) return;
		var cell = localMap[x,z];
		var t = p2TreePool.Instance.Get((TreeType) treeType);
		cell.AddTree(t,this,0);
		if ( photonView.isMine ) {
			while ( acCost > 0 ) {
				if( bonus.actionPoints > 0 ) bonus.actionPoints--;
				else basic.actionPoints --;
				acCost --;
			}
			gui.ac.text = "AC:"+(basic.actionPoints + bonus.actionPoints).ToString();
		}
	}

	void PreChop ( int x, int z ) {
		if ( !ValidateRange(x,z) ) return;
		var acLeft = ( basic.actionPoints + bonus.actionPoints ) - (basic.chopCost + bonus.chopCost ); 
		if ( acLeft >= 0 ) {
			photonView.RPC("Chop", PhotonTargets.All, x,z, basic.chopCost + bonus.chopCost);
		}
	}

	[RPC] void Chop ( int x, int z,int acCost ) {

		var cell = localMap[x,z];
		localMap.OnPlayerChop(this,cell);
		if ( photonView.isMine ) {
			while ( acCost > 0 ) {
				if( bonus.actionPoints > 0 ) bonus.actionPoints--;
				else basic.actionPoints --;
				acCost --;
			}
			gui.ac.text = "AC:"+(basic.actionPoints + bonus.actionPoints).ToString();
		}
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

	public void OnBeingChoped () {
		if ( photonView.isMine ){
			if ( bonus.hp > 0 ) bonus.hp --;
			else basic.hp --;
			if ( basic.hp < 0 ) p2Scene.Instance.OnPlayerDie ();
		}
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if ( globalScene == null ? false : !globalScene._run ) return;
		if (stream.isWriting)
		{
			stream.SendNext(_timer);
			stream.SendNext(basic.hp + bonus.hp);
			stream.SendNext(basic.actionPoints + bonus.actionPoints);
		}
		else
		{
			var _timer = (float)stream.ReceiveNext();
		 	if( state == TurnState.MyTurn ) gui.timer.text = _timer.ToString("0.00");
			var _hp = (int)stream.ReceiveNext();
			if( state == TurnState.MyTurn ) gui.hp.text = "HP:"+_hp.ToString();
			var _ac = (int)stream.ReceiveNext();
			if( state == TurnState.MyTurn ) gui.ac.text = "AC:"+_ac.ToString();
		}

	}
}
