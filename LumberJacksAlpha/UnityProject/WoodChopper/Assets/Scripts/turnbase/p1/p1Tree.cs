using UnityEngine;
using System.Collections;

public class p1Tree : MonoBehaviour
{
	p1TreeCore core;

	bool isGrown;

	public p1Cell cell;

	public void OnBeingPlanted (p1TreeType type ) {
		core = GetTree ( type );
	}

	static p1TreeCore GetTree ( p1TreeType type ) {
		switch (type ) {
		default : 
			throw new UnityException ("Unsupported tree type ");
			break;
		}
	}

	public void OnBeingTakenDown( int fx, int fz , p1SourceDmg source ) {
	}

	public void OnBackgroundProcess ( int turn ) {
	}

	public bool IsPassable () {
		return isGrown;
	}

}

