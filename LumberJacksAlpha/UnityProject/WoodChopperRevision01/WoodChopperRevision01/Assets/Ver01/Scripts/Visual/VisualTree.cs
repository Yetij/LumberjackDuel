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
        int diry = y - chopper.y;

        StartCoroutine(BeingChoped2(dirx, diry));
    }

    private IEnumerator BeingChoped2(int dirx, int diry)
    {
        
        var c = GetComponent<Animator>();
        if (c) c.Play("fall_01");
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    public void Init(Vector3 pos, int _x, int _y)
    {
        transform.position = pos;
        x = _x;
        y = _y;
    }
}
