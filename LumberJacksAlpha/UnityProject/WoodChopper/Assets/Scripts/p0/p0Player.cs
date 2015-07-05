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
	TurnState globalTurnState;
	p0CellController cellController;

	public p0Parameters parameters;
		
	p0Const _const;

	public PhotonView netview;
	/* public events */
	#region public events
	public void OnGameStart () {
	}

	#endregion

	#region internal functions
	void Start () {
		myTurnState = PhotonNetwork.isMasterClient? TurnState.P1: TurnState.P2;
		netview = GetComponent<PhotonView>();
		_const = p0Const.Instance;

		cellController = p0CellController.Instance;
		var host = cellController.CellAt(0,0);
		var client = cellController.CellAt(cellController.gridX-1,cellController.gridZ-1);
		if ( PhotonNetwork.isMasterClient ) {
			currentCell = netview.isMine ? host : client;
			transform.rotation = netview.isMine ? q01 : q0_1;
			fz =  netview.isMine ? 1 : -1;
		} else {
			currentCell = netview.isMine ? client : host;
			transform.rotation = netview.isMine ? q0_1 : q01;
			fz =  netview.isMine ? -1 : 1;
		}

		nextCell = null;
		transform.position = currentCell.position;
	}

	bool isMoving, isPlanting, isChopping;
	int fx,fz;

	p0Cell currentCell,nextCell;
	
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
		var _fz = fx == 0 ? (int) Input.GetAxisRaw("Vertical"): 0;
		isMoving = (_fx * _fz != 0 );
		if ( isMoving ) {
			var c = cellController.CellAt(currentCell.x + _fx, currentCell.z + _fz );
			if ( c != null ) {
				nextCell = c;
				
				_path = 0;
				_distance = (nextCell.position - currentCell.position).magnitude;
				
				if  ( fx != _fx | fz != _fz) {
					nextRotation = Dir (_fx,_fz);
					lastRotation =   transform.rotation;
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
		}
	}
	bool reverseMode;
	void _UpdateMove () {
		if( nextCell != null ) {
			_path += Time.deltaTime * parameters.moveSpeed;
			transform.position = Vector3.Lerp(currentCell.position,nextCell.position, _path/_distance);
			if ( transform.position == nextCell.position ) {
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

		isMoving = ( nextCell == null & lastRotation == nextRotation );
	}


	void Update () {
		if ( netview.isMine & cellController.globalState == myTurnState) {
			if ( !isMoving ) {
				_UpdateMoveState();
			} else {
				_UpdateMove();
			}

			isPlanting = Input.GetKey(_const.keyboardSettings.plant) & isMoving;
			isChopping = Input.GetKey(_const.keyboardSettings.chop)  & isMoving;

		} else {
			_UpdateSync();
		}
	}
	#endregion

	public float predictStrength = 0.2f;
	#region sync
	Vector3 predictedPosition = Vector3.zero;
	Vector3 realPos = Vector3.zero;
	Vector3 positionAtLastPacket = Vector3.zero;
	float currentTime = 0f;
	double currentPacketTime = 0.0;
	double timeToReachGoal = 0.0;

	float currentY, angleDif;
	Quaternion last_sync_rot;
	Quaternion sync_rot;

	void _UpdateSync(){
		currentTime += Time.deltaTime;
		transform.position = Vector3.Lerp(positionAtLastPacket, predictedPosition, (float)(currentTime / timeToReachGoal));
		
		currentY += Time.deltaTime*parameters.rotateAngleSpeed;
		transform.rotation= Quaternion.Lerp(last_sync_rot,sync_rot,currentY/angleDif);
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
			if( timeToReachGoal > 1 ) timeToReachGoal = 0.3f;
			
			currentPacketTime = info.timestamp;
			
		}
	}
	#endregion
}

