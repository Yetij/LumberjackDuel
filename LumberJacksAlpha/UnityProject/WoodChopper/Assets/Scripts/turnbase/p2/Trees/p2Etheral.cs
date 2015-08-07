using UnityEngine;
using System.Collections;

// drzewo eteryczne 
public class p2Etheral : AbsTree
{
	p2Player source;

	public override void OnBeingPlant (p2Player p, int deltaTurn)
	{
		base.OnBeingPlant (p, deltaTurn);
		source = p;
	}

	public override void OnBeingChoped (System.Collections.Generic.List<p2FallRecord> falllist, p2Player player, int tier)
	{
		if ( source != player ) return;
		base.OnBeingChoped (falllist, player, tier);
	}
}

