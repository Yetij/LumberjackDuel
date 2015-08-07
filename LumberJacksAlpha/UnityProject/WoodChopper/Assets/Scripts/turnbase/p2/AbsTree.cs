using UnityEngine;
using System.Collections.Generic;

public class AbsTree : MonoBehaviour
{
	[HideInInspector] public p2Cell cell;
	public AbsBuff[] buffs;
	public int pointCredit;

	public int turnToLife;

	virtual public void OnBeingPlant (p2Player p , int deltaTurn ) {
		turnToLife = deltaTurn;
	}

	virtual public bool IsPassable () {
		return false;
	}

	virtual public bool CanBeChopedDirectly () {
		return true;
	}

	virtual public bool CanBeAffectedByDomino () {
		return true;
	}

	protected int[] dominoPass;
	virtual public int[] PassDominoFuther () {
		return dominoPass;
	}

	virtual public void OnBeingChoped ( List<p2FallRecord> falllist, p2Player player, int tier ) {
		int fx = player.currentCell.x - cell.x;
		int fz = player.currentCell.z - cell.z;
		ActivateOnChop(player, ref fx, ref fz);
		if ( !(fx == 0 & fz == 0 ) ) {
			//falllist.Add(new p2FallRecord(this,DeltaTurn(),tier));
		}
	}

	virtual protected int DeltaTurn () {
		return 0;
	}

	virtual protected void ActivateOnChop (p2Player player, ref int fx, ref int fz ) {
	}

	virtual protected void ActivateOnFall (p2Player player ) {
	}

	virtual public p2TreeType TreeType () {
		return p2TreeType.Basic;
	}
}

