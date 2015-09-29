using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class p3Map : MonoBehaviour {
	public p3Cell cellPrefab;
	
	public byte total_x { get; private set; }
	public byte total_z { get; private set; }
	public float offset_x { get; private set; }
	public float offset_z { get; private set; }
	public Vector3 root { get; private set; } 
	
	List<AbsTree > perActionTreeList;
	List<AbsTree > perTurnTreeList;
	
	public p3Cell[,] cells { get; private set; }
	
	public byte localTotalX=10, localTotalY=8;
	public float localOffsetX=1, localOffsetY=1;
	
	public void SetUp () {
		CreateOne(localTotalX,localTotalY,localOffsetX,localOffsetY,-6,0,-4);
		
		transform.SetParent(p3Names.Instance.transform);
	}
	
	public void OnRematch () {
		foreach(var c in cells ) {
			c.Reset();
		}
	}
	
	public List<p3Cell> FreeCells () {
		return p3Cell.free;
	}
	public p3Cell GetPointedCell(Vector2 point ) {
		int x = Mathf.FloorToInt ((point.x - root.x) / offset_x);
		int z = Mathf.FloorToInt ((point.y - root.z) / offset_z);
		return IsValidIndex(x,z)?cells[x,z]:null;
	}
	
	public bool IsInMapZone (Vector2 point ) {
		return point.x >= root.x & point.x <= (root.x + total_x*offset_x ) 
			& point.y >= root.z & point.y <= (root.z + total_z*offset_z );
	}

	
	public void OnPlayerChop ( p3Player p, p3Cell chopedCell, int acCost ) {
		if ( chopedCell.tree != null ) {
			chopedCell.tree.OnBeingChoped( p, p.currentCell, 0, acCost);
		}
		if ( chopedCell.player != null &  chopedCell.player != p ) {
			chopedCell.player.OnBeingChoped(p,acCost);
		}
	}
	//--------------------------------------------------------------------------------------------------------------
	public p3Cell this[int x,int z] {
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
		
		cells = new p3Cell[total_x,total_z];
		
		for(int _z = 0; _z < total_z; _z ++ ) {
			for ( int _x = 0; _x < total_x ; _x ++ ) {
				var g = Instantiate ( cellPrefab);
				g.transform.parent = gameObject.transform;
				
				p3Cell c = g.GetComponent<p3Cell>();
				c.x = _x;
				c.z = _z;
				c.transform.position = Locate(_x,_z);
				c.Reset();
				
				cells[_x,_z] = c;
				
				g.transform.position = c.transform.position;
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
	
	private static p3Map _instance;
	public static p3Map Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p3Map)) as p3Map;
				if ( _instance == null ) throw new UnityException("Object of type p3Map not found");
			}
			return _instance;
		}
	}
	
}
