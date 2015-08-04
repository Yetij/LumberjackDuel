using UnityEngine;
using UnityEngine.UI;
public class p2Gui : MonoBehaviour
{
	private static p2Gui _instance;
	public static p2Gui Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p2Gui)) as p2Gui;
				if ( _instance == null ) throw new UnityException("Object of type p2Gui not found");
			}
			return _instance;
		}
	}

	public Toggle[] treeButtons;
	
	void Awake () {
		if ( treeButtons.Length != 0 ) {
			foreach ( var t in treeButtons ) {
				SetupButton(t);
			}
		}
	}

	AbsGuiListener listener;

	public void AddListener ( AbsGuiListener listener ) {
		this.listener = listener;
	}

	void SetupButton ( Toggle b ) {
		b.onValueChanged.AddListener((bool v) => { TreeSelected(v,b); });
	}
	
	void TreeSelected (bool value, Toggle source ) {
		p2GuiTree uiSelectedTree = null;
		if( value ) {
			uiSelectedTree = source.GetComponent<p2GuiTree>();
			Debug.Log("Selected tree = " + source);
		} 

		listener.OnTreeSelected(uiSelectedTree);
	}

	bool _isInControlZone;

	public void MouseEnter (bool s) {
		_isInControlZone = s;
	}

	public bool IsInControlZone() {
		return _isInControlZone;
	}
}

