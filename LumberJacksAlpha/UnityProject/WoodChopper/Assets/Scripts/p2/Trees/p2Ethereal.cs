using UnityEngine;
using System.Collections;

// drzewo eteryczne 
public class p2Ethereal : p3AbsTree
{
	p3Player source;

	public override void OnBeingPlant (p3Player p, int deltaTurn)
	{
		base.OnBeingPlant (p, deltaTurn);
		source = p;
	}

	public override void OnBeingChoped ( p3Player player, p3Cell sourceCell, int tier,int ac)
	{
		if ( player != source & tier == 0 & state != StaticStructure.TreeState.InSeed ) return;
		base.OnBeingChoped (player,sourceCell, tier, ac);
	}
}

