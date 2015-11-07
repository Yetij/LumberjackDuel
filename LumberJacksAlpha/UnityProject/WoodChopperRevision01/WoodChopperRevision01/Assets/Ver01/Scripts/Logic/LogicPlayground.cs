using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public delegate void OnPlayerTryChop(LogicJack jack, ref int ac_remain, ref int dirx, ref int diry, ref int priorityLock);
public delegate void OnPlayerTryPlant(LogicJack jack, ref int ac_remain,ref int priorityLock);
public delegate void OnPlayerTryMove(LogicJack jack, ref int ac_remain, ref int priorityLock);

public class LogicPlayground  {
    private int gridX;
    private int gridY;
    private float offsetX;
    private float offsetY;

    int priority;
    
    static TreeType[] t_types = new TreeType[] {
        TreeType.Basic,
        TreeType.BonusAc,
        TreeType.Ethereal,
        TreeType.Monumental,
        TreeType.Stone,
        TreeType.Swamp
    };
    public LogicTree[,] trees { get; private set; }

    public LogicPlayground(int gridX, int gridY, float offsetX, float offsetY)
    {
        this.gridX = gridX;
        this.gridY = gridY;
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        trees = new LogicTree[gridX, gridY];
        priority = 0;
    }

    public event OnPlayerTryPlant onPlayerTryPlant;
    public event Action<LogicJack> onPlayerPlantDone;

    public event OnPlayerTryMove onPlayerTryMove;
    public event Action<LogicJack> onPlayerMoveDone;

    public event OnPlayerTryChop onPlayerTryChop;
    public event Action<LogicJack,int> onPlayerChopDone;

    public event Action<LogicJack[]> onTurnChange;

    public void RandomTree(int startTreeNumber)
    {
        throw new NotImplementedException();
    }

    public void TurnChange(LogicJack[] character)
    {
        if ( onTurnChange != null )
        {
            onTurnChange(character);
        }
    }


    public bool PlantTree(LogicJack logicJack, TreeType treeSelected, int x, int y)
    {
        if (!ValidIndex(x, y))
        {
            Debug.LogError("wtf ? x,y= " + x + "," + y);
            return false;
        }
        if (treeSelected == TreeType.None)
        {
            Debug.LogError("TreeType cant be None in this case");
            return false;
        }
        int ac_remain = logicJack.ac;
        int priorityLock = 0;
        if ( onPlayerTryPlant != null & logicJack != null)
        {
            onPlayerTryPlant(logicJack,ref ac_remain, ref priorityLock);
        }

        if (ac_remain < 0) return false;
        logicJack.ac = ac_remain;
        
        LogicTree tree = GetTree(treeSelected);
        tree.Init(this, x,y, logicJack);
        trees[x, y] = tree;

        if (onPlayerPlantDone != null & logicJack != null)
        {
            onPlayerPlantDone(logicJack);
        }
        return true;
    }

    private LogicTree GetTree(TreeType treeSelected)
    {
        throw new NotImplementedException();
    }

    bool ValidIndex(int x, int y)
    {
        return x >= 0 & x < gridX & y >= 0 & y < gridY;
    }

    public bool PlayerMove(LogicJack jack,int x,int y)
    {
        int ac_remain = jack.ac;
        int priorityLock = 0;
        if (onPlayerTryMove != null)
        {
            onPlayerTryMove(jack, ref ac_remain, ref priorityLock);
        }

        if (ac_remain < 0 ) return false;
        jack.ac = ac_remain;

        if (onPlayerMoveDone != null)
        {
            onPlayerMoveDone(jack);
        }
        return true;

    }

    public LogicTree TreeAt(int x, int y)
    {
        if (ValidIndex(x, y)) return trees[x, y];
        return null;
    }
    public bool ChopTree(LogicJack jack, int x, int y, out List<LogicTree> domino)
    {
        domino = null;

        if (!jack.ValidRange(x, y)) return false;

        if (!ValidIndex(x, y))
        {
            Debug.LogError("wtf ? x,y= " + x + "," + y);
            return false;
        }

        int dirx = x - jack.x;
        int diry = y - jack.y;

        int ac_remain = jack.ac;
        int priorityLock = 0;
        if ( onPlayerTryChop != null )
        {
            onPlayerTryChop(jack,ref ac_remain, ref dirx, ref diry, ref priorityLock);
        }
       
        if ( dirx == 0 & diry == 0 )
        {
            return false;
        }

        if (ac_remain < 0) return false;
        jack.ac = ac_remain;

        domino = new List<LogicTree>();
        for (int i = x, j = y; i < gridX & i >= 0 & j < gridY & j >= 0; i += dirx, j += diry)
        {
            if (trees[i, j] != null) domino.Add(trees[i, j]);
            else break;
        }
        int earned_point = jack.EarnPoints(domino);

        if (onPlayerChopDone != null)
        {
            onPlayerChopDone(jack, earned_point);
        }
        return true;
    }

    public bool ChopPlayer(LogicJack chopper, LogicJack another)
    {
        if (!chopper.ValidRange(another.x, another.y)) return false;

        int dirx = another.x - chopper.x;
        int diry = another.y - chopper.y;

        int ac_remain = chopper.ac;
        int priorityLock = 0;
        if (onPlayerTryChop != null)
        {
            onPlayerTryChop(chopper, ref ac_remain, ref dirx, ref diry, ref priorityLock);
        }

        if (dirx == 0 & diry == 0)
        {
            return false;
        }

        if (ac_remain < 0) return false;
        chopper.ac = ac_remain;
        another.BeingChop(chopper);

        if (onPlayerChopDone != null)
        {
            onPlayerChopDone(chopper, 0);
        }
        return true;
    }
}
