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

	public int totalWinTimes { get; private set; }

	[HideInInspector] public p2Cell currentCell;


	enum TurnState : byte { MyTurn, Background, OpponentsTurn , NetWait }
	TurnState state;

	void Awake () {
		totalWinTimes = 0;
	}
	void Start () {
		localMap = p2Map.Instance;
		globalScene = p2Scene.Instance;
		gui = p2Gui.Instance;

		ResetBasic();
		ZeroBonus();

		if ( photonView.isMine ) {
			TouchInput.Instance.AddListener(this);
			gui.AddListener(this);
			gui.myName.text = photonView.owner.name;
		} else {
			gui.opponentName.text = photonView.owner.name;
		}

		var host = localMap[0,0];
		var client = localMap[localMap.total_x-1,0];

		state = TurnState.NetWait;

		if ( PhotonNetwork.isMasterClient ) {
			turn_identity = photonView.isMine ? 0 : 1;
			currentCell = photonView.isMine ? host : client;
			transform.rotation = photonView.isMine ? q01 : q0_1;
		} else {
			turn_identity = photonView.isMine ? 1 : 0;
			currentCell = photonView.isMine ? client : host;
			transform.rotation = photonView.isMine ? q0_1 : q01;
		}
		currentCell.OnPlayerMoveIn(this);

		p2Scene.Instance.OnPlayerReady(this);
	}
	
	[HideInInspector] public p2PlayerParameters bonus;
	[HideInInspector] public p2PlayerParameters basic;
	public p2PlayerParameters predef;
	float _timer;

	public void MoveHighlight (bool on) {
		var m = currentCell.map;
		foreach( var c in m ) {
			if ( c == currentCell | c == null ) continue;
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
	public int pointsToWin = 20;
	int points = 0;
	public void OnCredit (int tree_nb ) {
		if( tree_nb < 0 ) return;
		if ( photonView.isMine ) {
			points += (int)Mathf.Pow(tree_nb+1,2);
			gui.myPoints.text = "Points: " + points;

			if ( points >= pointsToWin ) p2Scene.Instance.OnVictoryConditionsReached (photonView.owner.ID);
		}
	}

	public void SkipTurn () {
		if ( state == TurnState.MyTurn ) {
			_timer = 0;
			gui.timer.text = _timer.ToString("0.00");
			state = TurnState.NetWait;
			globalScene.OnBackgroundStart (this,1);
			
			gui.Reset();
			if ( lastPointedCell != null ) {
				lastPointedCell.SelectedOn(false);
				lastPointedCell = null;
			}
			PlantHighlight(false);
			globalScene.ActivateTrees(TreeActivateTime.AfterTurn);
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
					SkipTurn();
				}
				break;
			case TurnState.NetWait:
				break;
			case TurnState.Background:
				state = TurnState.NetWait;
				globalScene.OnBackgroundEnd();
				break;
			case TurnState.OpponentsTurn:
				break;
			}

		}
	}
	//------------------------ server messages ----------------------------------
	public void OnGameStart (int start_turn) {
		Debug.Log(photonView.owner.name + ": OnGameStart");
		_timer = 0;
		if ( photonView.isMine ) {
			_timer = 0;
			gui.timer.text = _timer.ToString("0.00");
			state = TurnState.NetWait;
			if ( start_turn % 2 == turn_identity ) globalScene.OnBackgroundStart (this,globalScene.startTreeNumber);
			gui.Reset();
			if ( lastPointedCell != null ) {
				lastPointedCell.SelectedOn(false);
				lastPointedCell = null;
			}
			PlantHighlight(false);
		}
	}
		
	public void OnGameEnd (int winner) {
		bool i_won = winner == photonView.owner.ID;
		if ( i_won ) totalWinTimes ++;
		if ( photonView.isMine ) {
			StartCoroutine(_EndGame(i_won));
			if ( i_won ) gui.opponentHp.text = "HP: 0";
		}
	}

	IEnumerator _EndGame (bool k) {
		yield return new WaitForSeconds(2f);
		gui.endGamePanel.ShowResult(k);
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

				globalScene.ActivateTrees(TreeActivateTime.BeforeTurn);

				gui.ac.text = "AC: "+(basic.actionPoints + bonus.actionPoints);
				gui.myHp.text = "HP: "+(basic.hp + bonus.hp);
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

	public p2Player Owner () {
		return this;
	}


	//----------------------- player core behaviors ------------------------------

	void PreMove ( int x, int z, bool teleport ) {
		if ( !teleport & !ValidateRange(x,z) ) return;

		globalScene.ActivateTrees(TreeActivateTime.BeforeMove);

		var acLeft = ( basic.actionPoints + bonus.actionPoints ) - (basic.moveCost + bonus.moveCost ); 
		if ( acLeft >= 0 ) {
			photonView.RPC("MoveTo", PhotonTargets.All, x,z, basic.moveCost + bonus.moveCost);
		}
	}

	[PunRPC] void MoveTo ( int x, int z, int acCost ) {

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
			gui.ac.text = "AC: "+(basic.actionPoints + bonus.actionPoints).ToString();
			globalScene.ActivateTrees(TreeActivateTime.AfterMove);
		}
	}

	bool ValidateRange ( int x, int z ) {
		return Mathf.Abs(x-currentCell.x ) < 2 & Mathf.Abs(z-currentCell.z) < 2;
	}
	bool ValidateSquareRange ( int x, int z ) {
		return Mathf.Abs(x-currentCell.x ) + Mathf.Abs(z-currentCell.z) == 1;
	}
	void PrePlant ( int x, int z, byte treeType ) {
		if ( !ValidateRange(x,z) ) return;
		globalScene.ActivateTrees(TreeActivateTime.BeforePlant);
		var cost = p2TreePool.Instance.GetTreePlantCost((TreeType) treeType);
		var acLeft = ( basic.actionPoints + bonus.actionPoints ) - ( cost+ bonus.plantCost ); 
		if ( acLeft >= 0 ) {
			photonView.RPC("Plant", PhotonTargets.All, x,z,treeType, cost + bonus.plantCost);
		}
	}

	[PunRPC] void Plant ( int x, int z, byte treeType,int acCost ) {
		
		if ( !ValidateRange(x,z) ) return;
		var cell = localMap[x,z];
		var t = p2TreePool.Instance.Get((TreeType) treeType);
		cell.OnPlayerPlantTree(t,this,0);
		if ( photonView.isMine ) {
			while ( acCost > 0 ) {
				if( bonus.actionPoints > 0 ) bonus.actionPoints--;
				else basic.actionPoints --;
				acCost --;
			}
			gui.ac.text = "AC: "+(basic.actionPoints + bonus.actionPoints).ToString();
			gui.Reset();
			globalScene.ActivateTrees(TreeActivateTime.AfterPlant);
		}
	}

	void PreChop ( int x, int z ) {
		if ( !ValidateRange(x,z) ) return;
		
		globalScene.ActivateTrees(TreeActivateTime.BeforeChop);

		var acLeft = ( basic.actionPoints + bonus.actionPoints ) - (basic.chopCost + bonus.chopCost ); 
		if ( acLeft >= 0 & !isChopBlockOn ) {
			isChopBlockOn = true;
			photonView.RPC("Chop", PhotonTargets.All, x,z, basic.chopCost + bonus.chopCost);
		}
	}

	bool isChopBlockOn= false;
	[PunRPC] void Chop ( int x, int z,int acCost ) {
		var cell = localMap[x,z];
		localMap.OnPlayerChop(this,cell,acCost);
		if ( photonView.isMine ) globalScene.ActivateTrees(TreeActivateTime.AfterChop);
	}

	public bool IsPassable () {
		return false;
	}

	public void OnChopDone (int acCost) {
		if ( photonView.isMine & acCost > 0 ) {
			while ( acCost > 0 ) {
				if( bonus.actionPoints > 0 ) bonus.actionPoints--;
				else basic.actionPoints --;
				acCost --;
			}
			gui.ac.text = "AC: "+(basic.actionPoints + bonus.actionPoints).ToString();
		}
		isChopBlockOn = false;
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
			// turn off highlight last cell
			lastPointedCell.SelectedOn(false);
			if ( lastPointedCell.tree != null ) {
				lastPointedCell.tree.OnTouchExit();
			}
			if ( cell.tree != null ) {
				cell.tree.OnTouchEnter();
			}
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
			PrePlant(cell.x,cell.z,(byte)guiSelectedTree.type);
		} else if ( cell.CanMoveTo() ) {
			PreMove(cell.x,cell.z,false);
		}
	}

	public void OnBeingChoped (p2Player chopper, int acCost) {
		if ( photonView.isMine & basic.hp > 0 ){
			chopper.OnChopDone(acCost);
			if ( bonus.hp > 0 ) bonus.hp --;
			else basic.hp --;
			gui.myHp.text = "HP: "+(basic.hp + bonus.hp).ToString();
			if ( basic.hp == 0 ) {
				globalScene.OnVictoryConditionsReached (chopper.photonView.owner.ID);
			}
		}
	}

	void ResetBasic () {
		basic.actionPoints = predef.actionPoints;
		basic.chopCost = predef.chopCost;
		basic.hp = predef.hp;
		basic.moveCost = predef.moveCost;
		basic.plantCost = predef.plantCost;
		basic.turnTime = predef.turnTime;
	}

	public void OnRematch () {
		ResetBasic();
		points = 0;
		gui.timer.color = Color.black;
		gui.timer.text = "0.00";
		gui.ac.color = Color.black;
		gui.ac.text = "AC: 0";

		gui.opponentHp.text = "HP: "+basic.hp;
		gui.myHp.text = "HP: "+basic.hp;
		
		gui.myPoints.text = "Points: 0";
		gui.opponentPoints.text = "Points: 0";
		Start();
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if ( globalScene == null ? true : (!globalScene._run | gui == null ) ) return;
		if (stream.isWriting)
		{
			stream.SendNext(_timer);
			stream.SendNext(basic.hp + bonus.hp);
			stream.SendNext(basic.actionPoints + bonus.actionPoints);
			stream.SendNext(points);
		}
		else
		{
			var _timer = (float)stream.ReceiveNext();
			if( state == TurnState.MyTurn ) gui.timer.text = _timer.ToString("0.00");
			var _hp = (int)stream.ReceiveNext();
			if ( !photonView.isMine ) {
				gui.opponentHp.text = "HP: "+_hp.ToString();
			}
			var _ac = (int)stream.ReceiveNext();
			if( state == TurnState.MyTurn ) gui.ac.text = "AC: "+_ac.ToString();
			
			var _points = (int)stream.ReceiveNext();
			if ( !photonView.isMine ) {
				 gui.opponentPoints.text = "Points: "+ _points;
			}
		}

	}
}
