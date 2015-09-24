using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class p2ConnectorUI : Photon.PunBehaviour {

	public Text log;
	public Button play;
	public Button quit;
	Text playText;
	Text quitText;
	[SerializeField] float buttonsFadeTime;

	void Start () {
		playText = play.transform.GetChild(0).GetComponent<Text>();
		quitText = quit.transform.GetChild(0).GetComponent<Text>();

		masterReady = false;
		nonMasterReady = false;

	}
	public void PlayButtonClicked () {
		StartCoroutine(PlayButtonClicked2());
	}

	IEnumerator PlayButtonClicked2 () {
		float t = buttonsFadeTime;
		while ( t >= 0 ) {
			t -= Time.deltaTime;
			var r = playText.color;
			r.a = t/buttonsFadeTime;
			playText.color = r;
			quitText.color = r;
			yield return null;
		}
		play.gameObject.SetActive(false);
		quit.gameObject.SetActive(false);
		log.gameObject.SetActive(true);

		PhotonNetwork.autoJoinLobby = false;    
		PhotonNetwork.playerName = NameMap.get["defaultPlayerName"] + Random.Range(0,1000);
		PhotonNetwork.ConnectUsingSettings(NameMap.get["connectSettingString"]);
	}

	public void QuitButtonClicked () {
	}

	public void Log ( string s ) {	
		log.text = s;
		Debug.Log(s);
	}

	bool masterReady, nonMasterReady;

	//-------------------- events ------------------------
	[PunRPC] void OnNonMasterNetControlObjectInitialized () {
		photonView.RPC("LoadStuffs", PhotonTargets.All);
	}
	
	[PunRPC] void LoadStuffs () {
		p2Map.Instance.StartCustom();
		p2Map.Instance.gameObject.SetActive(false);
		if ( PhotonNetwork.isMasterClient ) {
			masterReady = true;
			if ( nonMasterReady ) {
				masterReady = false;
				nonMasterReady = false;
				photonView.RPC("OnConnectStageFinished",PhotonTargets.All);
			}
		} else {
			photonView.RPC("OnNonMasterLoadStuffsDone",PhotonTargets.MasterClient);
		}
	}

	[PunRPC] void OnNonMasterLoadStuffsDone() {
		nonMasterReady = true;
		if ( masterReady ) {
			masterReady = false;
			nonMasterReady = false;
			photonView.RPC("OnConnectStageFinished",PhotonTargets.All);
		}
	}

	[PunRPC] void OnConnectStageFinished () {
		StartCoroutine(OnConnectStageFinished2());
	}

	IEnumerator OnConnectStageFinished2 () {
		Log("All players ready ... 3");
		yield return new WaitForSeconds(1);
		Log("All players ready ... 2");
		yield return new WaitForSeconds(1);
		Log("All players ready ... 1");
		yield return new WaitForSeconds(1);
		Log("All players ready ... 0");
		yield return new WaitForSeconds(0.3f);
		p2Gui.Instance.connectorUI.gameObject.SetActive(false);
		p2Gui.Instance.pregamePanel.gameObject.SetActive(true);
	}
	//---------------- photon events --------------------

	public override void OnConnectedToMaster ()
	{
		base.OnConnectedToMaster ();
		Log("Connected to master");
		PhotonNetwork.JoinOrCreateRoom(	
           	NameMap.get["defaultRoomName"],
           	new RoomOptions() { maxPlayers = 2 },
			new TypedLobby(NameMap.get["defaultLobbyName"],LobbyType.Default)
		);
		Log("Trying joinning room");
	}

	public override void OnConnectedToPhoton ()
	{
		base.OnConnectedToPhoton ();
		Log("Connected to server");

	}

	public override void OnConnectionFail (DisconnectCause cause)
	{
		base.OnConnectionFail (cause);
		Log("OnConnectionFail");
	}

	public override void OnCreatedRoom ()
	{
		base.OnCreatedRoom ();
		Log("OnCreatedRoom");
	}

	public override void OnFailedToConnectToPhoton (DisconnectCause cause)
	{
		base.OnFailedToConnectToPhoton (cause);
		Log("OnFailedToConnectToPhoton");
	}

	public override void OnJoinedRoom ()
	{
		base.OnJoinedRoom ();
	
		Log("Joined room");
		if( PhotonNetwork.isMasterClient ) {
			Debug.Log("netcontrol object = " + NameMap.get["netControlObjectPrefabName"]);
			PhotonNetwork.InstantiateSceneObject(NameMap.get["netControlObjectPrefabName"],Vector3.zero,Quaternion.identity,0,null);
			Log("Initializing room");
		} else {
			Log("Processing to join room");
		}

	}

	public override void OnPhotonPlayerConnected (PhotonPlayer newPlayer)
	{
		base.OnPhotonPlayerConnected (newPlayer);
		Log("OnPhotonPlayerConnected");
	}

}
