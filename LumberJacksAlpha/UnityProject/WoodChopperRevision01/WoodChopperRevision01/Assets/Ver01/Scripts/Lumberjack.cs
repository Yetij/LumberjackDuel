using UnityEngine;
using System.Collections;
using System;

public enum PLAY : int {  ER1 = 0 , ER2 = 1 }
[RequireComponent(typeof(PhotonView))]
public class Lumberjack : Photon.MonoBehaviour {
    public Cell currentCell { get; private set; }
    public PLAY player;
    public LumberJackParams parameters;

	float move_to_time = 1f;

    public void Chop (Cell target )
    {

    }

    public void Plant (Cell target )
    {

    }

	public void VisualBeingChoped(PLAY player)
    {
        throw new NotImplementedException();
    }

	public void VisualChop(Cell c)
	{
		// start animation
        throw new NotImplementedException();
    }

	public void VisualPlant(Cell c)
    {
		// start animation
        throw new NotImplementedException();
    }

	public void VisualMoveTo(Cell c)
    {
		StartCoroutine (VisualMoveTo2 (c));
	}
		
	IEnumerator VisualMoveTo2 (Cell target) {
		// start animation
		Server.self.ServerPause();
		float timer = 0;
		var from = currentCell.transform.position;
		var to = target.transform.position;
		while (true) {
			yield return null;
			timer += Time.deltaTime;
			transform.position = Vector3.Lerp (from, to, timer/move_to_time );
			if ( timer >= move_to_time ) {
				transform.position = to;
				currentCell = target;
				Server.self.ServerUnPause();
				break;
			}
		}
	}
} 
