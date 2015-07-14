using UnityEngine;
using System.Collections;

[System.Serializable]
public class p0Parameters {
	public float moveSpeed;
	public float rotateAngleSpeed;
	public float dmg;
}
[RequireComponent(typeof(PhotonView))]
public class p0Player : MonoBehaviour
{
	readonly Quaternion q01 = Quaternion.Euler(0,0,0);
	readonly Quaternion q0_1 = Quaternion.Euler(0,180,0);
	readonly Quaternion q10 = Quaternion.Euler(0,90,0);
	readonly Quaternion q_10 = Quaternion.Euler(0,-90,0);

	public TurnState myTurnState { get; private set; }
	public float turnTime;
	public int actionPoints;
	int _actionPoints;

	TurnState globalTurnState;
	p0CellController cellController;

	public p0Parameters parameters;
		
	p0Const _const;

	[HideInInspector] public PhotonView netview;
	/* public events */
	#region public events
	bool _run;
	public void OnGameStart () {
		_run = true;
	}

	public void OnGameEnd () {
		_run = false;
	}

	public bool IsOnCell (int x, int z ) {
		var t = cellController.CellAt(x,z).position;
		return Mathf.Abs( t.x - transform.position.x) < 0.2f & Mathf.Abs(t.z- transform.position.z) < 0.2f;
	}

	public void OnLostHp (int i) {
		netview.RPC("LostHp", PhotonTargets.All,i);
	}
	int hp = 1;
	[RPC] void LostHp (int i) {
		Debug.Log("player = "+netview.owner.ID+ " got hit");
		hp -= i;
		if ( netview.isMine ) {
			if ( hp <= 0 ) {
				cellController.EndGame();
			}
		}
	}

	bool isMyTurn;
	float localTimer = 0;
	public void OnStartTurn () {
		isMyTurn = true;
		selectCellPlantMode = false;
		selectCellChopMode = false;
		localTimer = turnTime;
		_actionPoints = actionPoints;
	}

	public void _GUI () {
		if ( netview.isMine) {
			GUILayout.Label("hp ="+ hp );
			GUILayout.Label("timer ="+ (localTimer < 0 ? "0.0" : localTimer.ToString("0.0")) );
			GUILayout.Label("action points ="+ _actionPoints );
			GUILayout.Label("points ="+ points );
			if ( isMyTurn ) {
				GUILayout.Button("Reset action (disabled)", GUILayout.Width(200));
				if ( GUILayout.Button("End turn", GUILayout.Width(200)) ) {
					isMyTurn = false;
					localTimer = 0;
					netview.RPC("ManualEndTurn", PhotonTargets.MasterClient, (byte) myTurnState);
				}
			}
		}
	}
	public void OnEndTurn () {
		isMyTurn = false;
	}

	#endregion

	void Start () {
		netview = GetComponent<PhotonView>();
		_const = p0Const.Instance;

		cellController = p0CellController.Instance;
		var host = cellController.CellAt(0,0);
		var client = cellController.CellAt(cellController.gridX-1,0);
		if ( PhotonNetwork.isMasterClient ) {
			currentCell = netview.isMine ? host : client;
			transform.rotation = netview.isMine ? q01 : q0_1;
			fz =  netview.isMine ? 1 : -1;
			myTurnState = netview.isMine ? TurnState.P1: TurnState.P2;
		} else {
			currentCell = netview.isMine ? client : host;
			transform.rotation = netview.isMine ? q0_1 : q01;
			fz =  netview.isMine ? -1 : 1;
			myTurnState = netview.isMine ?  TurnState.P2 : TurnState.P1;
		}
		fx = 0;
		nextRotation = Dir(fx,fz);

		nextCell = null;
		transform.position = currentCell.position;

		p0CellController.Instance.OnPlayerRegMove(netview.owner.ID,currentCell.x, currentCell.z);

		if ( netview.isMine ) {
			netview.RPC("_PlayerReady", PhotonTargets.MasterClient );
		}
	}

	[RPC] void _PlayerReady () {
		Debug.Log("Player ready");
		p0CellController.Instance.OnPlayerReady();
	}

	bool isMoving, isPlanting, isChopping;
	int fx,fz;
	p0Cell currentCell,nextCell;

	#region hide
	
	float _path;
	float _distance;

	Quaternion Dir(int fx,int fz ) {
		if ( fx == 1 ) return q10;
		if ( fx == -1 ) return q_10;
		if ( fz == 1 ) return q01;
		if ( fz == -1 ) return q0_1;
		throw new UnityException("no shit, this cant be happening ..");
	}

