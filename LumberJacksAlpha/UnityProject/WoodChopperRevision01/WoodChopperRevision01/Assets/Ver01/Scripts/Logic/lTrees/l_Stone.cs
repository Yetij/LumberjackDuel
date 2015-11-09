using UnityEngine;
using System.Collections.Generic;

public class l_Stone : LogicTree
{
    public override void Init(LogicPlayground logicPlayground, int _x, int _y, LogicJack _owner, Growth _growth = Growth.Small)
    {
        base.Init(logicPlayground, _x, _y, _owner, _growth);
        type = TreeType.Stone;
    }

    public override bool PassDomino()
    {
        return false;
    }

    public override bool BeingChopped(LogicJack jack,bool directly)
    {
        return false;
    }
}
