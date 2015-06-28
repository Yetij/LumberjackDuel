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
	float deltaTime = 0.0f;
	
	void Update()
	{
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
	}
	string _fps="";

	void OnGUI () {
		GUILayout.Label(m,GUILayout.Width(Screen.width));
		if ( v5GameController.Instance != null ) {
			if ( v5GameController.Instance.players != null ) {
				foreach ( var p in v5GameController.Instance.players ) {
					if ( p.netID == PhotonNetwork.player.ID ) {
						GUILayout.Label("MY ID=" + p.netID,GUILayout.Width(Screen.width/4));
						GUILayout.Label("YOUR HP=" + p.hp,GUILayout.Width(Screen.width/4));
					}
				}
			}
		}
		GUILayout.Label(_fps,GUILayout.Width(Screen.width/2));
	}

	public void Start()
	{
		_const = v5Const.Instance;
		PhotonNetwork.autoJoinLobby = false;    
		PhotonNetwork.ConnectUsingSettings(_const.netConnectionSettings._GameVersion );
		StartCoroutine(_Calculate() );
	}

	IEnumerator _Calculate () {
		while ( true ) {
			float msec = deltaTime * 1000.0f;
			float fps = 1.0f / deltaTime;
		 	_fps = string.Format("{0:0.0} ms\n({1:0.} fps)", msec, fps);
			yield return new WaitForSeconds(1);
		}
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
										new TypedLobby(_const.netConnectionSettings._LobbyName,_const.netConnectionSettings._LobbyType)
		);
	}

	
	void OnReceivedRoomListUpdate  () {
		//Debug.Log("OnReceivedRoomListUpdate");
	}
	
	void OnJoinedRoom () {
		//Debug.Log("OnJoinedRoom");
		if( PhotonNetwork.isMasterClient ) {
			Debug.Log("im master client???");
			PhotonNetwork.InstantiateSceneObject(_const.prefabNames._GameController,Vector3.zero,Quaternion.identity,0, null);
		} else Debug.Log("im not master client :'(");
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

