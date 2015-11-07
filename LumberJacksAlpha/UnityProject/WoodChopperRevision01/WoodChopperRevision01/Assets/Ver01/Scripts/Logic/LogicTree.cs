using UnityEngine;
using System.Collections;
using System;

public enum Growth :int { Small, FullGrown }

public abstract class LogicTree  {
    public int x, y;
    public Growth growth;
    public TreeType type;
    LogicJack owner; 

    virtual public void Init(LogicPlayground logicPlayground,int _x, int _y, LogicJack _owner, Growth _growth = Growth.Small )
    {
        x = _x;
        y = _y;
        growth = _growth;
        owner = _owner;
    }
}
