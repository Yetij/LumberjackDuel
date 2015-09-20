using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class p2ButtonCellSelect : p2ButtonSelected {
	
	[HideInInspector] public Toggle toggle;
	void Awake () {
		toggle = GetComponent<Toggle>();
	}
	public void Set(p2PregameTreeInfo t ) {
		base.Set(t);
		Debug.Log("p2ButtonCellSelect ");
		if ( t == null ) {
			toggle.interactable = false;
			return;
		}
	}
}