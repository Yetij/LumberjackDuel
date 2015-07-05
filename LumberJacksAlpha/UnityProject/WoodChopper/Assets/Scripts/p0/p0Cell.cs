using UnityEngine;
using System.Collections;

[System.Serializable]
public class p0Cell {
	/* -2 == tree; -1 == free */
	public int locked;
	public double lock_time;
	public int x,z;
	public Vector3 position;
	public p0Cell left,right,up,down;
	public Tree tree;
	
	public void Free() {
		locked = -1;
		tree = null;
		lock_time = double.MaxValue;
	}
	
	public p0Cell Get(int x,int z ) {
		if ( x > 0 ) return right;
		if ( x < 0 ) return left;
		if ( z > 0 ) return up;
		if ( z < 0 ) return down;
		return this;
	}
}

