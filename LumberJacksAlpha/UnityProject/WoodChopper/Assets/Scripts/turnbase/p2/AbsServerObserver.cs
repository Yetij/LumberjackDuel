using UnityEngine;
using System.Collections;

public interface AbsServerObserver 
{
	void OnTurnStart ( int turn_nb );
	void OnBackgroundStart ( );
}

