using System;
using System.Collections.Generic;

public class LogicJack  {
    public PLAY player { get; private set;  }

    public int hp, ac, x, y;
    public int points;
    public int matchScore ;
    public int range;

    public LogicJack Opponent { get; internal set; }


    public LogicJack(PLAY player, int x, int y) 
    {
        this.player = player;
        this.x = x;
        this.y = y;
        matchScore = 0;
        points = 0;
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    internal bool BeingChop(LogicJack logicJack)
    {
        throw new NotImplementedException();
    }

    internal bool ValidRange(int x, int y)
    {
        throw new NotImplementedException();
    }

    internal int EarnPoints(List<LogicTree> domino)
    {
        throw new NotImplementedException();
    }

    public static explicit operator int (LogicJack v)
    {
        throw new NotImplementedException();
    }
}
