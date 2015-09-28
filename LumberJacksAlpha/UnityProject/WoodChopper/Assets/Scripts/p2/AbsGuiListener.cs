using UnityEngine;
using System.Collections;

public interface AbsGuiListener 
{
	void OnTreeSelected (p2GuiTree t);
	p2Player Owner ();
}

