using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class GuiInputAdapter : MonoBehaviour
{
	private static GuiInputAdapter _instance;
	public static GuiInputAdapter Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(GuiInputAdapter)) as GuiInputAdapter;
				if ( _instance == null ) throw new UnityException("Object of type GuiInputAdapter not found");
			}
			return _instance;
		}
	}
	public Toggle uiChopButton;
	public Toggle[] treeButtons;
	public p1ConfirmButton uiConfirmButton;

	public Button uiCancel;

	Toggle uiSelectedTree;
	
	bool cancelFlag;
	
	void UiCancelEvent () {
		cancelFlag =  true;
		/* hide button */
	}


	void Awake () {
		//Debug.Log( " uiChopButton == null ? " + (uiChopButton == null));
		if ( uiChopButton != null ) uiChopButton.onValueChanged.AddListener((bool v) => { TreeSelected(v,uiChopButton); } );
		if ( treeButtons.Length != 0 ) {
			foreach ( var t in treeButtons ) {
				SetupButton(t);
			}
		}
	}

	void SetupButton ( Toggle b ) {
		b.onValueChanged.AddListener((bool v) => { TreeSelected(v,b); });
	}

	bool hasInput;
	bool isApproved;
	bool isCanceled;

	public bool IsCanceled () {
		return isCanceled;
	}

	public bool IsApproved () {
		return hasInput & isApproved;
	}

	public bool HashChopSelected () {
		return uiChopButton.isOn;
	}

	public bool HasTreeSelected () {
		return uiSelectedTree != null;
	}

	public void FlushCancel () {
		isCanceled = false;
	}
	public void FlushChopButton () {
		uiChopButton.isOn = false;
	}

	public void FlushTree () {
		uiSelectedTree.isOn = false;
		uiSelectedTree = null;
	}

	void TreeSelected (bool value, Toggle source ) {
		if( value ) {
			uiSelectedTree = source;
		} else {
			uiSelectedTree = null;
		}
	}
	public void Hha ( bool value, string source ) {
		Debug.Log("value = " + value  + " source="+source );
	}
}

