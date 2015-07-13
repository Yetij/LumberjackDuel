using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class p0Cell : MonoBehaviour {
	/* -2 == tree; -1 == free */
	private int _locked;
	public int locked { 
		get {
			return _locked;
		}
		set {
			if ( value == -1 ) {
				if ( isReservered ) ground.color = treeGenReservedColor;
				else ground.color = unregColor;
				freeList.Add(this);
			} else {
				freeList.Remove(this);
				if ( value == -2 )  {
					isReservered = false;
					ground.color = unregColor;
				}
				else ground.color = regColor;
			}
			_locked = value;
		}
	}
	[HideInInspector] public int x,z;
	[HideInInspector] public Vector3 position;
	[HideInInspector] public p0Cell left,right,up,down;
	[HideInInspector] public List<p0Cell> freeList;
	public MeshRenderer tree;
	public SpriteRenderer ground;
	public Animator treeAnimator;


	static Color regColor = Color.red;
	static Color unregColor = Color.white;
	static Color treeGenReservedColor = Color.green;
	static int fallHash = Animator.StringToHash("fall");
	static int reverseHash = Animator.StringToHash("reverse");

	void Start () {
		treeAnimator.gameObject.SetActive(false);
	}

	public void HighlightGround () {

	}
	
	public void UnHighlightGround () {

	}

	int fallx, fallz;
	public void RegChop (int _fx, int _fz) {
		if ( _d == null & treeAnimator.isActiveAndEnabled ) {
			int f = 0;
			if ( _fx == 1 ) f = 2;
			else if ( _fx == -1 ) f = 4;
			if ( _fz == 1 ) f = 1;
			else if ( _fz == -1 ) f = 3;

			fallx = _fx;
			fallz = _fz;


			treeAnimator.SetInteger(fallHash,f);
			treeAnimator.transform.localScale = Vector3.one;
			StartCoroutine(_d = Disapear());
		}
	}

	public float dominoDelay = 0.4f;

	IEnumerator Disapear() {
		yield return new WaitForSeconds(dominoDelay);
		var c = Get(fallx,fallz);
		if ( c != null ) {
			c.RegChop(fallx,fallz);
			foreach ( var p in p0CellController.Instance.players ) {
				if ( p.netview.isMine & p.IsOnCell ( c.x, c.z ) ) {
					p.OnTreeFallOn();
				}
			}
		}
		yield return new WaitForSeconds(additiveDisapearDelay);
		locked = -1;
		treeAnimator.transform.rotation = Quaternion.identity;
		treeAnimator.gameObject.SetActive(false);
		_d = null;
	}

	public void UnRegChop () {
	}
	
	bool isReservered;
	public void OnGenTreeReserved () {
		ground.color = treeGenReservedColor;
		isReservered = true;
	}

	public float additiveDisapearDelay = 1.1f;
	IEnumerator _d;

	static int growHash = Animator.StringToHash("grow");

	public void PlantTree() {
		if ( locked != -1 ) {
			isReservered = false;
			return;
		}
		if ( _d == null ) { 
			locked = -2;
			treeAnimator.gameObject.SetActive(true);
			treeAnimator.SetInteger(reverseHash,0);
			treeAnimator.SetInteger(fallHash,0);
			//treeAnimator.Play(growHash);
		}
	}

	public void UnPlantTree() {
		if ( _d == null ) {
			locked = -1;
			treeAnimator.gameObject.SetActive(false);
		}
	}


	
	public void Free() {
		locked = -1;
	}
	
	public p0Cell Get(int x,int z ) {
		if ( x > 0 ) return right;
		if ( x < 0 ) return left;
		if ( z > 0 ) return up;
		if ( z < 0 ) return down;
		return this;
	}
}

