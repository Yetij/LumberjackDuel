using UnityEngine;
using System.Collections;

public class p1Connector : MonoBehaviour
{	
	public string room_name;

	string m="";
	p1Const _const;
	float deltaTime = 0.0f;
	string _fps="";
	
	void Awake () { Application.logMessageReceived += (message,stackTrace,type)=> { m=message; }; }
	void Update() { deltaTime += (Time.deltaTime - deltaTime) * 0.1f; }
	
	void OnGUI () {
		//GUILayout.Label(m,GUILayout.Width(Screen.width));
		//if ( p1CellController.Instance != null ) {
		//	p1CellController.Instance._GUI();
		//}
		GUILayout.Label(_fps,GUILayout.Height(Screen.height - 60),GUILayout.Width(Screen.width/2));
	}
	
	public void Start()
	{
		_const = p1Const.Instance;
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
	
	void OnConnectedToMaster  () {
		PhotonNetwork.JoinOrCreateRoom(	
		                               room_name,
		                               new RoomOptions() { maxPlayers = 2 },
		new TypedLobby(_const.netConnectionSettings._LobbyName,_const.netConnectionSettings._LobbyType)
		);
	}
	
	void OnJoinedRoom () {
		if( PhotonNetwork.isMasterClient ) {
			Debug.Log("joined room as master-client");
			PhotonNetwork.InstantiateSceneObject(_const.prefabNames._GameController,Vector3.zero,Quaternion.identity,0, null);
		} else Debug.Log("joined room as normal-client");
	}
	
}

