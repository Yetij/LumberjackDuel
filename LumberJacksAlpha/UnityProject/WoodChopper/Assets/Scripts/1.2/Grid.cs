using UnityEngine;
using System.Collections.Generic;
	
public class Grid {
	public readonly byte total_x,total_z;
	public readonly float offset_x,offset_z;
	public readonly Vector3 root;

	public Grid(byte x,byte z, float offx, float offz, float rootx, float rooty, float rootz) {
		total_x = x;
		total_z = z;
		offset_x  = offx;
		offset_z = offz;
		root = new Vector3(rootx,rooty,rootz);
	}

	Vector3 Locate ( int x, int z ) {
		return new Vector3(x*offset_x + offset_x/2 + root.x, root.y, z * offset_z + offset_z/2 + root.z);
	}
	
	
	public Cell this[int x,int z] {
		get {
			return IsValidIndex(x,z)? cells[x,z]: null;
		}
	}
	
	Cell[,] cells;
	
	public Cell[,]  GetCells () {
		if ( cells != null ) return cells;
		cells = new Cell[total_x,total_z];
		for(int _z = 0; _z < total_z; _z ++ ) {
			for ( int _x = 0; _x < total_x ; _x ++ ) {
				Cell c = new Cell();
				c.x = _x;
				c.z = _z;
				c.position = Locate(_x,_z);
				c.Free();
				cells[_x,_z] = c;
			}
		}
		for(int _z = 0; _z < total_z; _z ++ ) {
			for ( int _x = 0; _x < total_x ; _x ++ ) {
				var c = cells[_x,_z];
				c.up = _z + 1 < total_z? cells[_x,_z+1] : null;
				c.down = _z - 1 >= 0 ? cells[_x,_z-1] : null;
				c.right = _x + 1 < total_x ? cells[_x+1,_z] : null;
				c.left = _x - 1 >= 0 ? cells[_x-1, _z] : null;
			}
		}
		return cells;
	}

	bool IsValidIndex(int x, int z ) {
		return x >= 0 & x < total_x & z >=0 & z < total_z;
	}
}

