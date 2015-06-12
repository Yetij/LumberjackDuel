using UnityEngine;

[System.Serializable]
public class Grid {
	public int total_x, total_z;
	public float offset_x, offset_z;
	public Vector3 root;
	
	Vector3 Locate ( int x, int z ) {
		return new Vector3(x*offset_x + offset_x/2 + root.x, root.y, z * offset_z + offset_z/2 + root.z);
	}
	public Cell[,] GenCells () {
		Cell[,] r = new Cell[total_x,total_z];
		for(int _z = 0; _z < total_z; _z ++ ) {
			for ( int _x = 0; _x < total_x ; _x ++ ) {
				Cell c = new Cell();
				c.x = _x;
				c.z = _z;
				c.position = Locate(_x,_z);
				c.canStepOn = true;
				c.canGrowTree = true;
				c.locked = -1;
				c.tree = null;
				r[_x,_z] = c;
			}
		}
		for(int _z = 0; _z < total_z; _z ++ ) {
			for ( int _x = 0; _x < total_x ; _x ++ ) {
				var c = r[_x,_z];
				c.up = _z + 1 < total_z? r[_x,_z+1] : null;
				c.down = _z - 1 >= 0 ? r[_x,_z-1] : null;
				c.right = _x + 1 < total_x ? r[_x+1,_z] : null;
				c.left = _x - 1 >= 0 ? r[_x-1, _z] : null;
			}
		}
		return r;
	}
}

[System.Serializable]
public class Cell {
	public int locked;
	public bool canStepOn,canGrowTree;
	public int x,z;
	public Vector3 position;
	public Cell left,right,up,down;
	public v4Tree tree;
	
	public Cell Get(int x,int z ) {
		if ( x > 0 ) return right;
		if ( x < 0 ) return left;
		if ( z > 0 ) return up;
		if ( z < 0 ) return down;
		return this;
	}
}
