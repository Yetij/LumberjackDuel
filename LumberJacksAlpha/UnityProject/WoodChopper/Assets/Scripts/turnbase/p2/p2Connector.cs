using UnityEngine;
using System.Collections;

public class p2Connector : MonoBehaviour
{	
	public string room_name;
	public string gameVersion = "p2";
	public string serverPrefabName = "ServerP2";
	public string lobbyName = "p2lobby";
	public LobbyType lobbyType;
	
	public void Start()
	{
		PhotonNetwork.autoJoinLobby = false;    
		PhotonNetwork.ConnectUsingSettings(gameVersion);
	}

	void OnConnectedToMaster  () {
		PhotonNetwork.JoinOrCreateRoom(	
			room_name,
	       	new RoomOptions() { maxPlayers = 2 },
			new TypedLobby(lobbyName,lobbyType)
		);
	}
	
	void OnJoinedRoom () {
		if( PhotonNetwork.isMasterClient ) {
			Debug.Log("joined room as master-client");
			PhotonNetwork.InstantiateSceneObject(serverPrefabName,Vector3.zero,Quaternion.identity,0, null);
		} else Debug.Log("joined room as normal-client");
	}
	
}

