using UnityEngine;
using System.Collections;


public class v3NetManager : MonoBehaviour
{
	public int port = 54444;
	[HideInInspector] public v3NetControlNode net_node;
	public bool clientReady;
	public bool serverReady;

	v3Refs refs; 

	bool guioff;

	string s="";
	string m ="";

	void OnGUI () {
		if ( guioff ) return;
		GUILayout.Label ( m, GUILayout.Width(Screen.width));
		if ( !Network.isClient & !Network.isServer ) {
			if ( GUILayout.Button("Create Game ",GUILayout.Width(200) ) ) {
				Network.InitializeServer(8,port,!Network.HavePublicAddress());
			}
			GUILayout.BeginHorizontal();
			GUILayout.Label ("write host ip>>", GUILayout.Width(100));
			s = GUILayout.TextField (s, GUILayout.Width(200));
			if ( GUILayout.Button("Join Game" ,GUILayout.Width(150)) ) {
				Network.Connect(s,port);
			}
			GUILayout.EndHorizontal();
		}

		if ( Network.isServer ) {
			GUILayout.Label ("my ip="+Network.player.ipAddress, GUILayout.Width(300));
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
		Network.Instantiate(refs.net_control_node,Vector3.zero,Quaternion.identity,1)  ;
	}
	void OnConnectedToServer() {
		Debug.Log("Connected to server");
	}
	void Awake () {
		Application.logMessageReceived += HandleLog;
	}

	void Start () {
		refs = v3Refs.Instance;
	}

	void HandleLog (string message, string stackTrace, LogType type)
	{
		m = message;
	}

}

