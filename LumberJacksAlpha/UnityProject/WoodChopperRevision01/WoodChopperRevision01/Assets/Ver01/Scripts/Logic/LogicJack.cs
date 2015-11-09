using System;
using System.Collections.Generic;

public class LogicJack  {
    public PLAY player { get; private set;  }

    public int hp, ac, x, y;
    public int points;
    public int matchScore ;
    public int chop_range = 1; // default;
    public int move_range = 1;  // default;
    public int plant_range = 1;  // default;


    public LogicJack Opponent { get; internal set; }


    public LogicJack(PLAY player, int x, int y) 
    {
        this.player = player;
        this.x = x;
        this.y = y;
        matchScore = 0;
        points = 0;
    }

    internal bool BeingChop(LogicJack logicJack)
    {
        // hp --;
        //throw new NotImplementedException();
        UnityEngine.Debug.Log("Unimplemented feature ");
        return true;
    }

    internal bool ValidChopRange(int x, int y)
    {
        return Math.Abs(this.x - x) <= chop_range & Math.Abs(this.y - y) <= chop_range;
    }


    internal int EstimateEarnedPoints(LogicTree tree, List<LogicTree> domino)
    {
        int p = 0;
        if ( domino.Count > 0 )
        {
            p = domino.Count+1;
            p *= p;
        } else
        {
            p = 1;
        }
        return p;
    }

    internal bool ValidMoveRange(int x, int y)
    {
        int _x = Math.Abs(this.x - x);
        int _y = Math.Abs(this.y - y);
        if (_x > plant_range | _y > plant_range) return false;
        if (_x * _y > 0) return false;
        return true;
    }

    internal void MoveTo(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    internal bool ValidPlantRange(int x, int y)
    {
        return Math.Abs(this.x - x) <= plant_range & Math.Abs(this.y - y) <= plant_range;
    }
}
