using UnityEngine;
using System.Collections;
using System;

public class VisualTree : MonoBehaviour {
    public TreeType type;
    public Vector3 offset;
    public int x { get; private set;  }
    public int y { get; private set;  }

    [SerializeField]
    Sprite icon;
    public Sprite Icon
    {
        get { return icon; }
        set { icon = value; }
    }
    static int Clamp_101 (int h)
    {
        if (h == 0) return 0;
        if (h >= 1) return 1;
        if (h <= -1) return -1;
        throw new UnityException("Code should not reach here, h=" + h);
    }
    public void BeingChoped(VisualJack chopper)
    {
        int dirx = Clamp_101(x - chopper.x);
        int diry = Clamp_101(y - chopper.y);

        StartCoroutine(BeingChoped2(dirx, diry));
    }

    private IEnumerator BeingChoped2(int x, int y)
    {
        var animator = GetComponent<Animator>();
        if (animator == null) { 
            Debug.LogWarning("Component Animator not found");
        }
        else
        {
            animator.Play(x.ToString() + y.ToString());
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }
    }

    bool PairEquals ( int x1, int y1, int x2, int y2)
    {
        return x1 == x2 & y1 == y2;
    }
    public void Init(Vector3 pos, int _x, int _y)
    {
        transform.position = pos + offset;
        x = _x;
        y = _y;
    }
}
