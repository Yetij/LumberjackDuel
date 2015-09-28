using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using StaticStructure;

public class p2ButtonSelected : MonoBehaviour {

	public Text treeName;
	public Text ac;
	[HideInInspector] public p2PregameTreeInfo info;

	virtual public void Set(p2PregameTreeInfo t ) {
		if ( t == null ) {
			Debug.Log(gameObject.name);
			treeName.text = "";
			ac.text = "";
			info = null;
			return;
		}
		info = t;
		var k =  t.realTree.type;
		switch(k ) {
		case TreeType.Basic:
		case TreeType.BonusAc:
		case TreeType.Ethereal:
		case TreeType.Monumental:
		case TreeType.Swamp:
			treeName.text = k.ToString() + " Tree";
			ac.text = "AC: " +t.realTree.plantCost;
			break;
		case TreeType.Stone:
			treeName.text = k.ToString();
			ac.text = "";
			break;
		default:
			throw new UnityException("Invalid tree type = " + k);
		}
	}
}
