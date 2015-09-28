using UnityEngine;
using System.Collections;

public class p2BonusAc : p3AbsTree
{
	public int bonusAc = 1;
	public override bool Activate ()
	{
		if ( base.Activate() ) {
			foreach( var p in p3Scene.Instance.players ) {
				p.bonus.actionPoints += bonusAc;
			}
			return true;
		}
		return false;
	}
}

