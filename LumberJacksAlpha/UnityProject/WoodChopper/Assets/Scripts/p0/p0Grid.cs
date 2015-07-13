using UnityEngine;
using System.Collections.Generic;

public class p0Grid : MonoBehaviour {
	public byte total_x { get; private set; }
	public byte total_z { get; private set; }
	public float offset_x { get; private set; }
	public float offset_z { get; private set; }
	public Vector3 root { get; private set; } 
	
	public void Init (byte x,byte z, float offx, float offz, float rootx, float rooty, float rootz) {
		total_x = x;
		total_z = z;
		offset_x  = offx;
		offset_z = offz;
		root = new Vector3(rootx,rooty,rootz);
	}
	
	Vector3 Locate ( int x, int z ) {
		return new Vector3(x*offset_x + offset_x/2 + root.x, root.y, z * offset_z + offset_z/2 + root.z);
	}

	public p0Cell this[int x,int z] {
		get {
			return IsValidIndex(x,z)? cells[x,z]: null;
		}
	}

	public List<p0Cell> frees;
	
	p0Cell[,] cells;
	
	public p0Cell[,]  GetCells () {
		if ( cells != null ) return cells;
		if ( root == null ) throw new UnityException("Grid params not initialized");
		cells = new p0Cell[total_x,total_z];
		frees = new List<p0Cell>(total_x*total_z);
		for(int _z = 0; _z < total_z; _z ++ ) {
			for ( int _x = 0; _x < total_x ; _x ++ ) {
				var g = Instantiate ( p0Const.Instance.gridSettings.cell);
				g.transform.parent = gameObject.transform;

				p0Cell c = g.GetComponent<p0Cell>();
				c.x = _x;
				c.z = _z;
				c.position = Locate(_x,_z);
				c.freeList = frees;
				c.locked = -1;

				cells[_x,_z] = c;

				g.transform.position = c.position;
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

