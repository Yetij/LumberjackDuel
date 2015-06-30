using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class DmgEntry {
	public int dx,dz;
	public float dmg;
	public bool fromMaster;
}

public class Tree : MonoBehaviour
{
	float _hp;

	bool isFalling;
	
	Animator animator;
	
	readonly int isBeingChoppedHash = Animator.StringToHash("isBeingChopped");
	readonly int fallingHash = Animator.StringToHash("falling");
	readonly int directionChangedHash = Animator.StringToHash("directionChanged");
	
	public void Reset() {
		fallRoutine= null;
		stopGrow = false;
		localGrowthStage =0;
		isFalling = false;
		dmgRecord = new SortedList<double, DmgEntry>();
		_hp = v5Const.Instance.treeGeneralSettings.maxHp[localGrowthStage];
	}
	
	bool stopGrow = false;

	void Awake() {
		animator = GetComponent<Animator>();
	}

	#region being damaged 
	SortedList<double, DmgEntry > dmgRecord;

	public void OnBeingDamaged (int _dx,int _dz, float _dmg, double t, bool _fromMaster) {
		if( isFalling ) {
			return;
		}
		stopGrow = true;
		DmgEntry test = null;
		dmgRecord.TryGetValue(t,out test);
		if ( test == null ) {
			dmgRecord.Add(t,new DmgEntry(){dx=_dx, dz=_dz, dmg=_dmg, fromMaster=_fromMaster});
		}else {
			var p = CellManager.Instance.GetPlayer(cell.x-_dx,cell.z - _dz);
			if ( p != null ) {
				if ( (p.netview.isMine & !PhotonNetwork.isMasterClient) | (!p.netview.isMine & PhotonNetwork.isMasterClient)){
					test.dmg = _dmg;
					test.dx = _dx;
					test.dz = _dz;
					test.fromMaster = _fromMaster;
				}
			}
		}
		RecalculateDmg();
	}
	#endregion

	#region fall
	double fall_time;
	int fall_dx, fall_dz;
	bool fromMaster;

	void RecalculateDmg () {
		_hp = v5Const.Instance.treeGeneralSettings.maxHp[localGrowthStage];
		foreach ( var p in dmgRecord ) {
			_hp -= p.Value.dmg;		
			if ( _hp <= 0 ) {
				if ( fallRoutine == null ) {
					StartCoroutine(fallRoutine = FallWaitForSync());
				}
				fall_dx= p.Value.dx;
				fall_dz= p.Value.dz;
				fromMaster = p.Value.fromMaster;
				break;
			}
		}
	}

	IEnumerator fallRoutine; /* need reset */

	[SerializeField] float fallDelay = 0.1f; /* should be small enough so that player could not notice */
	[SerializeField] float estimatedNetDelay = 0.06f; /* net delay */
	[SerializeField] float additiveDominoDelay = 0.25f; /* after starting to fall */
	[SerializeField] float additiveCompletelyDisapearDelay = 0.25f; /* after domino */

	IEnumerator FallWaitForSync () {
		yield return new WaitForSeconds(fallDelay); /* after this time, no more changes will be made */
		isFalling = true;
		CellManager.Instance.OnTreeStartFalling(cell.x,cell.z, fall_dx,fall_dz,PhotonNetwork.isMasterClient);
	}
	public void OnRealFall(int fall_dx, int fall_dy, bool fromMaster) {
		StartCoroutine(RealFall(fall_dx, fall_dy, fromMaster));
	}
	IEnumerator RealFall (int fall_dx, int fall_dy, bool fromMaster) {
		if ( (PhotonNetwork.isMasterClient & fromMaster) | (!PhotonNetwork.isMasterClient & !fromMaster) ) {
			Debug.Log("Tree Real Fall, i chop, i wait");
			yield return new WaitForSeconds(estimatedNetDelay);
		}
		if ( fall_dx == 1 ) animator.SetInteger(fallingHash,1);
		else if ( fall_dx == -1 ) animator.SetInteger(fallingHash,3);
		if ( fall_dz == 1 ) animator.SetInteger(fallingHash,0);
		else if ( fall_dz == -1 ) animator.SetInteger(fallingHash,2);
		
		yield return new WaitForSeconds(additiveDominoDelay);
		if ( PhotonNetwork.isMasterClient ) CellManager.Instance.OnTreeCollide(cell.x, cell.z,fall_dx,fall_dz, xTime.Instance.time);
		
		yield return new WaitForSeconds(additiveCompletelyDisapearDelay);
		if ( PhotonNetwork.isMasterClient ) CellManager.Instance.OnTreeVanish(cell.x, cell.z);
	}

	public void Vanish () {
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one*0.1f;
		DeAttachFromCell();
		gameObject.SetActive(false);
	}

	#endregion

	readonly static int[] rand = { -1,  1, 0 };	

	int localGrowthStage= 0;
	IEnumerator treeGrowRoutine;

	IEnumerator TreeGrow () {
		yield return null;
		while ( true ) {
			CellManager.Instance.OnTreeGrow(cell.x,cell.z,localGrowthStage);
			yield return new WaitForSeconds(v5Const.Instance.treeGeneralSettings.additiveLifeTimeStage[localGrowthStage]);
			if ( stopGrow ) yield break;
			if ( localGrowthStage + 1 == v5Const.Instance.treeGeneralSettings.additiveLifeTimeStage.Length ) break;
			localGrowthStage++;
		}
		if ( PhotonNetwork.isMasterClient ) {
			int fx = rand[Random.Range(0,3)];
			int fz = fx == 0? rand[Random.Range(0,2)]: 0;
			OnBeingDamaged(fx,fz,10000f,xTime.Instance.time,PhotonNetwork.isMasterClient);
		}
	}

	public void Grow (int _growthStage) {
		localGrowthStage = _growthStage;
		_hp = v5Const.Instance.treeGeneralSettings.maxHp[localGrowthStage];
		StartCoroutine(_Grow(transform.localScale,Vector3.one * v5Const.Instance.treeGeneralSettings.sizeScale[_growthStage]));
	}

	#region help/tmp variables, functions	
	Cell cell;
	
	public void AttachToCell (Cell cell, double t ) {
		this.cell = cell;
		if ( cell != null ) transform.position = cell.position;
		
		cell.tree = this;
		cell.locked = -2;
		cell.lock_time = t;
		CellManager.Instance.freeCells.Remove(cell);
		if ( PhotonNetwork.isMasterClient ) StartCoroutine(treeGrowRoutine = TreeGrow());
	}
	
	void DeAttachFromCell () {
		cell.tree = null;
		cell.locked = -1;
		cell.lock_time = double.MaxValue;
		
		CellManager.Instance.freeCells.Add(cell);
		cell = null;
	}

	IEnumerator _Grow (Vector3 from, Vector3 to) {
		float _timer = 0;
		yield return null;
		while(true) {
			_timer += Time.deltaTime;
			var s = Vector3.Lerp(from, to, _timer/1f);
			transform.localScale = s;
			if ( s == to ) yield break;
			yield return null;
		}
	}
	#endregion
	
}
