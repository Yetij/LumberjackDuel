
public class p2Stone : AbsTree
{
	public override bool CanBeAffectedByDomino ()
	{
		return false;
	} 

	public override bool CanBeChopedDirectly ()
	{
		return false;
	}

	public override bool IsPassable ()
	{
		return false;
	}

}

