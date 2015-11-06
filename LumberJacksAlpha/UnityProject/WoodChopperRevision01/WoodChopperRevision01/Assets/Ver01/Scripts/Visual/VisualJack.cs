using UnityEngine;
using System.Collections;
using System;

public class VisualJack : MonoBehaviour {
    int x, y;
    private PLAY player;

    public void Init(PLAY eR1, int x, int y)
    {
        player = eR1;
        this.x = x;
        this.y = y;
    }

    public void Move(int x, int y)
    {
        StartCoroutine(_Move(x, y));
    }

    private IEnumerator _Move(int x, int y)
    {
        var from = transform.position;
        var to = VisualPlayground.self.Pos(x, y);
        float timer = 0;


        // animation time should be the same time as delay time on server 
        // need additional class to maintain this!!
        while ( timer < 1f)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(from, to, timer / 1f);
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
        throw new NotImplementedException();
    }

    public void Chop(int x, int y)
    {
        // animate 
        throw new NotImplementedException();
    }
}
