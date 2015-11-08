using UnityEngine;
using System.Collections.Generic;

public class l_Stone : LogicTree
{

    public override bool AffectedByDomino()
    {
        return false;
    }

    public override bool PassDomino()
    {
        return false;
    }

    public override bool BeingChopped(LogicJack jack)
    {
        return false;
    }
}
