using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class v5Tree : MonoBehaviour
{
	public List<v5Player> playerChopping;
	[SerializeField] float die_delay=0.7f;
	[SerializeField] float max_life_time;
	float life_time;
	float hp;

	int _updateTrigger = 0;
	int _playGrowAnimationTrigger = 0;
	bool isFalling;
	int _growProcess;
	
	Animator animator;

	readonly int isBeingChoppedHash = Animator.StringToHash("isBeingChopped");
	readonly int falling = Animator.StringToHash("falling");

	public void Reset() {
		isFalling = false;
		_growProcess = 0;

		life_time = max_life_time;
		hp = 100;
		if ( playerChopping == null ) playerChopping = new List<v5Player>();
		playerChopping.Clear();

		_updateTrigger = 0;
		_playGrowAnimationTrigger = 0;
	}

	
	void Awake() {
		animator = GetComponent<Animator>();
	}

	public void SetBeingCut (bool s,int id) {
		if ( s ) {
			playerChopping.Add(v5GameController.Instance.GetPlayer(id));
		} else {
			playerChopping.Remove(v5GameController.Instance.GetPlayer(id));
		}
		bool k = (playerChopping.Count == 0 & !s ) ? false: true;
		animator.SetBool(isBeingChoppedHash, k);
	}

	public void SyncGrowProcess (int p ) {
		_growProcess = p;
	}



	int GrowProcess ( float procent ) {
		if ( procent > 0.8f ) return 0;
		if ( procent > 0.55f ) return 1;
		if ( procent > 0f ) return 2;
		return 3;
	}

	void Update () {
		if ( isFalling ) return;
		if ( playerChopping.Count > 0 ) {
			foreach ( var p in playerChopping ) {
				hp -= p.parameters.dmgPerSec*Time.deltaTime;
				if ( hp <= 0 ) {
					//Debug.Log("tree chopped down, falling with dir="+p.fx+","+p.fz);
					v5GameController.Instance.OnTreeFall(cell.x,cell.z,p.fx,p.fz);
					break;
				}
			}
			return;
		}
		if( PhotonNetwork.isMasterClient ) {
			life_time -=  Time.deltaTime;
			_growProcess = GrowProcess( life_time/ max_life_time);
		} 
		switch ( _growProcess  ) {
			case 0: {
				if ( _playGrowAnimationTrigger == 0 ) {
					_last_scale = transform.localScale;
					_next_scale = Vector3.one * 0.45f;
					_playGrowAnimationTrigger++;
					StartCoroutine(_Grow());
				}
				if ( _updateTrigger == 0 & PhotonNetwork.isMasterClient ) {
					v5GameController.Instance.SyncTree(cell.x,cell.z,_growProcess);
					_updateTrigger ++;
				}
				break;
			} 
			case 1: {
				if ( _playGrowAnimationTrigger == 1 ) {
					_last_scale = transform.localScale;
					_next_scale = Vector3.one * 0.7f;
					_playGrowAnimationTrigger ++;
					StartCoroutine(_Grow());
				}
				if ( _updateTrigger == 1 & PhotonNetwork.isMasterClient ) {
					v5GameController.Instance.SyncTree(cell.x,cell.z,_growProcess);
					_updateTrigger ++;
				}
				break;
			} 
			case 2: {
				if ( _playGrowAnimationTrigger == 2 ) {
					_last_scale = transform.localScale;
					_next_scale = Vector3.one;
					_playGrowAnimationTrigger ++;
					StartCoroutine(_Grow());
				}
				if ( _updateTrigger == 2 & PhotonNetwork.isMasterClient ) {
					v5GameController.Instance.SyncTree(cell.x,cell.z,_growProcess);
					_updateTrigger ++;
				}
				break;
			} 
			case 3: {
				if ( _updateTrigger == 3 & PhotonNetwork.isMasterClient ) {
					v5GameController.Instance.SyncTree(cell.x,cell.z,_growProcess);
					_updateTrigger ++;
					/* die natually */
					int _x = rand[Random.Range(0,3)];
					int _z = _x == 0? rand[Random.Range(0,2)]: 0;
				
					v5GameController.Instance.OnTreeFall(cell.x,cell.z,_x,_z);
				}
				break;
			}
		}
	}
	
	#region help/tmp variables, functions	
	Vector3 _next_scale;
	Vector3 _last_scale;
	float _timer;
	int[] rand = new int[] { -1,  1, 0 };	

	v5Cell cell;

	public void AttachToCell (v5Cell cell ) {
		var game_controller = v5GameController.Instance;
		if ( PhotonNetwork.isMasterClient) game_controller.OnTreeAttachToCell(cell.x,cell.z);

		this.cell = cell;
		if ( cell != null ) transform.position = cell.position;

		cell.tree = this;
		cell.locked = -2;

		game_controller.free.Remove(cell);
	}

	void DeAttachFromCell () {
		var game_controller = v5GameController.Instance;

		cell.tree = null;
		cell.locked = -1;
		cell.lock_time = double.MaxValue;

		game_controller.free.Add(cell);
		cell = null;
	}
	
	IEnumerator _Grow () {
		_timer = 0;
		yield return null;
		while(true) {
			_timer += Time.deltaTime;
			var s = Vector3.Lerp(_last_scale, _next_scale, _timer/1f);
			transform.localScale = s;
			if ( s == _next_scale ) break;
			yield return null;
		}
	}
	#endregion

	public void Fall (int dx, int dz ) {
		if ( isFalling ) return;
		isFalling = true;

		if ( dx == 1 ) animator.SetInteger(falling,1);
		else if ( dx == -1 ) animator.SetInteger(falling,3);
		if ( dz == 1 ) animator.SetInteger(falling,0);
		else if ( dz == -1 ) animator.SetInteger(falling,2);

		StartCoroutine(_DieWithDelay ());
	}
	
	IEnumerator _DieWithDelay () {
		yield return new WaitForSeconds(die_delay);
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one*0.1f;
		DeAttachFromCell();
		gameObject.SetActive(false);
	}
}

