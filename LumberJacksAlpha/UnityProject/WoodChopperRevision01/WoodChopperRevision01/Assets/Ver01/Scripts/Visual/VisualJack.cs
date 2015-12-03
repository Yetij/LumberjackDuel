using UnityEngine;
using System.Collections;
using System;

public class VisualJack : MonoBehaviour {
    public int x, y;
    private PLAY player;

    VisualPlayground playground; 
    
    void Start ()
    {
        playground = GameObject.FindObjectOfType<VisualPlayground>();
        if (playground == null) throw new UnityException("cant find (visual) Playground");
    }

    public void Init(PLAY eR1, int x, int y, Vector3 pos)
    {
        player = eR1;
        this.x = x;
        this.y = y;
        transform.position = pos;
    }

    public void Move(int x, int y)
    {
        StartCoroutine(_Move(x, y));
    }

    private IEnumerator _Move(int x, int y)
    {
        var from = transform.position;
        var to = playground.Pos(x, y);
        float timer = 0;

        while ( timer < Definitions.visual_move_time)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, timer / Definitions.visual_move_time);
            yield return null;
        }

        transform.position = to;
        this.x = x;
        this.y = y;
    }

    public void Chop(VisualJack visualJack)
    {
        Chop(visualJack.x, visualJack.y);
    }

    public void BeingChop(VisualJack visualJack)
    {
        Debug.LogError("NotImplementedException");
    }

    public void Chop(int x, int y)
    {
        // animate 
        Debug.Log("CLIENT player chop");
    }
}
