using UnityEngine;
using System.Collections;

public class ConnectTEST : Photon.PunBehaviour {
	public GameObject g;
	void Start () {

		PhotonNetwork.autoJoinLobby = false;    
		PhotonNetwork.playerName = NameMap.get["defaultPlayerName"] + Random.Range(0,1000);

		PhotonNetwork.ConnectUsingSettings(NameMap.get["connectSettingString"]);
	}

	
	void Log ( string s ) {	
		Debug.Log(s);
	}

	
	//-------------------- events ------------------------

	
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
			PhotonNetwork.InstantiateSceneObject(g.name,Vector3.zero,Quaternion.identity,1,null);
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
