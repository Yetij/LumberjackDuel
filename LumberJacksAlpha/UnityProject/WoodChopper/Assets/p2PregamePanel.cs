using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class p2PregamePanel : MonoBehaviour {
	public Text detailTextDescription;

	[SerializeField] p2ButtonSelected[] selected;

	p2ButtonCellSelect[] buttons;
	p2PregameTreeInfo[] treeInfo;

	void Awake () {
		buttons = GameObject.FindObjectsOfType(typeof(p2ButtonCellSelect)) as p2ButtonCellSelect[];	
		treeInfo = GameObject.FindObjectsOfType(typeof(p2PregameTreeInfo)) as p2PregameTreeInfo[];	

		if ( treeInfo.Length > buttons.Length ) throw new UnityException("too many tree info");
		int i=0;
		foreach ( var t in treeInfo ) {
			if ( t.realTree.canBePlantedByPlayer ) buttons[i++].Set(t);
		}
		foreach ( var t in treeInfo ) {
			if ( !t.realTree.canBePlantedByPlayer )  buttons[i++].Set(t);
			t.gameObject.SetActive(false);
		}
		while ( i < buttons.Length ) {
			buttons[i++].toggle.interactable = false;
		}
		foreach ( var t in selected ) {
			t.Set(null);
		}
	}

	p2ButtonCellSelect currentViewed;
	int lastSelected=0;
	public void TogglePressed (p2ButtonCellSelect t ) {
		if( t.toggle.isOn ) { 
			if ( currentViewed != null ) {
				currentViewed.info.gameObject.SetActive(false);
			}
			currentViewed = t;
			detailTextDescription.text = t.info.description.Replace('\\','\n');
			t.info.gameObject.SetActive(true);
			if ( t.info.realTree.canBePlantedByPlayer ) {
				foreach ( var b in selected ) {
					if ( b.info == t.info ) return;
				}
				selected[lastSelected++ % selected.Length].Set(t.info);
			}
		} else {
			if ( t == currentViewed  ) {
				t.info.gameObject.SetActive(false);
				detailTextDescription.text = "";
			}
		}
	}
}
