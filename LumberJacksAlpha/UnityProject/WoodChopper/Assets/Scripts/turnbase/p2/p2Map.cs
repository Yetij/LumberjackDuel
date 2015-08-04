using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class p2Map : MonoBehaviour {
	public p2Cell cellPrefab;

	public byte total_x { get; private set; }
	public byte total_z { get; private set; }
	public float offset_x { get; private set; }
	public float offset_z { get; private set; }
	public Vector3 root { get; private set; } 

	List<AbsTree > perActionTreeList;
	List<AbsTree > perTurnTreeList;

	public p2Cell[,] cells { get; private set; }
	
	void Awake () {
		CreateOne(10,8,1,1,-7,0,-4);
	}

	public void ApplyPerActionBuffs (p2Player player, p2Cell currentCell) {
	}

	public p2Cell GetPointedCell(Vector2 point ) {
		int x = Mathf.FloorToInt ((point.x - root.x) / offset_x);
		int z = Mathf.FloorToInt ((point.y - root.z) / offset_z);
		return IsValidIndex(x,z)?cells[x,z]:null;
	}

	public bool IsInMapZone (Vector2 point ) {
		return point.x >= root.x & point.x <= (root.x + total_x*offset_x ) 
			& point.y >= root.z & point.y <= (root.z + total_z*offset_z );
	}

//--------------------------------------------------------------------------------------------------------------
	public p2Cell this[int x,int z] {
		get {
			return IsValidIndex(x,z)? cells[x,z]: null;
		}
	}
	
	public void CreateOne (byte x,byte z, float offx, float offz, float rootx, float rooty, float rootz) {
		total_x = x;
		total_z = z;
		offset_x  = offx;
		offset_z = offz;
		root = new Vector3(rootx,rooty,rootz);
		
		cells = new p2Cell[total_x,total_z];
		
		for(int _z = 0; _z < total_z; _z ++ ) {
			for ( int _x = 0; _x < total_x ; _x ++ ) {
				var g = Instantiate ( cellPrefab);
				g.transform.parent = gameObject.transform;
				
				p2Cell c = g.GetComponent<p2Cell>();
				c.x = _x;
				c.z = _z;
				c.position = Locate(_x,_z);
				
				cells[_x,_z] = c;
				
				g.transform.position = c.position;
			}
		}
		for(int _z = 0; _z < total_z; _z ++ ) {
			for ( int _x = 0; _x < total_x ; _x ++ ) {
				var c = cells[_x,_z];
				c.Link(0,1,_z + 1 < total_z? cells[_x,_z+1] : null);
				c.Link(0,-1, _z - 1 >= 0 ? cells[_x,_z-1] : null);
				c.Link(1,0,_x + 1 < total_x ? cells[_x+1,_z] : null);
				c.Link(-1,0,_x - 1 >= 0 ? cells[_x-1, _z] : null);
				
				c.Link(-1,1,(_z + 1 < total_z & _x - 1 >= 0 ) ? cells[_x - 1,_z + 1] : null);
				c.Link(1,1,(_z + 1 < total_z & _x + 1 < total_x ) ? cells[_x + 1,_z + 1] : null);
				c.Link(1,-1,(_z - 1 >= 0 & _x + 1 < total_x ) ? cells[_x+1,_z -1] : null);
				c.Link(-1,-1,(_z - 1 >= 0 & _x - 1 >= 0 ) ? cells[_x-1, _z-1] : null);
			}
		}
	}
	
	bool IsValidIndex(int x, int z ) {
		return x >= 0 & x < total_x & z >=0 & z < total_z;
	}
	
	Vector3 Locate ( int x, int z ) {
		return new Vector3(x*offset_x + offset_x/2 + root.x, root.y, z * offset_z + offset_z/2 + root.z);
	}

	private static p2Map _instance;
	public static p2Map Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p2Map)) as p2Map;
				if ( _instance == null ) throw new UnityException("Object of type p2Map not found");
			}
			return _instance;
		}
	}

}
