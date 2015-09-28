
using System.Collections.Generic;
using UnityEngine;

/*** MUST BE EXCUTED AT FIRST PLACE *****/
public class NameMap : MonoBehaviour
{
	[System.Serializable]
	public class PrefabName {
		public string scenePrefabName, playerPrefabName;
	}

	[System.Serializable]
	public class GameInfo {
		public string gameVersion;
		public string connectSettingString;
	}

	[System.Serializable]
	public class ConnectInfo {
		public string defaultLobbyName;
		public string defaultRoomName;
		public string defaultPlayerName;
	}
	


	public static Dictionary<string,string> get = new Dictionary<string, string>();

	[SerializeField] PrefabName prefabName;
	[SerializeField] GameInfo gameInfo;
	[SerializeField] ConnectInfo connectInfo;

	void Awake () {
		get.Add("gameVersion",gameInfo.gameVersion);
		get.Add("connectSettingString",gameInfo.connectSettingString);

		get.Add("scenePrefabName", prefabName.scenePrefabName);
		get.Add("playerPrefabName", prefabName.playerPrefabName);

		get.Add("defaultRoomName", connectInfo.defaultRoomName);
		get.Add("defaultPlayerName", connectInfo.defaultPlayerName);
		get.Add("defaultLobbyName", connectInfo.defaultLobbyName);
	}
}

