public class l_Ethereal : LogicTree
{
    public override void Init(LogicPlayground logicPlayground, int _x, int _y, LogicJack _owner, Growth _growth = Growth.Small)
    {
        base.Init(logicPlayground, _x, _y, _owner, _growth);
        type = TreeType.Ethereal;
    }

    public override bool BeingChopped(LogicJack jack, bool directly)
    {
        return directly? (jack == owner) : true;
    }
}
