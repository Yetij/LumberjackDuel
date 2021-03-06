﻿using System;

public class l_Swamp : LogicTree
{
    int slow_penalty = 1;
    int slow_priotiy = 0;
    int area = 1; 

    public override void Init(LogicPlayground logicPlayground, int _x, int _y, LogicJack _owner, Growth _growth = Growth.Small)
    {
        base.Init(logicPlayground, _x, _y, _owner, _growth);
        logicPlayground.onPlayerTryMove += MakeThemMoveHarder;
        type = TreeType.Swamp;
    }

    private void MakeThemMoveHarder(LogicJack jack, ref int ac_remain, ref int priorityLock)
    {
        if ( slow_priotiy <priorityLock  ) return;
        if (!this.InRange(jack, area)) return; 
        ac_remain -= slow_penalty;   // normal move cost = 1;
    }

    public override void Flush(LogicPlayground logicPlayground)
    {

        logicPlayground.onPlayerTryMove -= MakeThemMoveHarder;
    }
}
