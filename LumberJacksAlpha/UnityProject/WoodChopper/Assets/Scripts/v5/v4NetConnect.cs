using UnityEngine;
using System.Collections;

public class v4NetConnect : MonoBehaviour
{	
	public string room_name;
	string m="";
	v5Const _const;

	void Awake () {
		Application.logMessageReceived += (message,stackTrace,type)=> { m=message; };
	}

	void OnGUI () {
		GUILayout.Label(m,GUILayout.Width(Screen.width));
	}

	public void Start()
	{
		_const = v5Const.Instance;
		PhotonNetwork.autoJoinLobby = false;    
		PhotonNetwork.ConnectUsingSettings(_const.settings._GameVersion );
	}
	
	void OnJoinedLobby  () {
		//Debug.Log("OnJoinedLobby");
	}
	void OnLeftLobby  () {
		//Debug.Log("OnLeftLobby");
	}
	
	void OnConnectedToPhoton() {
		//Debug.Log("OnConnectedToPhoton");
	}
	void OnLeftRoom () {
		//Debug.Log("OnLeftRoom");
	}
	void OnPhotonCreateRoomFailed  () {
		//Debug.Log("OnPhotonCreateRoomFailed");
	}
	void OnPhotonJoinRoomFailed  () {
		//Debug.Log("OnPhotonJoinRoomFailed");
	}
	void OnCreatedRoom  () {
		//Debug.Log("OnCreatedRoom");
	}
	
	void OnDisconnectedFromPhoton  () {
		//Debug.Log("OnDisconnectedFromPhoton");
	}
	void OnConnectionFail  () {
		//Debug.Log("OnConnectionFail");
	}

	void OnFailedToConnectToPhoton  () {
		//Debug.Log("OnFailedToConnectToPhoton");
	}
	
	void OnConnectedToMaster  () {
		//Debug.Log("OnConnectedToMaster");
		PhotonNetwork.JoinOrCreateRoom(	room_name,
		                               	new RoomOptions() { maxPlayers = 2 },
										new TypedLobby(_const.settings._LobbyName,_const.settings._LobbyType)
		);
	}

	
	void OnReceivedRoomListUpdate  () {
		//Debug.Log("OnReceivedRoomListUpdate");
	}
	
	void OnJoinedRoom () {
		//Debug.Log("OnJoinedRoom");
		if( PhotonNetwork.isMasterClient ) {
			Debug.Log("wow im master client???");
			PhotonNetwork.InstantiateSceneObject(_const.prefabNames._GameController,Vector3.zero,Quaternion.identity,0, null);
		} else Debug.Log("im not master client :((");
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

