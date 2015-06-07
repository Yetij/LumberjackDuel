using UnityEngine;
using System.Collections;


public class v3PhotonNet : Photon.MonoBehaviour
{	
	public string room_name="whatever";
	public string lobby_name="lobby2";
	public string Version = "1.0";
	RoomInfo[] room_list = new RoomInfo[0];
	string m="";

	void Awake () {
		Application.logMessageReceived += (message,stackTrace,type)=> { m=message; };
	}

	public virtual void Start()
	{
		PhotonNetwork.autoJoinLobby = false;    
		PhotonNetwork.ConnectUsingSettings(Version );
	}


/*
	void OnGUI () {
		GUILayout.Label(m,GUILayout.Width(Screen.width));
		if ( !PhotonNetwork.connected ) return;
		if  (GUILayout.Button("Join Lobby ",GUILayout.Width(150) ) ) { 
			PhotonNetwork.JoinLobby();
		}
		if  (GUILayout.Button("Create Room or Join if room exits",GUILayout.Width(250)) ) { 
			PhotonNetwork.JoinOrCreateRoom(game_name, new RoomOptions() { maxPlayers = 2 },null);
		}
		if ( room_list.Length > 0 ) {
			GUILayout.Label("Select room to join",GUILayout.Width(Screen.width));
			foreach ( RoomInfo r in room_list ) {
				if ( GUILayout.Button(r.name,GUILayout.Width(250) ) ) {
					PhotonNetwork.JoinRoom(r.name);
				}
			}
		}
	}
*/
	void OnJoinedLobby  () {
		Debug.Log("OnJoinedLobby");
	}
	void OnLeftLobby  () {
		Debug.Log("OnLeftLobby");
	}

	void OnConnectedToPhoton() {
		Debug.Log("OnConnectedToPhoton");
	}
	void OnLeftRoom () {
		Debug.Log("OnLeftRoom");
	}
	void OnPhotonCreateRoomFailed  () {
		Debug.Log("OnPhotonCreateRoomFailed");

	}
	void OnPhotonJoinRoomFailed  () {
		Debug.Log("OnPhotonJoinRoomFailed");
	}
	void OnCreatedRoom  () {
		Debug.Log("OnCreatedRoom");
	}
	
	void OnDisconnectedFromPhoton  () {
		Debug.Log("OnDisconnectedFromPhoton");
	}
	void OnConnectionFail  () {
		Debug.Log("OnConnectionFail");

	}
	void OnFailedToConnectToPhoton  () {
		Debug.Log("OnFailedToConnectToPhoton");

	}	

	void OnConnectedToMaster  () {
		Debug.Log("OnConnectedToMaster");
		PhotonNetwork.JoinOrCreateRoom(room_name,new RoomOptions(){maxPlayers=2},new TypedLobby(lobby_name,LobbyType.Default));
	}

	void OnReceivedRoomListUpdate  () {
		Debug.Log("OnReceivedRoomListUpdate");
		room_list = PhotonNetwork.GetRoomList();
	}

	void OnJoinedRoom () {
		Debug.Log("OnJoinedRoom");
		//if ( PhotonNetwork.isMasterClient ) {
		PhotonNetwork.InstantiateSceneObject("v3GameManager",Vector3.zero,Quaternion.identity,0,null);
		//}
		//Network.Instantiate(v3Refs.Instance.player_pref,new Vector3(-10,0,-10),Quaternion.identity,0);
	}
	void OnPhotonPlayerConnected  () {
		Debug.Log("OnPhotonPlayerConnected");
	}
	void OnPhotonPlayerDisconnected  () {
		Debug.Log("OnPhotonPlayerDisconnected");

	}
	void OnPhotonRandomJoinFailed  () {
		
		Debug.Log("OnPhotonRandomJoinFailed");
	}


	void OnMasterClientSwitched  () {}
	void OnPhotonSerializeView  () {}
	void OnPhotonInstantiate  () {}
	void OnPhotonMaxCccuReached  () {}
	void OnPhotonCustomRoomPropertiesChanged  () {}
	void OnPhotonPlayerPropertiesChanged   () {}
	void OnUpdatedFriendList   () {}
	void OnCustomAuthenticationFailed   () {}
	void OnWebRpcResponse   () {}
	void OnOwnershipRequest   () {}
}

