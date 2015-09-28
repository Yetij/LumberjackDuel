using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class p3UiConnector : Photon.PunBehaviour {
	
	[SerializeField]  Text log;
	[SerializeField]  Button play;
	[SerializeField]  Button quit;
	Text playText;
	Text quitText;
	[SerializeField] float buttonsFadeTime;

	[SerializeField] Text version;
	
	bool masterReady, nonMasterReady;

	void Start () {
		playText = play.transform.GetChild(0).GetComponent<Text>();
		quitText = quit.transform.GetChild(0).GetComponent<Text>();
		
		masterReady = false;
		nonMasterReady = false;

		version.text = p3Names.Instance.staticStrings.version;
	}

	//------------------------ ui control events -------------------------
	public void QuitButtonClicked () {
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
		
		Connect();
	}

	void Connect () {
		PhotonNetwork.autoJoinLobby = false;    
		PhotonNetwork.playerName = p3Names.Instance.staticStrings.defaultPlayerName + Random.Range(0,1000);
		PhotonNetwork.ConnectUsingSettings(p3Names.Instance.staticStrings.connectionSettings);
	}

	

	//------------------------ photon events ---------------------
	public override void OnConnectedToMaster ()
	{
		base.OnConnectedToMaster ();
		PhotonNetwork.JoinOrCreateRoom(	
               p3Names.Instance.staticStrings.defaultRoomName,
               new RoomOptions() { maxPlayers = 2 },
			new TypedLobby(p3Names.Instance.staticStrings.defaultLobbyName,LobbyType.Default)
		);
		
	}

	public override void OnJoinedRoom ()
	{
		base.OnJoinedRoom ();
		
		Log("Joined room");
		
		if( PhotonNetwork.isMasterClient ) {
			masterReady = true;
			if ( nonMasterReady ) {
				nonMasterReady = false;
				masterReady = false;
				photonView.RPC("OnJoinedRoom2",PhotonTargets.All);
			}
		} else {
			photonView.RPC("OnNonMasterJoinedRoom",PhotonTargets.MasterClient);
		}
		
	}
	
	[PunRPC] void OnNonMasterJoinedRoom () {
		nonMasterReady = true;
		if ( masterReady ) {
			nonMasterReady = false;
			masterReady = false;
			photonView.RPC("OnJoinedRoom2",PhotonTargets.All);
		}
	}

	[PunRPC] void OnJoinedRoom2 () {
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
		yield return new WaitForSeconds(0.2f);	

		PhotonNetwork.isMessageQueueRunning = false;
		//PhotonNetwork.RemoveRPCsInGroup(0);
		Application.LoadLevel("p3s2");
	}


	//---------------- photon events : default implementations ------
	public override void OnConnectedToPhoton ()
	{
		base.OnConnectedToPhoton ();
		Log("Connected to server");
	}
	
	public override void OnConnectionFail (DisconnectCause cause)
	{
		base.OnConnectionFail (cause);
		Log("Can not connect to server");
	}
	
	public override void OnFailedToConnectToPhoton (DisconnectCause cause)
	{
		base.OnFailedToConnectToPhoton (cause);
		Log("Failed to connect to server");
	}
	
	
	public override void OnPhotonPlayerConnected (PhotonPlayer newPlayer)
	{
		base.OnPhotonPlayerConnected (newPlayer);
		Log("New player joined to room");
	}

	//------------------------ help functions -----------------
	public void Log ( string s ) {	
		log.text = s;
		Debug.Log(s);
	}

	
}
