using UnityEngine;
using System.Collections;

public class p2BonusAc : AbsTree
{
	public int bonusAc = 1;
	public override bool Activate ()
	{
		if ( base.Activate() ) {
			foreach( var p in p2Scene.Instance.players ) {
				p.bonus.actionPoints += 1;
			}
			return true;
		}
		return false;
	}
}

