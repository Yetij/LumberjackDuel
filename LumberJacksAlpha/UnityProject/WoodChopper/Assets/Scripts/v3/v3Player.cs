using UnityEngine;
using System.Collections;

public struct Vec2Int {		
	public int x, z;
	public Vec2Int(int _x, int _z) {	x = _x;	z = _z;	} 
}

[RequireComponent(typeof(NetworkView))]
public class v3Player : MonoBehaviour
{	
	#region caches & initialization
	v3MapManager map_manager;
	Animator animator;
	Transform trans;
	NetworkView netview;

	void Awake ()
	{
		animator = GetComponent<Animator>();
		trans = transform;
		netview = GetComponent<NetworkView>();
		netview.observed = this;
		netview.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
		v3Refs.Instance.players.Add(this);
	}	

	public void Initialize () {
		map_manager = v3MapManager.Instance;
		if ( Network.isClient ) {
			current_x =  netview.isMine ? map_manager.total_tree_x-1 : 0;
			current_z = netview.isMine? map_manager.total_tree_z-1 : 0;
			facing_x=0;
			facing_z= netview.isMine? -1:  1;
			trans.position = map_manager.MoveTo(current_x,current_z);
			trans.eulerAngles = netview.isMine ? new Vector3(0,180,0): new Vector3(0,0,0);
		}
		if ( Network.isServer ) {
			current_x = netview.isMine ? 0 : map_manager.total_tree_x-1;
			current_z = netview.isMine? 0 : map_manager.total_tree_z-1;
			facing_x=0;
			facing_z=netview.isMine? 1 : -1;
			trans.position = map_manager.MoveTo(current_x,current_z);
			trans.eulerAngles = netview.isMine ? new Vector3(0,0,0): new Vector3(0,180,0);
		}
	}
	
	#endregion
	
	#region readonly
	readonly int cutting_hash = Animator.StringToHash("cutting");
	readonly int kicking_hash = Animator.StringToHash("kicking");
	readonly int running_hash = Animator.StringToHash("running");
	#endregion
	
	#region tmp/help variables, can be ignored
	int current_x,current_z,facing_x=0,facing_z=1;
	Quaternion next_rot,last_rot;
	Vector3 next_pos,last_pos;
	float _move_timer,_rot_timer;
	bool _need_rot,_kickOnCoolDown;
	
	IEnumerator _KickCoolDown () {
		yield return new WaitForSeconds(kick_cooldown);
		_kickOnCoolDown = false;
	}

	#endregion

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		stream.Serialize(ref kicking);
		stream.Serialize(ref chopping);
	}

	bool moving,chopping, kicking;

	public Vec2Int[] GetReservatedCells () {
		int x2 = current_x + facing_x;
		int z2 = current_z + facing_z;
		if ( map_manager.IsIndexValid(x2,z2) & x2 != current_x & z2 != current_z ) 
			return new Vec2Int[] { new Vec2Int(current_x,current_z), new Vec2Int(x2,z2) };
		return new Vec2Int[] { new Vec2Int(current_x,current_z) };
	}
	
	[SerializeField] float cell_travel_time = 1;
	[SerializeField] float rotation_time;
	[SerializeField] float chop_strength;
	[SerializeField] float kick_cooldown;


	[RPC] void _NetMove (int dir_x, int dir_z )  {
		facing_x = dir_x;
		facing_z = dir_z;
		
		last_pos = trans.position;
		next_pos = v3MapManager.Instance.MoveTo(current_x+facing_x, current_z+ facing_z);
		moving = map_manager.OnPlayerMove(current_x, facing_x, current_z, facing_z);
		
		last_rot = trans.rotation;
		int r = facing_x == 0 ? 0 : facing_x*90;
		r = r == 0 ? (facing_z-1)*90 : r;
		next_rot = Quaternion.Euler(0,r,0);
		_need_rot = true;
	}

	public void ManualUpdate ()
	{
		if ( netview.isMine ) {
			if  (!moving ) {
				int dir_x = (int)Input.GetAxisRaw("Horizontal");
				int dir_z = dir_x != 0 ? 0 : (int)Input.GetAxisRaw("Vertical");
				
				if ( dir_x != 0 | dir_z != 0 ) {
					netview.RPC("_NetMove", RPCMode.AllBuffered,dir_x, dir_z);
				} 
				chopping = Input.GetKey(KeyCode.C);
				kicking = Input.GetKey(KeyCode.K);
			} else {
				_move_timer += Time.deltaTime;
				var t = Vector3.Lerp(last_pos, next_pos, _move_timer/cell_travel_time);
				trans.position = t;
				if ( t == next_pos ) {
					moving = false;
					current_x += facing_x;
					current_z += facing_z;
					_move_timer = 0;
				}
			} 
			if ( _need_rot ) {
				_rot_timer += Time.deltaTime;
				var r = Quaternion.Lerp(last_rot,next_rot,_rot_timer/rotation_time);
				trans.rotation = r;
				if ( r == next_rot ) {
					_need_rot = false;
					_rot_timer = 0;
				}
			}
		}

		if ( moving) {
			_move_timer += Time.deltaTime;
			var t = Vector3.Lerp(last_pos, next_pos, _move_timer/cell_travel_time);
			trans.position = t;
			if ( t == next_pos ) {
				moving = false;
				current_x += facing_x;
				current_z += facing_z;
				_move_timer = 0;
			}
		} 
		if ( _need_rot ) {
			_rot_timer += Time.deltaTime;
			var r = Quaternion.Lerp(last_rot,next_rot,_rot_timer/rotation_time);
			trans.rotation = r;
			if ( r == next_rot ) {
				_need_rot = false;
				_rot_timer = 0;
			}
		}
		if ( kicking & !_kickOnCoolDown ) {
			map_manager.OnPlayerKick(current_x ,facing_x, current_z, facing_z);
			_kickOnCoolDown = true;
			StartCoroutine(_KickCoolDown ());
		}

		if ( chopping ) {
			map_manager.OnPlayerChop(chop_strength*Time.deltaTime, current_x ,facing_x, current_z, facing_z);
		} else {
			map_manager.OnPlayerNotChop();
		}

		animator.SetBool(running_hash, moving);
		animator.SetBool(kicking_hash, kicking);
		animator.SetBool(cutting_hash, chopping );
	}
}

