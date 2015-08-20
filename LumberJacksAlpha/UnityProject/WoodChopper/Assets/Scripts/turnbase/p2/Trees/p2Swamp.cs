using UnityEngine;
using System.Collections.Generic;

// bagienne
public class p2Swamp : AbsTree
{
	public int moveAdditionalCost=1;
	[Range(1,50)]
	public int area_x;
	[Range(1,50)]
	public int area_z;

	List<p2Player > affected = new List<p2Player>(2);


	protected override void Start ()
	{
		base.Start ();
		if ( area_x % 2 == 0 ) throw new UnityException("Invalid area_x="+area_x + ", must be an odd number");
		if ( area_z % 2 == 0 ) throw new UnityException("Invalid area_z="+area_z + ", must be an odd number");
	}

	public override void OnTurnStart (int turn_nb)
	{
		affected.Clear();
	}

	protected override void ActivateOnFall (p2Player player)
	{
		foreach(var p in affected ) {
			p.bonus.moveCost -= moveAdditionalCost;
		}
		affected.Clear();
	}
	public override void Activate ()
	{
		var aoex = (area_x - 1 )/2;
		var aoez = (area_z - 1 )/2;
		foreach ( var p in p2Scene.Instance.players ) {
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

	}
}
