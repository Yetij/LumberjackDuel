using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class p3ButtonCellSelect : p3ButtonSelected {
	
	[HideInInspector] public Toggle toggle;
	void Awake () {
		toggle = GetComponent<Toggle>();
	}
	override public void Set(p3PregameTreeInfo t ) {
		base.Set(t);
		if ( t == null ) {
			toggle.interactable = false;
			return;
		}
	}
}