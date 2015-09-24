using UnityEngine;
using System.Collections;

public class p2Connector : Photon.MonoBehaviour
{	
	public string room_name;
	public string my_name="player";
	public string gameVersion = "p2";
	public string serverPrefabName = "ServerP2";
	public string lobbyName = "p2lobby";
	public LobbyType lobbyType;
	
	public void Start()
	{
		PhotonNetwork.autoJoinLobby = false;    
		PhotonNetwork.playerName = my_name + Random.Range(0,1000);
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
			p2Map.Instance.StartCustom();
			p2Map.Instance.gameObject.SetActive(false);
		} else Debug.Log("joined room as normal-client");
	}
	
}

