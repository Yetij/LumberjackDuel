using UnityEngine;
using System.Collections;

public class p0Cell : MonoBehaviour {
	/* -2 == tree; -1 == free */
	[HideInInspector] public int locked;
	[HideInInspector] public int x,z;
	[HideInInspector] public Vector3 position;
	[HideInInspector] public p0Cell left,right,up,down;
	public MeshRenderer tree;
	public SpriteRenderer ground;
	public Animator treeAnimator;

	static Color regColor = Color.red;
	static Color unregColor = Color.white;
	static int fallHash = Animator.StringToHash("fall");
	static int inverseHash = Animator.StringToHash("inverse");

	public void HighlightGround () {
		ground.color = regColor;
	}
	
	public void UnHighlightGround () {
		ground.color = unregColor;
	}

	public void RegChop (int _fx, int _fz) {
		int f = 0;
		if ( _fx == 1 ) f = 2;
		else if ( _fx == -1 ) f = 4;
		if ( _fz == 1 ) f = 1;
		else if ( _fz == -1 ) f = 3;

		locked = -1;
		treeAnimator.SetInteger(fallHash,f);
	}

	public void UnRegChop () {
	}

	public void RegPlant() {
		locked = -2;
		tree.enabled = true;
	}

	public void UnRegPlant() {
		locked = -1;
		tree.enabled = false;
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

