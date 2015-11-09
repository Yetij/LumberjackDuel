using UnityEngine;
using System.Collections;
using System;


public enum Growth :int { Small = 0, FullGrown =1}

public static class LogicTreeExtensions
{
    public static bool InRange(this LogicTree tree, LogicJack jack, int area)
    {
        return Mathf.Abs(tree.x - jack.x) <= area & Mathf.Abs(tree.y - jack.y) <= area;
    }
}

public abstract class LogicTree {
    public int x { get; protected set; }
    public int y { get; protected set; }
    public Growth growth { get; protected set; }
    public TreeType type { get; protected set; }
    protected LogicJack owner;

    virtual public void Init(LogicPlayground logicPlayground,int _x, int _y, LogicJack _owner, Growth _growth = Growth.Small )
    {
        x = _x;
        y = _y;
        growth = _growth;
        owner = _owner;
    }

    virtual public bool BeingChopped(LogicJack jack, bool directly)
    {
        return true;
    }

    virtual public bool PassDomino()
    {
        Debug.LogError("!MODIFIED FOR TEST PURPOSE ONLY !");
        return true;
        //return growth != Growth.Small;
    }

    virtual public void Flush(LogicPlayground logicPlayground)
    {
    }

    internal void AfterChop(LogicJack jack, ref int earned_point)
    {
    }
}