	float _rotPath;
	float _rotDistance;
	Quaternion nextRotation,lastRotation;

	void _UpdateMoveState () {
		var _fx = (int)Input.GetAxisRaw("Horizontal");
		var _fz = _fx == 0 ? (int) Input.GetAxisRaw("Vertical"): 0;
		isMoving = (_fx + _fz != 0 );
		if ( isMoving ) {
			var c = cellController.CellAt(currentCell.x + _fx, currentCell.z + _fz );
			if ( c != null ) {
				if ( c.locked == -1 ) { 
					nextCell = c;
					
					_path = 0;
					_distance = (nextCell.position - transform.position).magnitude;
					
					if  ( fx != _fx | fz != _fz) {
						nextRotation = Dir (_fx,_fz);
						lastRotation = transform.rotation;
						if ( reverseMode ) {
							var q = lastRotation.eulerAngles;
							lastRotation = Quaternion.Euler(new Vector3(q.x,q.y-180,q.z));
						} 

						_rotPath = 0;
						_rotDistance = Mathf.Abs(nextRotation.eulerAngles.y - lastRotation.eulerAngles.y );
						if ( _rotDistance > 180 ) _rotDistance -= 180;
					}
					
					fx = _fx;
					fz = _fz;
				} else isMoving = false;
			} else isMoving = false;
		}
		if ( isMoving ) {
			netview.RPC("RegMove", PhotonTargets.All,netview.owner.ID,currentCell.x + _fx, currentCell.z + _fz);
		}
	}
	string GetName () {
		return myTurnState == TurnState.P1? "Player1 " : "Player2 ";
	}
	[RPC] void RegMove(int id, int x, int z ) {
		cellController.OnPlayerRegMove(id,x,z);
	}

	[RPC] void UnRegMove(int x, int z ) {
		cellController.OnPlayerUnRegMove(x,z);
	}		                            
	bool reverseMode;
	void _UpdateMove () {
		if( nextCell != null ) {
			_path += Time.deltaTime * parameters.moveSpeed;
			transform.position = Vector3.Lerp(currentCell.position,nextCell.position, _path/_distance);
			if ( transform.position == nextCell.position ) {		
				netview.RPC("UnRegMove", PhotonTargets.All,currentCell.x , currentCell.z );
				currentCell = nextCell;
				nextCell = null;
			}
		}
		
		if ( transform.rotation != nextRotation ) {
			_rotPath += Time.deltaTime * parameters.rotateAngleSpeed;
			transform.rotation = Quaternion.Lerp(lastRotation,nextRotation,_rotPath/_rotDistance);
			if ( Quaternion.Angle(transform.rotation,nextRotation ) < 1f ) {
				transform.rotation = nextRotation;
				lastRotation = nextRotation;
			}
		}

		if ( nextCell == null & transform.rotation == nextRotation ) {
			isMoving = false;
		}
	}
	public void ConsumeActionPoint(int i) {
		_actionPoints -= i;
	}

	[RPC] void ManualEndTurn (byte playerTurn) {
		cellController.OnPlayerEndTurn(playerTurn);
	}
	float _lastChop = 0;
	float _lastPlant = 0;

