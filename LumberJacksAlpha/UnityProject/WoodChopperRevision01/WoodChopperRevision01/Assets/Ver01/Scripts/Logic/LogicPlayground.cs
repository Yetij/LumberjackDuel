using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public delegate void OnPlayerTryChop(LogicJack jack, ref int ac_remain, ref int dirx, ref int diry, ref int priorityLock);
public delegate void OnPlayerTryPlant(LogicJack jack, ref int ac_remain,ref int priorityLock);
public delegate void OnPlayerTryMove(LogicJack jack, ref int ac_remain, ref int priorityLock);
public delegate void OnPlayerChopDone(LogicJack jack, ref int earned_points);

public class Int2 : IEquatable<Int2>
{
    public int p, q;
    
    public Int2(int _x, int _y)
    {
        p = _x;
        q = _y;
    }

    public bool Equals(Int2 other)
    {
        return p == other.p & q == other.q;
    }

    public override string ToString()
    {
        return "(" + p + "," + q + ")";
    }
}

public class CellControl : IEnumerable<LogicTree>
{
    LogicTree[,] _trees;

    public List<Int2> free_cells { get; private set; }

    public CellControl(int gx, int gy)
    {
        _trees = new LogicTree[gx, gy];
        free_cells = new List<Int2>(gx * gy);
        for (int _y = 0; _y < gy; _y++)
        {
            for (int _x = 0; _x < gx; _x++)
            {
                free_cells.Add(new Int2 (_x,_y) );
            }
        }
    }
    
    public LogicTree this[int x, int y]
    {
        set
        {
            _trees[x, y] = value;
            if (value == null)
            {
                free_cells.Add(new Int2(x, y));
            }
            else
            {
                free_cells.Remove(new Int2(x, y));
            }
        }
        get
        {
            return _trees[x, y];
        }
    }

