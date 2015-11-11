using System;

public class l_BonusAc : LogicTree
{
    int bonus_amount = 1;
    public override void Init(LogicPlayground logicPlayground, int _x, int _y, LogicJack _owner, Growth _growth = Growth.Small)
    {
        base.Init(logicPlayground, _x, _y, _owner, _growth);
        logicPlayground.onTurnChange += GiveBonusAc;
        type = TreeType.BonusAc;
    }

    public override void Flush(LogicPlayground logicPlayground)
    {
        logicPlayground.onTurnChange -= GiveBonusAc;
    }

    private void GiveBonusAc(LogicJack[] jacks)
    {
        foreach ( var j in jacks )
        {
            j.ac += bonus_amount;
        }
    }
}
