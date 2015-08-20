
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

}

