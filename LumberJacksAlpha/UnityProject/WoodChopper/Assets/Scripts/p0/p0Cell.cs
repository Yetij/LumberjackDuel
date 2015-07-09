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

	Animator animator;

	void Awake() {
		animator = GetComponent<Animator>();
	}

	static Color regColor = Color.red;
	static Color unregColor = Color.white;

	public void HighlightGround () {
		ground.color = regColor;
	}
	
	public void UnHighlightGround () {
		ground.color = unregColor;
	}
	int _fx, _fz;
	public void RegChop (int _fx, int _fz) {
	}

	public void UnRegChop () {
	}

	public void RegPlant() {
		tree.enabled = true;
	}

	public void UnRegPlant() {
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

