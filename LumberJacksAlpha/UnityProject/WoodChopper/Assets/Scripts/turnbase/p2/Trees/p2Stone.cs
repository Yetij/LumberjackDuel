using UnityEngine;

public class p2Stone : AbsTree
{
	public override bool CanBeAffectedByDomino ()
	{
		return false;
	} 

	protected override void ActivateOnChop (p2Player player, ref int fx, ref int fz)
	{
		fx = 0;
		fz = 0;
	}

	public override bool IsPassable ()
	{
		return false;
	}

	public override void OnBeingPlant (p2Player p , int deltaTurn ) {
		base.OnBeingPlant(p,deltaTurn);
		turnToLifeCounter = -1;

		state = StaticStructure.TreeState.Grown;
		transform.localScale = Vector3.one;
	}
}

