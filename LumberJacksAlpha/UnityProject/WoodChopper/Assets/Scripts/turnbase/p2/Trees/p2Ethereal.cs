using UnityEngine;
using System.Collections;

// drzewo eteryczne 
public class p2Ethereal : AbsTree
{
	p2Player source;

	public override void OnBeingPlant (p2Player p, int deltaTurn)
	{
		base.OnBeingPlant (p, deltaTurn);
		source = p;
	}

	public override void OnBeingChoped ( p2Player player, int tier)
	{
		if ( player != source ) return;
		base.OnBeingChoped (player, tier);
	}
}

