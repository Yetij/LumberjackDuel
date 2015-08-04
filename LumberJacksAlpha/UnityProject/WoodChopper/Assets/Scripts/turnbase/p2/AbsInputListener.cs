using UnityEngine;
using System.Collections;

public interface AbsInputListener
{
	void OnApprove ();
	void OnCancel ();
	void OnControlZoneTouchMove( Vector2 delta );
	void OnMapZoneTouchMove( Vector2 pos );
	void OnMapZoneTap( Vector2 pos );
}

