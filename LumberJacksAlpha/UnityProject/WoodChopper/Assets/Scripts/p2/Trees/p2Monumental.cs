using UnityEngine;
using System.Collections;

public class p2Monumental : p3AbsTree
{
	public override bool CanBeAffectedByDomino ()
	{
		return state == StaticStructure.TreeState.InSeed;
	}
}

