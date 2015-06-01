using UnityEngine;
using System.Collections;

public class v2Player : MonoBehaviour
{	
	#region caches & initialization
	v2MapManager map_manager;
	Animator animator;
	Transform trans;
	
	void Awake ()
	{
		animator = GetComponent<Animator>();
		trans = transform;
	}	
	void Start () {
		trans.position = v2MapManager.Instance.MoveTo(0,0);
		current_x = 0;
		current_z = 0;
		map_manager = v2MapManager.Instance;
		map_manager.OnPlayerMove(0,0,0,0);
	}
	#endregion
	
	#region readonly
	readonly int cutting_hash = Animator.StringToHash("cutting");
	readonly int kicking_hash = Animator.StringToHash("kicking");
	readonly int running_hash = Animator.StringToHash("running");
	#endregion
	
	#region tmp/help variables, can be ignored
	int current_x;
	int current_z;
	int facing_x=0;
	int facing_z=1;
	Quaternion next_rot;
	Quaternion last_rot;
	Vector3 next_pos;
	Vector3 last_pos;
	float _move_timer;
	float _rot_timer;
	bool _need_rot;
	bool _kickOnCoolDown;

	void OnGUI () {
		GUILayout.Label("Press C to chop, K to kick");
	}

	IEnumerator _KickCoolDown () {
		yield return new WaitForSeconds(kick_cooldown);
		_kickOnCoolDown = false;
	}
	#endregion
	
	public bool isControllable;

	bool moving;

	[SerializeField] float cell_travel_time = 1;
	[SerializeField] float rotation_time;
	[SerializeField] float chop_strength;
	[SerializeField] float kick_cooldown;

	void Update ()
	{
		if ( isControllable ) {
			if  (!moving ) {
				int dir_x = (int)Input.GetAxisRaw("Horizontal");
				int dir_z = dir_x != 0 ? 0 : (int)Input.GetAxisRaw("Vertical");

				if ( dir_x != 0 | dir_z != 0 ) {
					last_pos = trans.position;
					next_pos = v2MapManager.Instance.MoveTo(current_x+dir_x, current_z+ dir_z);
					
					last_rot = trans.rotation;
					int r = dir_x == 0 ? 0 : dir_x*90;
					r = r == 0 ? (dir_z-1)*90 : r;
					next_rot = Quaternion.Euler(0,r,0);
					_need_rot = true;

					facing_x = dir_x;
					facing_z = dir_z;


					moving = map_manager.OnPlayerMove(current_x, facing_x, current_z, facing_z);
					animator.SetBool(running_hash,moving);
				} else animator.SetBool(running_hash,false);

				bool _tmp;
				if ( _tmp = Input.GetKey(KeyCode.C)) {
					map_manager.OnPlayerChop(chop_strength*Time.deltaTime, current_x ,facing_x, current_z, facing_z);
				} else {
					map_manager.OnPlayerNotChop();
				}
				animator.SetBool(cutting_hash ,_tmp );

				if ( _tmp = Input.GetKey(KeyCode.K) & !_kickOnCoolDown ) {
					map_manager.OnPlayerKick(current_x ,facing_x, current_z, facing_z);
					_kickOnCoolDown = true;
					StartCoroutine(_KickCoolDown ());
				}
				animator.SetBool(kicking_hash , _tmp);
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
	}
}

