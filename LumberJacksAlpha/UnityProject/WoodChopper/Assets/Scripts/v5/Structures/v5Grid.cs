using UnityEngine;

[System.Serializable]
public class v5Grid {
	public int total_x, total_z;
	public float offset_x, offset_z;
	public Vector3 root;
	
	Vector3 Locate ( int x, int z ) {
		return new Vector3(x*offset_x + offset_x/2 + root.x, root.y, z * offset_z + offset_z/2 + root.z);
	}
	public v5Cell[,] GenCells () {
		v5Cell[,] r = new v5Cell[total_x,total_z];
		for(int _z = 0; _z < total_z; _z ++ ) {
			for ( int _x = 0; _x < total_x ; _x ++ ) {
				v5Cell c = new v5Cell();
				c.x = _x;
				c.z = _z;
				c.position = Locate(_x,_z);
				c.locked = -1;
				c.tree = null;
				c.lock_time = double.MaxValue;
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

	public v5Cell RandomCell (v5Cell[,] cells ) {
		return cells[Random.Range(0,total_x),Random.Range(0,total_z)];
	}
}

[System.Serializable]
public class v5Cell {
	public int locked;
	public double lock_time;
	public int x,z;
	public Vector3 position;
	public v5Cell left,right,up,down;
	public v5Tree tree;
	
	public v5Cell Get(int x,int z ) {
		if ( x > 0 ) return right;
		if ( x < 0 ) return left;
		if ( z > 0 ) return up;
		if ( z < 0 ) return down;
		return this;
	}
}
