using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkView))]
public class v3NetControlNode : MonoBehaviour
{
	NetworkView netview;
	void Awake() {
		Debug.Log("v3NetControlNode " + Application.loadedLevelName + " Awake ");
		( (v3NetHelp) GameObject.FindObjectOfType(typeof(v3NetHelp))).net_node = this;
		netview = GetComponent<NetworkView>();
		netview.group = 1;
	}
	
	void OnLevelWasLoaded(int level) {
		if (Application.loadedLevelName.Equals("v3test") ) {
			Debug.Log("v3NetControlNode " +Application.loadedLevelName+" OnLevelWasLoaded");
			if ( Network.isServer ) netview.RPC("OnServerReady", RPCMode.AllBuffered);
			if ( Network.isClient ) netview.RPC("OnClientReady", RPCMode.AllBuffered);
		}
	}
	void _f () {
		if ( Network.isServer ) Network.Instantiate(v3Refs.Instance.game_manager,Vector3.zero,Quaternion.identity,0);
		Network.Instantiate(v3Refs.Instance.player_pref,new Vector3(-10,0,-10),Quaternion.identity,0);
	}

	bool serverReady,clientReady;

	[RPC] void OnServerReady() {
		serverReady = true;
		if ( serverReady & clientReady ) _f ();
	}
	[RPC] void OnClientReady() {
		clientReady = true;
		if ( serverReady & clientReady ) _f ();
	}

	[RPC] IEnumerator StartGame ()
	{
		// There is no reason to send any more data over the network on the default channel,
		// because we are about to load the level, thus all those objects will get deleted anyway
		Network.SetSendingEnabled(0, false);    
		
		// We need to stop receiving because first the level must be loaded first.
		// Once the level is loaded, rpc's and other state update attached to objects in the level are allowed to fire
		Network.isMessageQueueRunning = false;
		
		// All network views loaded from a level will get a prefix into their NetworkViewID.
		// This will prevent old updates from clients leaking into a newly created scene.
		Network.SetLevelPrefix(v3Refs.Instance.level_net_prefix ++);
		Application.LoadLevel("v3test");
		yield return null;
		
		// Allow receiving data again
		Network.isMessageQueueRunning = true;
		// Now the level has been loaded and we can start sending out data to clients
		Network.SetSendingEnabled(0, true);

	}

}

