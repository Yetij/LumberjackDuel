using UnityEngine;
using System.Collections;

[System.Serializable]
public class Vec2Int {
	public int x,z;
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
	Animator animator;
	
	bool gameStarted;

	
	Vector3 predictedPosition = Vector3.zero;
	Vector3 realPos = Vector3.zero;
	Vector3 positionAtLastPacket = Vector3.zero;
	double currentTime = 0.0;
	double currentPacketTime = 0.0;
	double timeToReachGoal = 0.0;
	bool isChopping=false,isKicking=false, lasMoving= false;
	
	float _path;
	float _distance;
	int facing_x, facing_z;

	
	void Start() {
		netview = GetComponent<PhotonView>();
		animator = GetComponent<Animator>();

		game_control = v4GameController.Instance;
		Cell host = game_control.Get(0,0);
		Cell client = game_control.Get(game_control.grid.total_x-1,game_control.grid.total_z-1);
		if ( PhotonNetwork.isMasterClient ) {
			currentCell = netview.isMine ? host : client;
		} else {
			currentCell = netview.isMine ? client : host;
		}
		transform.position = currentCell.position;
		currentCell.locked = netview.owner.ID;
		game_control.UpdateCellProp(currentCell.x,currentCell.z,false,true);
		game_control.OnReady += OnGameReady;
		nextCell = null;

		game_control.netview.RPC("OnPlayerReady",PhotonTargets.AllBufferedViaServer);
	}
	
	void OnGameReady () {
		gameStarted = true;
	}

	void OnCell (int x,int z  ) {
		if ( !game_control.ValidIndex(x,z)) {
			Debug.Log("invalid index : x="+x+" z="+z);
		}

	}
	[RPC] void SetChopping (bool isChopping) {
		isChopping = isChopping;
	}
	[RPC] void SetKicking (bool isKicking) {
		isKicking = isKicking;
	}

	void UpdateTransform () {
		int dir_x = (int)Input.GetAxisRaw("Horizontal");
		int dir_z = dir_x != 0 ? 0 : (int)Input.GetAxisRaw("Vertical");
		int my_id = PhotonNetwork.player.ID;

		if ( dir_x != 0 | dir_z != 0 ) {
			if ( dir_x != facing_x & dir_z != facing_z & isChopping ) {
				//game_control.netview.RPC("SetTreeBeingChopped",PhotonTargets.AllBuffered,
				//                         new object[] { currentCell.x + dir_x, currentCell.z + dir_z, false });
			}

			facing_x = dir_x;
			facing_z = dir_z;

			Cell n = currentCell.Get(dir_x,dir_z);
			if( n != null ) {
				if ( n == nextCell ) {
					if ( nextCell.locked != -1 & nextCell.locked != my_id) {
						Debug.Log("Cell locked to another played");
						transform.position = currentCell.position;
						nextCell = null;
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
					} else {
						if ( n.canStepOn & n.locked == -1) {
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

	readonly int isChoppingHash = Animator.StringToHash("isChopping");
	readonly int isKickingHash = Animator.StringToHash("isKicking");

	public v4Tree GetBeingChoppedTree () {
		if ( !isChopping ) return null;
		int tx = currentCell.x + facing_x;
		int tz = currentCell.z + facing_z;
		if ( !game_control.ValidIndex(tx,tz) ) return null;
		Cell c = game_control.Get(tx,tz);
		return c.tree;
	}

	[RPC] void SetChopping (int tree_x, int tree_z, bool isChopping ) {
		//game_control.SetTreeBeingChopped(tree_x, tree_z, isChopping);
		//animator.SetBool(isChoppingHash, isChopping);
	}

	void Update ()
	{
		if ( !gameStarted  ) return;
		if (!netview.isMine)
		{
			currentTime += Time.deltaTime;
			transform.position = Vector3.Lerp(positionAtLastPacket, predictedPosition, (float)(currentTime / timeToReachGoal));
		} else {
			UpdateTransform ();

			bool _isChopping = Input.GetKey(KeyCode.C);
			bool _isKicking = Input.GetKey(KeyCode.K);

			if ( _isChopping != isChopping ) {
				netview.RPC("SetChopping",
				            PhotonTargets.AllBufferedViaServer,
				            new object[] { currentCell.x + facing_x, currentCell.z + facing_z, _isChopping }
				);
				isChopping = _isChopping;
			}
			if ( _isKicking != isKicking ) {
				netview.RPC("SetKicking",
				            PhotonTargets.AllBufferedViaServer,
				            new object[] { currentCell.x + facing_x, currentCell.z + facing_z, _isKicking }
				);
				isKicking = _isKicking;
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
			if( timeToReachGoal > 1 ) timeToReachGoal = 0.3f;
			currentPacketTime = info.timestamp;
		}
	}
}

