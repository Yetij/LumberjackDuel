using UnityEngine;
using System.Collections;

public class p2BonusAc : AbsTree
{
	public int bonusAc = 1;
	public override void Activate ()
	{
		foreach( var p in p2Scene.Instance.players ) {
			p.bonus.actionPoints += 1;
		}
	}
}

