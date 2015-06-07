using UnityEngine;
using System.Collections;

[System.Serializable]
public class Vec2Int {
	public int x,z;
}

[System.Serializable]
class Parameters {
	public float moveSpeed;
	public float rotateSpeed;
}

[RequireComponent(typeof(PhotonView))]
public class v4Player : UnityEngine.MonoBehaviour
{
	[SerializeField] Cell currentCell = null;
	[SerializeField] Cell nextCell = null;

	public float predictStrength=0.2f;

	[SerializeField] Parameters parameters;

	PhotonView netview;
	v4GameController game_control;
	
	void Start() {
		netview = GetComponent<PhotonView>();
		game_control = v4GameController.Instance;
		Cell host = game_control.Get(0,0);
		Debug.Log("host cell =" + host.position);
		Cell client = game_control.Get(game_control.grid.total_x-1,game_control.grid.total_z-1);
		if ( PhotonNetwork.isMasterClient ) {
			currentCell = netview.isMine ? host : client;
		} else {
			currentCell = netview.isMine ? client : host;
		}
		transform.position = currentCell.position;
		nextCell = null;
	}

	[RPC] void SetChopping (bool isChopping) {
		lastChopping = isChopping;
	}
	[RPC] void SetKicking (bool isKicking) {
		lastKicking = isKicking;
	}

	Vector3 predictedPosition = Vector3.zero;
	Vector3 realPos = Vector3.zero;
	Vector3 positionAtLastPacket = Vector3.zero;
	double currentTime = 0.0;
	double currentPacketTime = 0.0;
	double timeToReachGoal = 0.0;
	bool lastChopping=false,lastKicking=false, lasMoving= false;

	float _path;
	float _distance;

	void UpdateTransform () {
		int dir_x = (int)Input.GetAxisRaw("Horizontal");
		int dir_z = dir_x != 0 ? 0 : (int)Input.GetAxisRaw("Vertical");
		int my_id = PhotonNetwork.player.ID;

		if ( dir_x != 0 | dir_z != 0 ) {
			Cell n = currentCell.Get(dir_x,dir_z);
			if( n != null ) {
				if ( n == nextCell ) {
					if ( nextCell.locked != -1 & nextCell.locked != my_id) {
						Debug.Log("Cell locked to another played");
						nextCell = currentCell;						
						currentCell = n;
					}
				} else  {

					if ( IsMovingBetweenCells() ) {
						Debug.Log("IsMovingBetweenCells");
						var m = currentCell.Get(-dir_x,-dir_z);
						if ( m != null & m == nextCell ) {
							Debug.Log("Reverse path");
							var t = nextCell;
							nextCell = currentCell;
							currentCell = t;
							_path = 0;
							_distance = (transform.position - nextCell.position).magnitude;
						}
					} else if ( n.canStepOn & n.locked == -1) {
						Debug.Log("New cell , i go there");
						nextCell = n;
						game_control.UpdateCellProp(nextCell.x,nextCell.z,false,true);
						game_control.UpdateCellProp2(nextCell.x,nextCell.z,my_id);
						_path = 0;
						_distance = (nextCell.position - currentCell.position).magnitude;
					} else {
						Debug.Log("Blocked cell , cant go there");
						//nextCell = null;
					}
				}
			}
			// rotation
		}

		if( nextCell != null ) {
			_path += Time.deltaTime * parameters.moveSpeed;
			transform.position = Vector3.Lerp(currentCell.position,nextCell.position, _path/_distance);
			if ( transform.position == nextCell.position ) {
				if ( currentCell.locked == my_id ) {
					game_control.UpdateCellProp2(currentCell.x,currentCell.z,-1);
					game_control.UpdateCellProp(currentCell.x,currentCell.z,true,true);
				}
				currentCell = nextCell;
				nextCell = null;

			}
		}
	}

	bool IsMovingBetweenCells () {
		if( nextCell == null ) return false;
		var pos = transform.position;
		if ( pos != currentCell.position & pos != nextCell.position ) return true;
		return false;
	}

	void Update ()
	{
		if (!netview.isMine)
		{
			currentTime += Time.deltaTime;
			transform.position = Vector3.Lerp(positionAtLastPacket, predictedPosition, (float)(currentTime / timeToReachGoal));
		} else {
			UpdateTransform ();

			bool isChopping = Input.GetKey(KeyCode.C);
			bool isKicking = Input.GetKey(KeyCode.K);

			if ( isChopping != lastChopping ) {
				netview.RPC("SetChopping",
				            PhotonTargets.AllBufferedViaServer,
				            new object[] { isChopping }
				);
			}
			if ( isKicking != lastKicking ) {
				netview.RPC("SetKicking",
				            PhotonTargets.AllBufferedViaServer,
				            new object[] { isKicking }
				);
			}
		}
	}

	void UpdateRotation () {
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext((Vector3)transform.position);
		}
		else
		{
			currentTime = 0.0;
			positionAtLastPacket = transform.position;
			var _pos = (Vector3)stream.ReceiveNext();
			var dir = (_pos - realPos);
			realPos = _pos;

			predictedPosition = realPos + dir*predictStrength;
			timeToReachGoal = info.timestamp - currentPacketTime;
			currentPacketTime = info.timestamp;
		}
	}
}

