using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class Player : MonoBehaviour
{
	bool _run=false;

	public void OnGameStart () {
		_run = true;
	}
	public void OnGameEnd() {
		_run = false;
	}
	
	Cell currentCell;
	Cell nextCell;
	
	public float predictStrength=0.2f;
	public int hp=2;
	public Parameters parameters;
	
	CellManager cell_manager;
	Animator animator;
	[HideInInspector] public PhotonView netview;
	KeyboardSettings keybind;

	
	public int netID;
	#region hide

	readonly int chopHash = Animator.StringToHash("chop");
	readonly int isMovingHash = Animator.StringToHash("isMoving");
	bool isMoving= false;

	int fx, fz;

	readonly Quaternion q01 = Quaternion.Euler(0,0,0);
	readonly Quaternion q0_1 = Quaternion.Euler(0,180,0);
	readonly Quaternion q10 = Quaternion.Euler(0,90,0);
	readonly Quaternion q_10 = Quaternion.Euler(0,-90,0);
	
	
	Quaternion nextRotation = Quaternion.identity;
	Quaternion lastRotation = Quaternion.identity;
	float _rotAngleDif;
	float _rotTimer;
	
	Quaternion Dir(int fx,int fz ) {
		if ( fx == 1 ) return q10;
		if ( fx == -1 ) return q_10;
		if ( fz == 1 ) return q01;
		if ( fz == -1 ) return q0_1;
		throw new UnityException("no shit, this cant be happening ..");
	}
	#endregion
	#region start
	void Start () {
		netview = GetComponent<PhotonView>();
		netID = netview.owner.ID;
		
		animator = GetComponent<Animator>();
		
		cell_manager = CellManager.Instance;
		var host = cell_manager.CellAt(0,0);
		var client = cell_manager.CellAt(cell_manager.gridX-1,cell_manager.gridZ-1);
		if ( PhotonNetwork.isMasterClient ) {
			currentCell = netview.isMine ? host : client;
			transform.rotation = netview.isMine ? q01 : q0_1;
			fz =  netview.isMine ? 1 : -1;
		} else {
			currentCell = netview.isMine ? client : host;
			transform.rotation = netview.isMine ? q0_1 : q01;
			fz =  netview.isMine ? -1 : 1;
		}
		_lastFz = fz;
		
		transform.position = currentCell.position;
		last_sync_rot = transform.rotation;
		if ( netview.isMine ) netview.RPC("__RegMove", PhotonTargets.AllBuffered,
		            new object[] { currentCell.x, currentCell.z , double.MinValue } );
		
		nextCell = null;
		if ( PhotonNetwork.isMasterClient ) cell_manager.OnPlayerReady();
		
		keybind = v5Const.Instance.keyboardSettings;
	}
	#endregion
	
	public bool isOnCell (int x, int z ) {
		return currentCell.x == x & currentCell.z == z;
	}

	public void OnPlantFailed () {
		canPlant = true;
	}

	[RPC] void __DoChop () {
		animator.SetTrigger(chopHash);
	}
	public void OnLostHp(){
		if ( !netview.isMine ) return;
		hp --;
		if ( hp == 0 ) cell_manager.OnPlayerDie();
	}

	public void _GUI () {
		GUILayout.Label("MY ID=" + netID,GUILayout.Width(Screen.width/4));
		GUILayout.Label("MY HP=" + hp,GUILayout.Width(Screen.width/4));
		GUILayout.Label("Chop cd=" + _chopTimer,GUILayout.Width(Screen.width/4));
		GUILayout.Label("Plant cd=" + _plantTreeTimer,GUILayout.Width(Screen.width/4));
	}

	#region updates
	Vector3 predictedPosition = Vector3.zero;
	Vector3 realPos = Vector3.zero;
	Vector3 positionAtLastPacket = Vector3.zero;
	float currentTime = 0f;
	double currentPacketTime = 0.0;
	double timeToReachGoal = 0.0;
	float _path;
	float _distance;

	void _UpdateMoveState(int dx,int dz) {
		if ( dx == 0 & dz == 0 ) return;
		fx = dx;
		fz = dz;
		if  ( fx != _lastFx | fz != _lastFz) {
			nextRotation = Dir (fx,fz);
			lastRotation = transform.rotation;
			_rotAngleDif = Mathf.Abs(nextRotation.eulerAngles.y - lastRotation.eulerAngles.y );
			if ( _rotAngleDif > 180 ) _rotAngleDif -= 180;
		}
		if ( cell_manager.CellAt(currentCell.x + dx,currentCell.z + dz) == null ) return;
		
		var next = currentCell.Get(dx,dz);
		if ( next == null ) return;
		
		var betweenCell = false;
		if ( nextCell != null ) {
			var p = transform.position;
			betweenCell = (p != currentCell.position & p != nextCell.position);
		}
		
		if (  next.locked != -1 & next.locked != netID ) {
			if( (next != nextCell & !betweenCell) | (next == nextCell) ) {
				nextCell = null;
				transform.position = currentCell.position;
			}
			return;
		} 
		
		if ( next == nextCell ) return;
		
		if ( nextCell != null & betweenCell) {
			if ( currentCell.Get(-dx,-dz) == nextCell ) {
				next = currentCell;
				currentCell = nextCell;
			} else return;
		}
		
		nextCell = next;
		_distance = (nextCell.position - currentCell.position).magnitude;
		_path = 0;
		netview.RPC("__RegMove", PhotonTargets.AllBuffered,
		            new object[] { nextCell.x, nextCell.z , xTime.Instance.time } );
	}
	
	void _UpdateMove () {
		if( nextCell != null ) {
			_path += Time.deltaTime * parameters.moveSpeed;
			transform.position = Vector3.Lerp(currentCell.position,nextCell.position, _path/_distance);
			if ( transform.position == nextCell.position ) {
				netview.RPC("__UnRegMove", PhotonTargets.AllBuffered,
				            new object[] { currentCell.x, currentCell.z } );
				currentCell = nextCell;
				nextCell = null;
			}
		}
		
		if ( transform.rotation != nextRotation ) {
			_rotTimer += Time.deltaTime * parameters.rotateAngleSpeed;
			transform.rotation = Quaternion.Lerp(lastRotation,nextRotation,_rotTimer/_rotAngleDif);
			if ( Quaternion.Angle(transform.rotation,nextRotation ) < 1f ) {
				_rotTimer = 0;
				transform.rotation = nextRotation;
				lastRotation = nextRotation;
			}
		}
	}

	[RPC] void __UnRegMove (int x,int z ) {
		var c = cell_manager.CellAt(x,z);
		if ( c.locked == netID ) c.Free();
	}
	
	[RPC] void __RegMove(int x,int z ,double time) {
		var c = CellManager.Instance.CellAt(x,z);
		if ( c.locked != -2 ) {
			if ( time < c.lock_time ) {
				c.lock_time = time;
				c.locked = netID;
				return;
			}
			else if ( time == c.lock_time ) { 
				if ( (PhotonNetwork.isMasterClient & netview.isMine) | (!PhotonNetwork.isMasterClient & !netview.isMine) ) {
					c.lock_time = time;
					c.locked = netID;
				}
			}
		}
	}

	public float chopCooldown=1;
	public float plantCooldown=1;
	
	bool isPlanting;
	bool isChopping;
	bool canChop;
	bool canPlant;
	float plantTreeCooldown = 1;
	float _plantTreeTimer,_chopTimer;
	
	void _UpdatePlant(){
		isPlanting = Input.GetKey(keybind.plant);
		if ( isPlanting & !canPlant ) {
			isPlanting = false;
		}
		if ( canPlant & isPlanting ) {
			canPlant = false;
			Plant(netID,currentCell.x + fx,currentCell.z + fz, xTime.Instance.time, CanFastPlant());
			_plantTreeTimer = plantTreeCooldown;
		} else {
			_plantTreeTimer -= Time.deltaTime;
			if ( _plantTreeTimer <= 0 ) {
				canPlant = true;
			}
		}
	}

	void _UpdateChop() {
		isChopping = Input.GetKey(keybind.chop);
		if ( isChopping & !canChop ) {
			isChopping = false;
		}
		if ( canChop & isChopping ) {
			canChop = false;
			Chop(currentCell.x + fx,currentCell.z + fz,fx,fz , parameters.dmg, xTime.Instance.time);
			_chopTimer = chopCooldown;
		} else {
			_chopTimer -= Time.deltaTime;
			if ( _chopTimer <= 0 ) {
				canChop = true;
			}
		}
	}

	float currentY, angleDif;
	Quaternion last_sync_rot;
	Quaternion sync_rot;
	//	public float rotPredictionStrength;
	void _UpdateSync(){
		currentTime += Time.deltaTime;
		transform.position = Vector3.Lerp(positionAtLastPacket, predictedPosition, (float)(currentTime / timeToReachGoal));
		
		currentY += Time.deltaTime*parameters.rotateAngleSpeed;
		transform.rotation= Quaternion.Lerp(last_sync_rot,sync_rot,currentY/angleDif);
	}
	#endregion

	void Plant(int id, int x, int z, double time ,bool canFastPlant) {
		CellManager.Instance.OnPlayerPlant(id,x,z,fx,fz,time,canFastPlant);
	}

	void Chop(int x, int z, int fx, int fz, float dmg,double time ) {
		CellManager.Instance.OnPlayerChop(x,z,fx,fz,dmg,time);
		netview.RPC("__DoChop",PhotonTargets.All);
	}

	int _lastFx, _lastFz;
	

	void Update (){
		if ( !_run  ) return;
		if ( netview.isMine ) {
			int dx = (int)Input.GetAxisRaw("Horizontal");
			int dz = dx != 0 ? 0 : (int)Input.GetAxisRaw("Vertical");
			
			_UpdateMoveState(dx,dz);
			_UpdateMove();
			
			isMoving = nextCell == null? false : true;

			_UpdatePlant();
			_UpdateChop();

			if (  fx != _lastFx | fz != _lastFz ) {
				_lastFx = fx;
				_lastFz = fz;
			}
		} else {
			_UpdateSync();
		}  
		animator.SetBool(isMovingHash,isMoving);
	}

	public bool CanFastPlant() {
		if ( nextCell == null ) return true;
		return (nextCell.position-transform.position).magnitude/_distance > 0.5f ? true : false;
	}

	#region sync
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
			if( timeToReachGoal > 1 ) timeToReachGoal = 0.3f;
			
			currentPacketTime = info.timestamp;
			
		}
	}
	#endregion
}

