using UnityEngine;
using System.Collections.Generic;

// bagienne
public class p2Swamp : p3AbsTree
{
	public int moveAdditionalCost=1;
	[Range(1,50)]
	public int area_x;
	[Range(1,50)]
	public int area_z;

	List<p3Player > affected = new List<p3Player>(2);

	p3Map localMap;

	protected override void Start ()
	{
		base.Start ();
		if ( area_x % 2 == 0 ) throw new UnityException("Invalid area_x="+area_x + ", must be an odd number");
		if ( area_z % 2 == 0 ) throw new UnityException("Invalid area_z="+area_z + ", must be an odd number");
		localMap = p3Map.Instance;
	}

	public override void OnTurnStart (int turn_nb)
	{
		affected.Clear();
	}

	protected override void ActivateOnFall (p3Player player)
	{
		foreach(var p in affected ) {
			p.bonus.moveCost -= moveAdditionalCost;
		}
		affected.Clear();
	}

	public override void OnTouchEnter ()
	{
		Debug.Log("swamp: OnTouchEnter");
		base.OnTouchEnter ();
		var aoex = (area_x - 1 )/2;
		var aoez = (area_z - 1 )/2;
		p3Cell c;

		for(int x=-aoex; x <= aoex; x ++ ) {
			for (int z=-aoez; z <= aoez; z++ ) {
				if( x == 0 & z == 0 ) continue;
				c = localMap[cell.x + x,cell.z +z];
				if ( c != null ) {
					c.AuraOn(true);
				}
			}
		}
	}

	public override void OnTouchExit ()
	{
		Debug.Log("swamp: OnTouchExit");
		base.OnTouchExit ();
		var aoex = (area_x - 1 )/2;
		var aoez = (area_z - 1 )/2;
		p3Cell c;
		
		for(int x=-aoex; x <= aoex; x ++ ) {
			for (int z=-aoez; z <= aoez; z++ ) {
				if( x == 0 & z == 0 ) continue;
				c = localMap[cell.x + x,cell.z +z];
				if ( c != null ) {
					c.AuraOn(false);
				}
			}
		}
	}

	public override bool Activate ()
	{
		if ( base.Activate() ) {
			var aoex = (area_x - 1 )/2;
			var aoez = (area_z - 1 )/2;
			foreach ( var p in p3Scene.Instance.players ) {
				var c = p.currentCell;
				if ( Mathf.Abs( c.x - cell.x ) <= aoex & Mathf.Abs ( c.z - cell.z ) <= aoez ) {
					if ( !affected.Contains(p ) ) {
						affected.Add(p);
						p.bonus.moveCost += moveAdditionalCost;
					}
				} else {
					if ( affected.Contains(p ) ) {
						affected.Remove(p);
						p.bonus.moveCost -= moveAdditionalCost;
					}
				}
			}
			return true;
		}
		return false;
	}
}
