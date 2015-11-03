using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Playground : Photon.PunBehaviour {

    public Cell cellPrefab;
    int gridX, gridY;
    float offsetX, offsetY;

    Cell[,] cells;

    public void Init(int _gridX, int _gridY, float _offsetX, float _offsetY)
    {
        cells = new Cell[_gridX,_gridY];
        for (int y = 0; y < _gridY; y++)
        {
            for (int x = 0; x < _gridX; x++)
            {
                Cell c = Instantiate<Cell>(cellPrefab);
                c.transform.SetParent(transform);
                c.Init(x, y, _offsetX, _offsetY);
                cells[x, y] = c;
            }
        }

        transform.Translate(new Vector3(-(_gridX - 1) * _offsetY / 2, -(_gridY - 1) * _offsetY / 2));
        gridX = _gridX;
        gridY = _gridY;
        offsetX = _offsetX;
        offsetY = _offsetY;
    }

    public Cell GetCellAtPos(Vector2 pos)
    {
        var delta = pos - (Vector2) transform.position;
        int x = (int) (delta.x / offsetX);
        int y = (int) (delta.y / offsetY);

        Debug.Log("getting cell at " + x + "," + y);
        if ( Valid(x, y) )
        {
            return cells[x, y];
        }
        else return null;
    }

    public Cell GetCellAtIndex(int x, int y)
    {
        if (x >= 0 & x < gridX & y >= 0 & y < gridY)
        {
            return cells[x, y];
        }
        else throw new UnityException("invalid index, weird");
    }

    public int GetPLantCost(Lumberjack p , TreeType type)
    {
        return 1;
    }

    public AbsTree GetTree(TreeType type)
    {
        throw new NotImplementedException();
    }

    public int GetMoveCost(Lumberjack p)
    {
        int move_cost = 1;
        int bonus_move_cost = 0;
        foreach ( var r in Server.self.availableTrees )
        {
            r.MoveCost(p, ref bonus_move_cost);
        }
        move_cost += bonus_move_cost;
        if (move_cost < 0) move_cost = 0;
        return move_cost;
    }

    public int GetChopCost(Lumberjack p,int cx, int cy)
    {
        int chop_cost = 1;
        int bonus_chop_cost = 0;
        foreach (var r in Server.self.availableTrees)
        {
            r.ChopCost(p, ref bonus_chop_cost);
        }
        chop_cost += bonus_chop_cost;

        cells[cx, cy].tree.DirectChopCost(ref chop_cost);
        if (chop_cost < 0) chop_cost = 0;
        return chop_cost;
    }

    internal bool Valid(int x, int y)
    {
        return x >= 0 & x < gridX & y >= 0 & y < gridY;
    }
}
