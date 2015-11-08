using System;

public class l_Swamp : LogicTree
{
    int slow_factor = 2;
    int slow_priotiy = 0;
    int area = 1; 

    public override void Init(LogicPlayground logicPlayground, int _x, int _y, LogicJack _owner, Growth _growth = Growth.Small)
    {
        base.Init(logicPlayground, _x, _y, _owner, _growth);
        logicPlayground.onPlayerTryMove += MakeThemMoveHarder;
    }

    private void MakeThemMoveHarder(LogicJack jack, ref int ac_remain, ref int priorityLock)
    {
        if ( slow_priotiy <priorityLock  ) return;
        if (!this.InRange(jack, area)) return; 
        ac_remain -= 1 * slow_factor;   // normal move cost = 1;
    }
}
