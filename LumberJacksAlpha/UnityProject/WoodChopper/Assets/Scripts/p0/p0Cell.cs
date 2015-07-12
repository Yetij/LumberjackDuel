using UnityEngine;
using System.Collections;

public class p0Cell : MonoBehaviour {
	/* -2 == tree; -1 == free */
	public int locked;
	[HideInInspector] public int x,z;
	[HideInInspector] public Vector3 position;
	[HideInInspector] public p0Cell left,right,up,down;
	public MeshRenderer tree;
	public SpriteRenderer ground;
	public Animator treeAnimator;

	static Color regColor = Color.red;
	static Color unregColor = Color.white;
	static int fallHash = Animator.StringToHash("fall");
	static int reverseHash = Animator.StringToHash("reverse");

	void Start () {
		treeAnimator.gameObject.SetActive(false);
	}

	public void HighlightGround () {
		ground.color = regColor;
	}
	
	public void UnHighlightGround () {
		ground.color = unregColor;
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

	public float additiveDisapearDelay = 1.1f;
	IEnumerator _d;

	public void RegPlant() {
		if ( _d == null ) { 
			locked = -2;
			Debug.Log("RegPlant , locked = " + locked);
			treeAnimator.gameObject.SetActive(true);
			treeAnimator.SetInteger(reverseHash,0);
			treeAnimator.SetInteger(fallHash,0);
		}
	}

	public void UnRegPlant() {
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

