using UnityEngine;
using System.Collections;

public interface IInputListener 
{
	void OnTreeSelected (p2GuiTree t);
	void OnSkipButtonClicked ();
	void OnDrag (Vector2 current, Vector2 delta);
	void OnTouch (Vector2 current );
}

