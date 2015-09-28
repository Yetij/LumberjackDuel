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

	public Camera cam1;
	public Camera cam2;

	public p2PregamePanel pregamePanel;
	public p2IngamePanel ingamePanel;
	public p2ConnectorUI connectorUI;

	public p2EndGamePanelV2 endGamePanel;

	[SerializeField] p2Dialog visualLog;

	public Button skip;

	public Text timer;
	public Text ac;
	public Text myHp;
	public Text myName;
	public Text myPoints;
	public Text opponentHp;
	public Text opponentName;
	public Text opponentPoints;
	
	AbsGuiListener listener;

	[SerializeField] Toggle[] treeButtons;
	public Toggle currentSelected { get; private set; }

	public void PanelPreToIn () {
		pregamePanel.gameObject.SetActive(false);
		p2Map.Instance.gameObject.SetActive(true);
		ingamePanel.gameObject.SetActive(true);
	}

	public void PanelInToPre () {
		pregamePanel.gameObject.SetActive(true);
		p2Map.Instance.gameObject.SetActive(false);
		ingamePanel.gameObject.SetActive(false);
	}

	void Awake () {
		if ( treeButtons.Length != 0 ) {
			foreach ( var t in treeButtons ) {
				SetupButton(t);
			}
		}
	}

	public void Reset () {
		if ( currentSelected != null ) currentSelected.isOn = false;
	}


	public void AddListener ( AbsGuiListener listener ) {
		this.listener = listener;
	}

	
	void SetupButton ( Toggle b ) {
		b.onValueChanged.AddListener((bool v) => { TreeSelected(v,b); });
	}

	void TreeSelected (bool value, Toggle source ) {
		p2GuiTree selected = null;
		if( value ) {
			currentSelected = source;
			selected = source.GetComponent<p2GuiTree>();
			DisplayDialog(p2TreePool.Instance.GetTreePlantLog(selected.type));
		} else {
			currentSelected = null;
			selected = null;
		}

		listener.OnTreeSelected(selected);
	}

	public void DisplayDialog (string s ) {
		visualLog.PopUp(s);
	}

	bool _isInControlZone;

	public void MouseEnter (bool s) {
		_isInControlZone = s;
	}

	public bool IsInControlZone() {
		return _isInControlZone;
	}

	public void SetColor (Color r ) {
		timer.color = r;
		ac.color = r;
	}

	public void OnSkipButtonClicked () {
		listener.Owner().SkipTurn();
	}
}

