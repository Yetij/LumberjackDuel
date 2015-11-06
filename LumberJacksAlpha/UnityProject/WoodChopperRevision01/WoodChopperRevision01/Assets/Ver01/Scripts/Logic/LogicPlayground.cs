using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class LogicPlayground  {
    private int gridX;
    private int gridY;
    private float offsetX;
    private float offsetY;

    public List<LogicTree> trees { get; private set; }
    static TreeType[] t_types = new TreeType[] {
        TreeType.Basic,
        TreeType.BonusAc,
        TreeType.Ethereal,
        TreeType.Monumental,
        TreeType.Stone,
        TreeType.Swamp
    };

    public LogicPlayground(int gridX, int gridY, float offsetX, float offsetY)
    {
        this.gridX = gridX;
        this.gridY = gridY;
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        trees = new List<LogicTree>();
    }
    
    internal void RandomTree(int startTreeNumber)
    {
        throw new NotImplementedException();
    }

    internal void TurnChange(LogicJack[] character)
    {
        throw new NotImplementedException();
    }

    internal bool PlantTree(LogicJack logicJack, TreeType treeSelected, int x, int y)
    {
        throw new NotImplementedException();
    }

    internal bool PlayerMove(LogicJack logicJack,int x,int y)
    {
        throw new NotImplementedException();
    }

    internal LogicTree TreeAt(int x, int y)
    {
        throw new NotImplementedException();
    }

    internal bool ChopTree(LogicJack logicJack, int x, int y, out List<LogicTree> domino)
    {
        throw new NotImplementedException();
    }
}