	#endregion
	bool selectCellPlantMode;
	bool selectCellChopMode;
	void Update () {
		if ( netview.isMine & _run ) {
			if ( isMoving ) {
				_UpdateMove();
			}
			if ( !isMyTurn  | _actionPoints <= 0 ) {
				return;
			}

			if ( !isMoving & !selectCellPlantMode & !selectCellChopMode) {
				_UpdateMoveState();
			}

			isPlanting = Input.GetKey(_const.keyboardSettings.plant) & !isMoving & !selectCellPlantMode & !selectCellChopMode;
			isChopping = Input.GetKey(_const.keyboardSettings.chop)  & !isMoving & !selectCellPlantMode & !selectCellChopMode;

			if( isPlanting & Time.time - _lastPlant > 0.5f) {
				_lastPlant = Time.time;
				selectCellPlantMode = true;
			} 

			if ( selectCellPlantMode ) {
				if ( Input.GetButtonDown("Fire1") ) {
					var pointed_cell = cellController.GetPointedCell (Camera.main.ScreenToWorldPoint(Input.mousePosition));

					if( pointed_cell != null ) {
						if ( Mathf.Abs( pointed_cell.x - currentCell.x ) < 2 & Mathf.Abs( pointed_cell.z - currentCell.z ) < 2 ) {
							var tree_x = pointed_cell.x;
							var tree_z = pointed_cell.z;
							if ( cellController.FreeCell(tree_x,tree_z ) ) {
								netview.RPC("DoPlant", PhotonTargets.All, tree_x,tree_z);
								selectCellPlantMode = false;
							} else {
								//Debug.Log("!!! pointed cell is not free");
							}
						} else {
							//Debug.Log("!!! pointed cell is too far from player");
						}
					} else {
						//Debug.Log("!!! pointed cell == null");
					}
				}
			}

			if( isChopping & Time.time - _lastChop > 0.4f ) {
				_lastChop = Time.time;
				selectCellChopMode = true;
			} 

			if ( selectCellChopMode ) {
				if ( Input.GetButtonDown("Fire1") ) {
					var pointed_cell = cellController.GetPointedCell (Camera.main.ScreenToWorldPoint(Input.mousePosition));
					
					if( pointed_cell != null ) {
						if ( Mathf.Abs( pointed_cell.x - currentCell.x ) < 2 & Mathf.Abs( pointed_cell.z - currentCell.z ) < 2 ) {
							var tree_fx = pointed_cell.x - currentCell.x;
							var tree_fz = pointed_cell.z - currentCell.z;
							var tree_x = pointed_cell.x;
							var tree_z = pointed_cell.z;
							if ( cellController.HasTree (tree_x, tree_z )) {
								netview.RPC("DoChop", PhotonTargets.All, netview.owner.ID, tree_x,tree_z,tree_fx,tree_fz);
								selectCellChopMode = false;
							} else {
								Debug.Log("!!! pointed cell is not free");
							}
						} else {
							Debug.Log("!!! pointed cell is too far from player");
						}
					} else {
						Debug.Log("!!! pointed cell == null");
					}
				}
			}

			localTimer -= Time.deltaTime;
		} else {
			_UpdateSync();
		}
	}
	public int points = 0;
	public void CreditPoints (int p ) {
		points += p;
	}

	[RPC] void DoChop (int id, int x ,int z , int fx, int fz) {
		cellController.OnPlayerChop(id, x,z,fx,fz,1);
		ConsumeActionPoint(1);
	}

	[RPC] void DoPlant (int x ,int z) {
		cellController.OnPlayerPlant(x,z);
		ConsumeActionPoint(1);
	}

	#region sync
	public float predictStrength = 0.2f;
	Vector3 predictedPosition = Vector3.zero;
	Vector3 realPos = Vector3.zero;
	Vector3 positionAtLastPacket = Vector3.zero;
	float currentTime = 0f;
	double currentPacketTime = 0.0;
	double timeToReachGoal = -1f;

	float currentY, angleDif=-1f;
	Quaternion last_sync_rot;
	Quaternion sync_rot;

	void _UpdateSync(){
		currentTime += Time.deltaTime;
		if( currentTime <= timeToReachGoal) transform.position = Vector3.Lerp(positionAtLastPacket, predictedPosition, (float)(currentTime / timeToReachGoal));
		
		currentY += Time.deltaTime*parameters.rotateAngleSpeed;
		if ( currentY <= angleDif ) transform.rotation= Quaternion.Lerp(last_sync_rot,sync_rot,currentY/angleDif);
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext(transform.position);
			stream.SendNext(isMoving);
			stream.SendNext(transform.rotation.eulerAngles.y);
		}
		else
		{
			var _pos = (Vector3)stream.ReceiveNext();
			isMoving = (bool )stream.ReceiveNext();
			var _y = (float ) stream.ReceiveNext();
			
			currentY = 0;
			last_sync_rot = transform.rotation;
			//	var _dy = Mathf.Abs(last_sync_rot.eulerAngles.y - _y);
			angleDif = Mathf.Abs(_y /* + _dy*rotPredictionStrength*/ - last_sync_rot.eulerAngles.y );
			if ( angleDif > 180 ) angleDif -= 180;
			sync_rot = Quaternion.Euler(new Vector3(0,_y  /*+ _dy*rotPredictionStrength*/,0));
			
			currentTime = 0;
			positionAtLastPacket = transform.position;
			var dp = (_pos - realPos);
			realPos = _pos;
			
			predictedPosition = realPos + dp*predictStrength;
			timeToReachGoal = info.timestamp - currentPacketTime;
			if( timeToReachGoal > 1 ) timeToReachGoal = 0.2f;
			
			currentPacketTime = info.timestamp;
			
		}
	}
	#endregion
}

