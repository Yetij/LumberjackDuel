using UnityEngine;
using System.Collections;
using System;

public class VisualTree : MonoBehaviour {
    public TreeType type;
    public int x { get; private set;  }
    public int y { get; private set;  }

    public void BeingChoped(VisualJack chopper)
    {
        int dirx = x - chopper.x;
        int dirt = y - chopper.y;

        Destroy(gameObject);
    }

    public void Init(Vector3 pos, int _x, int _y)
    {
        transform.position = pos;
        x = _x;
        y = _y;
    }
}
