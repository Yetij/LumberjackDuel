using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class p1Cell : MonoBehaviour {
	[HideInInspector] public int x,z;
	[HideInInspector] public Vector3 position;

	public p1Tree tree;
	public p1Player player;

	p1Const _const;
	
	void Start () {
		_const = p1Const.Instance;
	}
	
	public void OnTurnStart (int turn_nb) {
	}

	public void OnTurnEnd () {
	}

	public bool CanStep () {
		return ( player == null ? true: player.IsPassable() ) & ( tree == null ? true: tree.IsPassable() );
	}

	public bool CanPlant () {
		return player == null & tree == null;
	}

	public bool CanChop () {
		return player != null | ( tree == null ? false: !tree.IsPassable() );
	}

	public void AddTree (p1Tree t) {
		tree = t;

		t.transform.position = position;
		t.cell = this;
	}

	public void Highlight ( Color t ) {
		
	}

	public void OffHighlight () {
	}

	public p1Cell[,] map { get; private set; }
	
	/* x,z must be -1,0,1 or else ArrayOutOfBounds Exception */
	public void Map (int x, int z, p1Cell p ) {
		if( map == null ) {
			map = new p1Cell[3,3];
			map[1,1] = this;
		}
		map[x+1,z+1] = p;
	}
	
	public p1Cell Get(int x,int z ) {
		return map[x+1,z+1];
	}
}
