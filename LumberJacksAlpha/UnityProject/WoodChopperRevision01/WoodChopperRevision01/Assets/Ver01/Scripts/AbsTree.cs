using System;
using System.Collections;
using UnityEngine;

public class AbsTree : MonoBehaviour
{
    public Cell cell;

    public void VisualBeingChoped(PLAY player)
    {
        
    }

    public void OnTurnChange()
    {
        
    }

    public void Activate ()
    {
        gameObject.SetActive(true);
        transform.rotation = Quaternion.identity;
        Server.self.availableTrees.Add(this);
    }

    private void DeactivateVisual ()
    {
        gameObject.SetActive(false);
    }
    private void DeactivateLogic()
    {
        Server.self.availableTrees.Remove(this);
        cell = null;
    }

    virtual public void MoveCost(Lumberjack p, ref int bonus_move_cost)
    {
        
    }

    virtual public void ChopCost(Lumberjack p, ref int bonus_chop_cost)
    {
        
    }

    virtual public void DirectChopCost(ref int chop_cost)
    {
    }

    virtual public void BeingChoped(Lumberjack p, int tier)
    {

        DeactivateLogic();

        if (PassChop())
        {
            var dirx = cell.x - p.currentCell.x;
            var diry = cell.y - p.currentCell.y;
            Cell c = Server.self.playground.GetCellAtIndex(cell.x + dirx, cell.y + diry);
            if (c != null)
            {
                if (c.tree != null)
                {
                    c.tree.BeingChoped(p, tier + 1);
                    return;
                }
            }
        }
        p.GainCredit(tier);
    }

    private bool PassChop()
    {
        return true;
    }

    virtual public void VisualFall()
    {
        Server.self.Pause();
        StartCoroutine(VisualFall2());
    }

    private IEnumerator VisualFall2()
    {
        float time = 1f;
        while ( time > 0 )
        {
            yield return null;
            time -= Time.deltaTime;
            transform.Rotate(transform.up, 360 * Time.deltaTime);
        }
        DeactivateVisual();
        Server.self.Unpause();
    }
}