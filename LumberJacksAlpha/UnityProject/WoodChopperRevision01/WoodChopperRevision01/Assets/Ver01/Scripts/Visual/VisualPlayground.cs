using UnityEngine;
using System.Collections;
using System;

public class VisualPlayground : MonoBehaviour {
    private int gridX;
    private int gridY;
    private float offsetX;
    private float offsetY;

    static public VisualPlayground self;

    void Awake ()
    {
        if ( self == null )
        {
            self = this;
        } else
        {
            throw new UnityException("object Playground must be unique");
        }
    }
    public void Init(int gridX, int gridY, float offsetX, float offsetY)
    {
        this.gridX = gridX;
        this.gridY = gridY;
        this.offsetX = offsetX;
        this.offsetY = offsetY;
        // create cells
    }

    public Vector3 Pos(int x, int y)
    {
        throw new NotImplementedException();
    }

    public VisualCell GetCellAtIndex(int v1, int v2)
    {
        throw new NotImplementedException();
    }

    public void PlaceTree(VisualTree tree, int x, int y)
    {
        throw new NotImplementedException();
    }

    public bool ValidPos(Vector2 pos, out int x, out int y)
    {
        throw new NotImplementedException();
    }

    public void ValidPos(Vector2 pos)
    {
        throw new NotImplementedException();
    }

    public void ChopTree(int x, int y)
    {
        throw new NotImplementedException();
    }
}
