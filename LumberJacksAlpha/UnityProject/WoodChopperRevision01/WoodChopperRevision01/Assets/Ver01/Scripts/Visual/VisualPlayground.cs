using UnityEngine;
using System.Collections;
using System;

public class VisualPlayground : MonoBehaviour {
    private int gridX;
    private int gridY;
    private float offsetX;
    private float offsetY;

    static public VisualPlayground self;
    private VisualCell[,] cells;
    private VisualTree[,] trees;

    [SerializeField]
    VisualCell cellPrefab;
    [SerializeField]
    VisualTree[] treeSeeds;

    public void Init(int _gridX, int _gridY, float _offsetX, float _offsetY)
    {
        gridX = _gridX;
        gridY = _gridY;
        offsetX = _offsetX;
        offsetY = _offsetY;

        cells = new VisualCell[_gridX, _gridY];
        trees = new VisualTree[_gridX, _gridY];
        for (int y = 0; y < _gridY; y++)
        {
            for (int x = 0; x < _gridX; x++)
            {
                VisualCell c = Instantiate<VisualCell>(cellPrefab);
                c.transform.SetParent(transform);
                c.transform.position = new Vector3( x * offsetX, y * offsetY , 0 );
                cells[x, y] = c;
            }
        }

        transform.Translate(new Vector3(-(_gridX - 1) * _offsetY / 2, -(_gridY - 1) * _offsetY / 2));
        
    }

    public Vector3 Pos(int x, int y)
    {
        var r = transform.position;
        return new Vector3(x * offsetX + r.x, y * offsetY + r.y, 0);
    }

    public VisualCell GetCellAtIndex(int x, int y)
    {
        if (ValidIndex(x, y)) return cells[x, y];
        return null;
    }

    public bool ValidPos(Vector2 pos, out int _x, out int _y)
    {
        var position = transform.position;
        var _fx = pos.x - position.x;
        var _fy = pos.y - position.y;
        Debug.Log("click pos = " + pos);
        Debug.Log("pos = " + position);
        
        if ( _fx < 0 | _fy < 0 | _fx >= gridX*offsetX | _fy >= gridY*offsetY )
        {
            _x = _y = -1;
            return false;
        }

        _x = (int)(_fx/offsetX);
        _y = (int)(_fy/offsetY);
        Debug.Log("result = " + new Int2(_x,_y));


        return true;
    }

    bool ValidIndex(int x, int y)
    {
        return x >= 0 & x < gridX & y >= 0 & y < gridY;
    }

    public void ChopTree(VisualJack chopper, int x, int y)
    {
        if (!ValidIndex(x, y)) throw new UnityException("wtf ? x,y= " + x + "," + y);
        if ( trees[x,y] == null ) throw new UnityException("wtf no treed? x,y= " + x + "," + y);
        trees[x, y].BeingChoped(chopper);
        trees[x, y] = null;
    }

    public void PlaceTree(TreeType tree_type, int x, int y, Growth tree_growth)
    {
        if (!ValidIndex(x, y) ) throw new UnityException("wtf ? x,y= " + x+","+y);

        VisualTree tree = null;
        foreach (var s in treeSeeds)
        {
            if (s.type == tree_type)
            {
                tree = Instantiate<VisualTree>(s);
            }
        }

        if (tree == null) throw new UnityException("wtf ? type= " + tree_type);

        trees[x, y] = tree;
        tree.transform.SetParent(transform);
        tree.Init(Pos(x, y), x, y);
        // tree set growth
    }
}
