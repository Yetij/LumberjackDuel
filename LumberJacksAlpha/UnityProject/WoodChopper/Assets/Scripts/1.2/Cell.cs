using UnityEngine;
using System.Collections;

[System.Serializable]
public class Cell {
	/* -2 == tree; -1 == free */
	public int locked;
	public double lock_time;
	public int x,z;
	public Vector3 position;
	public Cell left,right,up,down;
	public Tree tree;
	
	public void Free() {
		locked = -1;
		tree = null;
		lock_time = double.MaxValue;
	}

	public Cell Get(int x,int z ) {
		if ( x > 0 ) return right;
		if ( x < 0 ) return left;
		if ( z > 0 ) return up;
		if ( z < 0 ) return down;
		return this;
	}
}

