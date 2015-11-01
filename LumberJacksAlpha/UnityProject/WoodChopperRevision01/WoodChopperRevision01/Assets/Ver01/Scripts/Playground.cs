using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Playground : Photon.PunBehaviour {

    public Cell cellPrefab;

    public void Init(int gridX, int gridY, float offsetX, float offsetY)
    {
        List<Cell> cells = new List<Cell>(gridX * gridY);
        for (int y = 0; y < gridY; y++)
        {
            for (int x = 0; x < gridX; x++)
            {
                Cell c = Instantiate<Cell>(cellPrefab);
                c.transform.SetParent(transform);
                c.Init(x, y, offsetX, offsetY);
                cells.Add(c);
            }
        }
        transform.Translate(new Vector3(-(gridX - 1) * offsetY / 2, -(gridY - 1) * offsetY / 2));
    }

    internal Cell GetCellAt(Vector2 pos)
    {
        throw new NotImplementedException();
    }

    internal Cell GetCellAt(int cx, int cy)
    {
        throw new NotImplementedException();
    }

    internal int GetPLantCost(Lumberjack p , TreeType type)
    {
        throw new NotImplementedException();
    }

    internal AbsTree GetTree(TreeType type)
    {
        throw new NotImplementedException();
    }

    internal int GetMoveCost(Lumberjack p)
    {
        throw new NotImplementedException();
    }

    internal int GetChopCost(Lumberjack p,int cx, int cy)
    {
        throw new NotImplementedException();
    }
}
