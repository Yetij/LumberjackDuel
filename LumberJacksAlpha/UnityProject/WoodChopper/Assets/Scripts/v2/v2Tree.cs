using UnityEngine;
using System.Collections;

public class v2Tree : MonoBehaviour
{
	enum State { GROWING, BEING_CHOPPED, FALLING };
	State state;

	[SerializeField] float max_life_time;
	[SerializeField] float life_time;
	[SerializeField] float die_delay=0.7f;
	[SerializeField] float hp;
	Vector3 next_scale;
	Vector3 last_scale;
	float _timer;
	bool _trigger;
	int x=-1,z=-1;

	int[] rand = new int[] { -1,  1, 0 };	
	readonly int being_cut_hash = Animator.StringToHash("being_cut");
	readonly int fall_dir_hash = Animator.StringToHash("fall_dir");

	Animator animator;
	Transform trans;

	public void SetIndex(int x, int z ) {
		this.x = x;
		this.z = z;
	}

	void Awake() {
		x = -1;
		z = -1;
		trans = transform;
		animator = GetComponent<Animator>();
	}

	void OnEnable () {
		life_time = max_life_time;
		hp = 100;
		state = State.GROWING;
	}
	
	IEnumerator _Grow () {
		_timer = 0;
		yield return null;
		while(true) {
			_timer += Time.deltaTime;
			var s = Vector3.Lerp(last_scale, next_scale, _timer/1f);
			trans.localScale = s;
			if ( s == next_scale ) break;
			yield return null;
		}
		_trigger = false;
	}

	public bool isAlive () {
		return gameObject.activeInHierarchy;
	}

	public void Grow () {
		if ( state != State.GROWING ) return;
		life_time -=  Time.deltaTime;
		var p = life_time/ max_life_time;
		if ( p > 0.8f ) {
			if ( !_trigger ) {
				last_scale = trans.localScale;
				next_scale = Vector3.one * 0.45f;
				StartCoroutine(_Grow());
			}
		} else if ( p > 0.55f ) {
			if ( !_trigger ) {
				last_scale = trans.localScale;
				next_scale = Vector3.one * 0.7f;
				StartCoroutine(_Grow());
			}
		} else if ( p > 0f ) {
			if ( !_trigger ) {
				last_scale = trans.localScale;
				next_scale = Vector3.one;
				StartCoroutine(_Grow());
			}
		} else {
			/* die natually */
			int x = rand[Random.Range(0,3)];
			int z = x == 0? rand[Random.Range(0,2)]: 0;
			Fall(x,z);
		}
	}

	public void OnBeingChopped ( float chop_strength, int facing_x, int facing_z) {
		if ( state == State.FALLING ) return;
		state = State.BEING_CHOPPED;
		animator.SetBool(being_cut_hash,true);
		hp -= chop_strength;
		if ( hp <= 0 ) {
			animator.SetBool(being_cut_hash,false);
			Fall (facing_x,facing_z);
		}
	}

	public void OnNotBeingChopped() {
		state = state == State.FALLING ?State.FALLING: State.GROWING;
		animator.SetBool(being_cut_hash,false);
	}

	public void OnBeingKicked ( int facing_x, int facing_z) {
		if ( state == State.FALLING ) return;
		if ( hp <= 50 ) {
			hp = 0;
			Fall (facing_x,facing_z);
		}
	}

	public void Fall (int facing_x, int facing_z ) {
		if ( state == State.FALLING ) return;
		state = State.FALLING;
		if ( facing_x == 1 ) animator.SetInteger(fall_dir_hash,1);
		else if ( facing_x == -1 ) animator.SetInteger(fall_dir_hash,3);
		if ( facing_z == 1 ) animator.SetInteger(fall_dir_hash,0);
		else if ( facing_z == -1 ) animator.SetInteger(fall_dir_hash,2);
		v2MapManager.Instance.OnTreeFall(x,z,facing_x,facing_z);
		StartCoroutine(_DieWithDelay ());
	}
	
	IEnumerator _DieWithDelay () {
		yield return null;
		yield return new WaitForSeconds(die_delay);
		FinishDie();
	}

	public void ForceDieNoDelay () {
		StopAllCoroutines();
		FinishDie();
	}

	void FinishDie() {
		hp = 0;
		trans.localRotation = Quaternion.identity;
		trans.localScale = Vector3.one*0.1f;
		gameObject.SetActive(false);
	}
}

