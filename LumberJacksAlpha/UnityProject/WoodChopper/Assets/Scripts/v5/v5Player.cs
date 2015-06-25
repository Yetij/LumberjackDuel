using UnityEngine;
using System.Collections;

[System.Serializable]
public class Parameters {
	public float moveSpeed;
	public float rotateAngleSpeed;
	public float dmgPerSec;
}

[RequireComponent(typeof(PhotonView))]
public class v5Player : MonoBehaviour
{
	v5Cell currentCell;
	v5Cell nextCell;
	
	public float predictStrength=0.2f;
	
	public Parameters parameters;

	v5GameController game_control;
	Animator animator;
	PhotonView netview;

	bool gameStarted = false;

	#region hide
	Vector3 predictedPosition = Vector3.zero;
	Vector3 realPos = Vector3.zero;
	Vector3 positionAtLastPacket = Vector3.zero;
	float currentTime = 0f;
	double currentPacketTime = 0.0;
	double timeToReachGoal = 0.0;
	float _path;
	float _distance;
	
	readonly int isChoppingHash = Animator.StringToHash("isChopping");
	readonly int isMovingHash = Animator.StringToHash("isMoving");
	readonly int isKickingHash = Animator.StringToHash("isKicking");
	#endregion

	bool isChopping=false,isKicking=false, isMoving= false;

	[HideInInspector] public int fx, fz;
	public int netID;

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
	void Start () {
		netview = GetComponent<PhotonView>();
		netID = netview.owner.ID;

		animator = GetComponent<Animator>();

		game_control = v5GameController.Instance;
		var host = game_control.Get(0,0);
		var client = game_control.Get(game_control.grid.total_x-1,game_control.grid.total_z-1);
		if ( PhotonNetwork.isMasterClient ) {
			currentCell = netview.isMine ? host : client;
			transform.rotation = netview.isMine ? q01 : q0_1;
			fz = netview.isMine ? 1 : -1;
		} else {
			currentCell = netview.isMine ? client : host;
			transform.rotation = netview.isMine ? q0_1 : q01;
			fz = netview.isMine ? -1 : 1;
		}
		_lastFz = fz;

		transform.position = currentCell.position;

		netview.RPC("__RegMove", PhotonTargets.AllBuffered,
		            new object[] { currentCell.x, currentCell.z , xTime.Instance.time } );

		nextCell = null;
		game_control.OnPlayerReady();
	}

	public void OnGameStart () {
		gameStarted = true;
	}

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
		if ( !game_control.ValidIndex(currentCell.x + dx,currentCell.z + dz) ) return;

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
		var c = game_control.Get(x,z);
		if ( c.locked == netID ) c.Free();
	}

	[RPC] void __RegMove(int x,int z ,double time) {
		var c = v5GameController.Instance.Get(x,z);
		if ( c.locked != -2 ) {
			if ( time < c.lock_time ) {
				c.lock_time = time;
				c.locked = netID;
				v5GameController.Instance.RegMove(netID, x, z);
				return;
			}
			else if ( time == c.lock_time ) { 
				/**/													
				/* ONLY WORKS IF THERE IS ONE HOST - ONE CLIENT */
				/**/
				int localPlayer_netid = PhotonNetwork.player.ID;
				if ( PhotonNetwork.isMasterClient ) {
					if  (netID == localPlayer_netid ) {
						/* host machine - host player */
						return;
					} else {
						/* host machine - client player */
						c.lock_time = time;
						c.locked = netID;
						v5GameController.Instance.RegMove(netID, x, z);
					}
				} else {
					if  (netID == localPlayer_netid ) {
						/* client machine - client player */
						c.lock_time = time;
						c.locked = netID;
						v5GameController.Instance.RegMove(netID, x, z);
					} else {
						/* client machine - host player */
						return;
					}
				}
			}
		}
	}

	bool _lastIsChopping, _lastIsKicking;
	int _lastFx, _lastFz;

	void Update (){
		if ( !gameStarted  ) return;
		if ( netview.isMine ) {
			int dx = (int)Input.GetAxisRaw("Horizontal");
			int dz = dx != 0 ? 0 : (int)Input.GetAxisRaw("Vertical");

			_UpdateMoveState(dx,dz);
			_UpdateMove();

			isChopping = Input.GetKey(KeyCode.C);
			isKicking = Input.GetKey(KeyCode.K);
			isMoving = nextCell == null? false : true;

			if ( _lastIsChopping != isChopping ) {
				game_control.SetTreeBeingCut(currentCell.x +fx, currentCell.z + fz,isChopping,netID);
				_lastIsChopping = isChopping;
			}
			if (  fx != _lastFx | fz != _lastFz ) {
				game_control.SetTreeBeingCut(currentCell.x + _lastFx, currentCell.z + _lastFz,false,netID);
				if ( isChopping) game_control.SetTreeBeingCut(currentCell.x + fx, currentCell.z + fz,true,netID);
				_lastFx = fx;
				_lastFz = fz;
			}

			if ( Input.GetKey(KeyCode.Z)) {
				var  l = game_control.GetLastReg(netID);
				Debug.Log("mine=" + netID + " cell locked=" + game_control.Get(l.x,l.z).locked + " time="+game_control.Get(l.x,l.z).lock_time );			
			}
		} else {
			currentTime += Time.deltaTime;
			transform.position = Vector3.Lerp(positionAtLastPacket, predictedPosition, (float)(currentTime / timeToReachGoal));

			currentY += Time.deltaTime*parameters.rotateAngleSpeed;
			transform.rotation= Quaternion.Lerp(last_sync_rot,sync_rot,currentY/angleDif);
		}  
		animator.SetBool(isChoppingHash,isChopping);
		animator.SetBool(isKickingHash,isKicking);
		animator.SetBool(isMovingHash,isMoving);
	}
	float currentY, angleDif;
	Quaternion last_sync_rot;
	Quaternion sync_rot;
//	public float rotPredictionStrength;

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext(transform.position);
			stream.SendNext(isChopping);
			stream.SendNext(isKicking);
			stream.SendNext(isMoving);
			stream.SendNext(transform.rotation.eulerAngles.y);
		}
		else
		{
			var _pos = (Vector3)stream.ReceiveNext();
			isChopping = (bool )stream.ReceiveNext();
			isKicking = (bool )stream.ReceiveNext();
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
}