    public IEnumerator<LogicTree> GetEnumerator()
    {
        foreach (var r in _trees)
            yield return r;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public class LogicPlayground {
    
    public event OnPlayerTryPlant onPlayerTryPlant;
    public event Action<LogicJack> onPlayerPlantDone;

    public event OnPlayerTryMove onPlayerTryMove;
    public event Action<LogicJack> onPlayerMoveDone;

    public event OnPlayerTryChop onPlayerTryChop;
    public event OnPlayerChopDone onPlayerChopDone;

    public event Action<LogicJack[]> onTurnChange;

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

    public CellControl cellControl { get; private set;  }

    public LogicPlayground(int gridX, int gridY, float offsetX, float offsetY)
    {
        this.gridX = gridX;
        this.gridY = gridY;
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        priority = 0;
        cellControl = new CellControl(gridX, gridY);
    }

    

    public void RandomTree(int startTreeNumber)
    {
        for(int i=0; i < startTreeNumber; i ++ )
        {
            Int2 r = RandomFreeCell();
            TreeType r_type = t_types[UnityEngine.Random.Range(0, t_types.Length)];
            var t = GetTree(r_type);
        }

    }

    private Int2 RandomFreeCell()
    {
        return cellControl.free_cells.Count > 0 ? cellControl.free_cells[UnityEngine.Random.Range(0, cellControl.free_cells.Count)] : null;
    }

    public void TurnChange(LogicJack[] character)
    {
        if ( onTurnChange != null )
        {
            onTurnChange(character);
        }
    }


    public bool PlantTree(LogicJack[] jacks, int player, TreeType treeSelected, int x, int y)
    {
        if (!ValidIndex(x, y))
        {
            Debug.LogError("wtf ? x,y= " + x + "," + y);
            return false;
        }

        if ( cellControl[x,y] != null )
        {
            Debug.LogError("there is already a tree at " + x + "," + y);
            return false;
        }

        foreach (var j in jacks)
        {
            if (j.x == x & j.y == y)
            {
                return false;
            }
        }

        if (treeSelected == TreeType.None)
        {
            return false;
        }

        LogicJack logicJack = null;
        if (player >= 0)
        {
            logicJack = jacks[player];
            if (!logicJack.ValidPlantRange(x, y)) return false;

            int ac_remain = logicJack.ac - 1; // default plant cost
            int priorityLock = 0;
            if (onPlayerTryPlant != null)
            {
                onPlayerTryPlant(logicJack, ref ac_remain, ref priorityLock);
            }

            if (ac_remain < 0) return false;
            logicJack.ac = ac_remain;
        }
        
        LogicTree tree = GetTree(treeSelected);
        tree.Init(this, x,y, logicJack );
        cellControl[x, y] = tree;

        if (onPlayerPlantDone != null & player >= 0 )
        {
            onPlayerPlantDone(logicJack);
        }
        return true;
    }

    private LogicTree GetTree(TreeType treeSelected)
    {
        switch( treeSelected )
        {
            case TreeType.None:
                throw new UnityException("TreeType cant be None here");
            case TreeType.Basic:
                return new l_BasicTree();
            case TreeType.BonusAc:
                return new l_BonusAc();
            case TreeType.Ethereal:
                return new l_Ethereal();
            case TreeType.Monumental:
                return new l_MonumentalTree();
            case TreeType.Stone:
                return new l_Stone();
            case TreeType.Swamp:
                return new l_Swamp();
            default:
                throw new UnityException("Code should not reach here");

        }
    }

    bool ValidIndex(int x, int y)
    {
        return x >= 0 & x < gridX & y >= 0 & y < gridY;
    }

    public bool PlayerMove(LogicJack jack,int x,int y)
    {
        if (!jack.ValidMoveRange(x, y)) return false;
        if (jack.x == x & jack.y == y) return false;

        int ac_remain = jack.ac-1; // default cost
        int priorityLock = 0;
        if (onPlayerTryMove != null)
        {
            onPlayerTryMove(jack, ref ac_remain, ref priorityLock);
        }

        if (ac_remain < 0)
        {
            Debug.Log("not enough ac");
            return false;   
        }

        jack.ac = ac_remain;
        jack.MoveTo(x, y); 

        if (onPlayerMoveDone != null)
        {
            onPlayerMoveDone(jack);
        }
        return true;

    }

    public LogicTree TreeAt(int x, int y)
    {
        if (ValidIndex(x, y)) return cellControl[x, y];
        return null;
    }
    public bool ChopTree(LogicJack jack, int x, int y, out List<LogicTree> domino)
    {
        domino = null;

        if (!jack.ValidChopRange(x, y)) return false;

        if (!ValidIndex(x, y))
        {
            Debug.LogError("wtf ? x,y= " + x + "," + y);
            return false;
        }

        int dirx = x - jack.x;
        int diry = y - jack.y;

        int ac_remain = jack.ac - 1; // default cost = 1;
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
        
        var tree = cellControl[x, y];
        
        if (!ChopTree2(jack, tree)) return false;

        //tree chop succeeded
        jack.ac = ac_remain;
        domino = new List<LogicTree>();
        
        for (int i = x+ dirx, j = y+diry; i < gridX & i >= 0 & j < gridY & j >= 0; i += dirx, j += diry)
        {
            var t = cellControl[i, j];
            if ( t != null)
            {
                if (!ChopTree2(jack, t, false)) break;
                domino.Add(t);
                if (!t.PassDomino())
                {
                    break;
                }
            }
            else break;
        }
        
        int earned_point = jack.EstimateEarnedPoints(tree, domino);

        tree.AfterChop(jack, ref earned_point);
        foreach ( var _t in domino)
        {
            _t.AfterChop(jack, ref earned_point);
        }

        if (onPlayerChopDone != null)
        {
            onPlayerChopDone(jack,ref earned_point);
        }

        jack.points += earned_point;

        return true;
    }

    private bool ChopTree2(LogicJack jack, LogicTree logicTree,bool directly = true)
    {
        if (logicTree.BeingChopped(jack, directly))
        {
            cellControl[logicTree.x, logicTree.y] = null;
            logicTree.Flush(this);
            return true;
        }
        return false;
    }

    public bool ChopPlayer(LogicJack chopper, LogicJack another)
    {
        if (!chopper.ValidChopRange(another.x, another.y)) return false;

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
            int zero = 0;
            onPlayerChopDone(chopper,ref zero);
        }
        
        return true;
    }
}
