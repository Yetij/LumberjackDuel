using UnityEngine;
using System.Collections.Generic;

public class l_MonumentalTree : LogicTree
{
    public override void Init(LogicPlayground logicPlayground, int _x, int _y, LogicJack _owner, Growth _growth = Growth.Small)
    {
        base.Init(logicPlayground, _x, _y, _owner, _growth);
        type = TreeType.Monumental;

    }

    public override bool BeingChopped(LogicJack jack, bool directly)
    {
        Debug.LogError("!MODIFIED FOR TEST PURPOSE ONLY !");
        return directly? true: false;
        //if (!directly & growth == Growth.FullGrown) return false;
       // return true;
    }
    
}
