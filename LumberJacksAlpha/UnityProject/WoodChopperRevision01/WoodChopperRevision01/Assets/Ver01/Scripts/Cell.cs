using UnityEngine;
using System.Collections;
using System;

public class Cell : MonoBehaviour {
    internal Lumberjack character;
    internal AbsTree tree;
    public int x
    {
        get; private set;
    }
    public int y
    {
        get; private set;
    }
	public void Init ( int x, int y, float offsetx, float offsety )
    {
        this.x = x;
        this.y = y;
        transform.position = new Vector3(x * offsetx,  y * offsety);
    }

    public void VisualAddTree(AbsTree v)
    {
        throw new NotImplementedException();
    }
}
