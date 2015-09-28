using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class p3UiPregame : Photon.PunBehaviour, IControlable
{
	public p3ButtonSelected[] selectedButtons;
	[SerializeField] p3ButtonCellSelect[] toggleButtons;
	[SerializeField] Camera previewCam1,previewCam2;

	p3PregameTreeInfo[] treeInfo;


	public void SetUp () {
		treeInfo = GameObject.FindObjectsOfType(typeof(p3PregameTreeInfo)) as p3PregameTreeInfo[];	
		
		if ( treeInfo.Length > toggleButtons.Length ) throw new UnityException("too many tree info");

		int i=0;

		foreach ( var t in treeInfo ) {
			if ( t.realTree.canBePlantedByPlayer ) {
				toggleButtons[i++].Set(t);
			}
		}
		foreach ( var t in treeInfo ) {
			if ( !t.realTree.canBePlantedByPlayer ) toggleButtons[i++].Set(t);
			t.gameObject.SetActive(false);
		}

		while ( i < toggleButtons.Length) {
			toggleButtons[i++].toggle.interactable = false;
		}

		foreach ( var t in selectedButtons ) {
			t.Set(null);
		}

		previewCam1.gameObject.SetActive(false);
		previewCam2.gameObject.SetActive(false);
	}
	
	public void Initialize () {
		gameObject.SetActive(true);
		previewCam1.gameObject.SetActive(true);
		previewCam2.gameObject.SetActive(true);
	}

	public void Run () {
	}

	public void Stop () {
		previewCam1.gameObject.SetActive(false);
		previewCam2.gameObject.SetActive(false);
		gameObject.SetActive(false);
	}

	bool masterReady, nonMasterReady;

	public void StartClicked (Button startbutton) {
		startbutton.interactable = false;
		startbutton.transform.GetChild(0).GetComponent<Text>().text = "Processing";
		if ( PhotonNetwork.isMasterClient ) {
			masterReady = true;
			if ( nonMasterReady ) {
				photonView.RPC("OnAllStart", PhotonTargets.All);
			}
		} else {
			photonView.RPC("OnNonMasterStartClicked",PhotonTargets.MasterClient);
		}
	}

	[PunRPC] void OnNonMasterStartClicked() {
		nonMasterReady = true;
		if ( masterReady ) {
			photonView.RPC("OnAllStart", PhotonTargets.All);
		}
	}
	
	[PunRPC] void OnAllStart() {
		Debug.Log("pregam OnAllStart");
		masterReady = false;
		nonMasterReady = false;	

		Stop();

		p3Ui.Instance.ingamePanel.Initialize();
		
		p3Ui.Instance.ingamePanel.Run();
	}

	[SerializeField] Text detailText;
	p3ButtonCellSelect currentViewed;
	int lastSelected=0;
	public void OnTogglePressed (p3ButtonCellSelect t ) {
		if( t.toggle.isOn ) { 
			if ( currentViewed != null ) {
				currentViewed.info.gameObject.SetActive(false);
			}
			currentViewed = t;
			detailText.text = t.info.description.Replace('\\','\n');
			t.info.gameObject.SetActive(true);
			if ( t.info.realTree.canBePlantedByPlayer ) {
				foreach ( var b in selectedButtons ) {
					if ( b.info == t.info ) return;
				}
				selectedButtons[lastSelected++ % selectedButtons.Length].Set(t.info);
			}
		} else {
			if ( t == currentViewed  ) {
				t.info.gameObject.SetActive(false);
				detailText.text = "";
			}
		}
	}
}

