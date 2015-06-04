using UnityEngine;
using System.Collections;


public class v3NetHelp : MonoBehaviour
{
	public string gameType;
	public string gameName="my_custom_game";
	public int port = 54444;
	public bool disable_clear_hostlist = false;
	[HideInInspector] public v3NetControlNode net_node;

	public string master_ip="192.227.166.97";
	public int master_port=31235;
	public string facilitator_ip="192.227.166.97";
	public int facilitator_port=31234;

	v3Refs refs; 

	bool guioff;

	string s="";
	string m ="";

	void Awake () {
		Application.logMessageReceived += HandleLog;
	}
	
	void Start () {
		refs = v3Refs.Instance;
		MasterServer.ipAddress = master_ip;
		MasterServer.port = master_port;
		Network.natFacilitatorIP = facilitator_ip;
		Network.natFacilitatorPort = facilitator_port;


	}

	void OnMasterServerEvent(MasterServerEvent msEvent) {
		switch(msEvent ) {
		case MasterServerEvent.RegistrationSucceeded:
			Debug.Log("OnMasterServerEvent RegistrationSucceeded");
			break;
		case MasterServerEvent.HostListReceived:
			Debug.Log("OnMasterServerEvent HostListReceived");
			break;
		case MasterServerEvent.RegistrationFailedGameName:
			Debug.Log("OnMasterServerEvent RegistrationFailedGameName");
			break;
		case MasterServerEvent.RegistrationFailedGameType:
			Debug.Log("OnMasterServerEvent RegistrationFailedGameType");
			break;
		case MasterServerEvent.RegistrationFailedNoServer:
			Debug.Log("OnMasterServerEvent RegistrationFailedNoServer");
			break;
		}
	}

	void OnFailedToConnectToMasterServer(NetworkConnectionError info) {
		Debug.Log("Could not connect to master server: " + info);
	}

	HostData[] list_games;
	void OnGUI () {
		if ( guioff ) return;
		
		GUILayout.Label ( m, GUILayout.Width(Screen.width));
		if ( !Network.isClient & !Network.isServer ) {
			if ( GUILayout.Button("Create Game ",GUILayout.Width(200) ) ) {
				Network.InitializeServer(8,port,!Network.HavePublicAddress());
			}
			
			list_games  = MasterServer.PollHostList();
			if ( GUILayout.Button("Update hostlist ",GUILayout.Width(200) ) ) {
				MasterServer.RequestHostList(gameType);
				list_games  = MasterServer.PollHostList();
			}
			if ( list_games != null ) {
				if ( list_games.Length > 0 ) { 
					GUILayout.Label ("Slelect game >>", GUILayout.Width(100));
					foreach ( HostData h in list_games ) {
						if ( GUILayout.Button(h.gameName ,GUILayout.Width(150)) ) {
							Network.Connect(h);
						}
					}
				}
			}
			//if ( !disable_clear_hostlist) MasterServer.ClearHostList();
		}

		if ( Network.isServer ) {
			GUILayout.Label ("my external ip="+Network.player.externalIP, GUILayout.Width(300));
			GUILayout.Label ("my internal ip="+Network.player.ipAddress, GUILayout.Width(300));
			GUILayout.Label ("players in room="+(Network.connections.Length + 1));
			if ( GUILayout.Button("Start Game" ,GUILayout.Width(150)) ) {
				net_node.GetComponent<NetworkView>().RPC("StartGame",RPCMode.AllBuffered);
			}
		}

		if ( Network.isClient ) {
			GUILayout.Label ( m, GUILayout.Width(Screen.width));
			GUILayout.Label ("host ip="+s, GUILayout.Width(Screen.width));
		}
	}
	
	void OnPlayerConnected(NetworkPlayer player) {
		Debug.Log("Player connected from " + player.ipAddress + ":" + player.port);
	}
	void OnPlayerDisconnected(NetworkPlayer player) {
		Debug.Log("Player disconnected");
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}
	void OnServerInitialized() {
		Debug.Log("Server initialized");
		MasterServer.RegisterHost(gameType,gameName);
		Network.Instantiate(refs.net_control_node,Vector3.zero,Quaternion.identity,1)  ;
	}
	void OnConnectedToServer() {
		Debug.Log("Connected to server");
	}

	void HandleLog (string message, string stackTrace, LogType type)
	{
		m = message;
	}

}

