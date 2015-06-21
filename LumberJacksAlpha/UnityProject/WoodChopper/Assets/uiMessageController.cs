using UnityEngine;
using System.Collections;

public class uiMessageController 
{
	static bool _isInitialized = false;
	static GameObject panel;

	static uiMessage current;

	

	public void Update(float delta ) {
		if ( current != null ) current.Update(delta);
	}
}

