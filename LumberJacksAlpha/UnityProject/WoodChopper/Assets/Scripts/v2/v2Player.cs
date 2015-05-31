using UnityEngine;
using System.Collections;

public class v2Player : MonoBehaviour
{

	public bool isControllable;
	public float cell_travel_time = 1;
	public float rotation_time;

	Animator animator;
	Transform trans;
	void Awake ()
	{
		animator = GetComponent<Animator>();
		trans = transform;
	}

	int current_x;
	int current_z;
	int dir_x;
	int dir_z;
		
	void Start () {
		trans.position = v2TMapManager.Instance.MoveTo(0,0);
		current_x = 0;
		current_z = 0;
	}

	Quaternion next_rot;
	Quaternion last_rot;
	Vector3 next_pos;
	Vector3 last_pos;
	bool moving;

	float _timer;

	int cutting_hash = Animator.StringToHash("cutting");
	int kicking_hash = Animator.StringToHash("kicking");
	int run_hash = Animator.StringToHash("run");

	static readonly float POS_TOLERANCE = 0.01f;
	void OnGUI () {
		GUILayout.Label("Press C to chop");
	}
	void Update ()
	{
		if ( isControllable ) {
			if  (!moving ) {
				dir_x = (int)Input.GetAxisRaw("Horizontal");
				dir_z = dir_x != 0 ? 0 : (int)Input.GetAxisRaw("Vertical");

				if ( dir_x != 0 | dir_z != 0 ) {
					moving = true;
					last_pos = trans.position;
					next_pos = v2TMapManager.Instance.MoveTo(current_x+dir_x, current_z+ dir_z);
					
					last_rot = trans.rotation;
					int r = dir_x == 0 ? 0 : dir_x*90;
					r = r == 0 ? (dir_z-1)*90 : r;
					next_rot = Quaternion.Euler(0,r,0);

					Debug.Log("dir_x="+dir_x+" dir_z"+ dir_z);

					_timer = 0;
					animator.SetTrigger(run_hash);
				}
				
				animator.SetBool(cutting_hash , Input.GetKey(KeyCode.C));
				animator.SetBool(kicking_hash , Input.GetKey(KeyCode.K));
			} else {
				_timer += Time.deltaTime;
				var t = Vector3.Lerp(last_pos, next_pos, _timer/cell_travel_time);
				if ( Mathf.Abs(t.x - next_pos.x) < POS_TOLERANCE && Mathf.Abs(t.z - next_pos.z) < POS_TOLERANCE ) {
					moving = false;
					t = next_pos;
					current_x += dir_x;
					current_z += dir_z;
				}
				trans.position = t;
				trans.rotation = Quaternion.Lerp(last_rot,next_rot,_timer/rotation_time);
			} 

		}

	}
}

