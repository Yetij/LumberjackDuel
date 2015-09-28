using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public class InfoBarControl {
	public Text timer;
	public Text ac;
	public Text myHp;
	
	public Text myName;
	public Text myPoints;
	public Text opponentHp;
	public Text opponentName;
	public Text opponentPoints;
}

public class p3UiIngame : MonoBehaviour,  IControlable
{
	public Toggle currentSelected { get; private set; }
	[SerializeField] Toggle[] treeButtons;
	public p2EndGamePanelV2 endGamePanel;

	[SerializeField] p2Dialog visualLog;
	public InfoBarControl inforbar;

	p3TouchInput input;

	IInputListener listener;

	void Awake () {
		this.enabled = false;
	}

	public void AddListener ( IInputListener listener ) {
		this.listener = listener;
	}
	
	public void Reset () {
		if ( currentSelected != null ) currentSelected.isOn = false;
	}

	void Update () {
		if ( _run ) {
			input._Update(listener);
		}
	}
	
	public void DisplayDialog (string s ) {
		visualLog.PopUp(s);
	}

	bool _run;
	bool _runRequested;
	public void Run () {
		_runRequested = true;
		TryRunning();
	}

	void TryRunning() {
		if ( _runRequested & sceneSetUpDone ) {
			p3Scene.Instance.Initialize();
			p3Scene.Instance.Run();
			_run = true;
		}
	}

	public void Stop () {
		_run = false;
	}

	public void SetUp () {
		if ( PhotonNetwork.isMasterClient ) {
			PhotonNetwork.InstantiateSceneObject(p3Names.Instance.prefabs.scene.name,Vector3.zero,Quaternion.identity,0,null);
		}
		p3Map.Instance.SetUp();
		input = new p3TouchInput();
		sceneLoaded = false;
		playerMineLoaded = false;
		playerNotMineLoaded = false;
		sceneSetUpDone = false;

		PhotonNetwork.Instantiate(p3Names.Instance.prefabs.player.name,Vector3.zero,Quaternion.identity,0,null);
	}

	public void Initialize() {
		if ( treeButtons.Length != 0 ) {
			foreach ( var t in treeButtons ) {
				SetupButton(t);
			}
		}
		_runRequested = false;
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

	public void OnSkipButtonClicked () {
		listener.OnSkipButtonClicked();
	}

	bool sceneLoaded, sceneSetUpDone;
	public void OnSceneLoaded () {
		sceneLoaded = true;
		TrySettingUpScene();
	}

	bool playerMineLoaded,playerNotMineLoaded;
	public void OnPlayerLoaded (p3Player player) {
		if ( player.photonView.isMine ) playerMineLoaded = true;
		else playerNotMineLoaded = true;
		TrySettingUpScene();
	}

	void TrySettingUpScene () {
		if ( sceneLoaded & playerMineLoaded & playerNotMineLoaded ) {
			p3Scene.Instance.SetUp();
		}
	}

	public void OnSceneSetUpDone () {
		sceneSetUpDone = true;
		TryRunning();
	}

	public void SetColor (Color r ) {
		inforbar.timer.color = r;
		inforbar.ac.color = r;
	}
}

