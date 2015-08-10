using UnityEngine;
using System.Collections;

// bagienne
public class p2Swamp : AbsTree
{
	[Range(1,50)]
	public int area_x;
	[Range(1,50)]
	public int area_z;

	protected override void Start ()
	{
		base.Start ();
		if ( area_x % 2 == 0 ) throw new UnityException("Invalid area_x="+area_x + ", must be an odd number");
		if ( area_z % 2 == 0 ) throw new UnityException("Invalid area_z="+area_z + ", must be an odd number");
	}


	public override void Activate ()
	{
		var startx = cell.x - (area_x - 1 )/2;
		var startz = cell.x - (area_z - 1 )/2;
		for(int j= startz; j < startz + area_z; j++ ) {
			for(int i= startx; i < startx + area_x; i++ ) {
				var c = p2Map.Instance[i,j];
				if ( c != null ) {
					if ( c.player != null ){
						c.player.bonus.moveCost += 2;
					}
				}
			}
		}
	}
}
