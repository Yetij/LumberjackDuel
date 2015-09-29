using UnityEngine;
using System.Collections.Generic;
using StaticStructure;

[RequireComponent(typeof(PhotonView))]
public class p2Scene : Photon.MonoBehaviour
{
	public float globalDominoDelay = 0.25f;

	public bool _run { get; private set; } 
	public int currentTurnNb { get; private set; }
	public int startTreeNumber=10;

	p2Map localMap;
	p2TreePool localPool;

	void Start () {
		players = new List<p2Player>();
		GameObject g = new GameObject("Touch Input2", typeof(TouchInput));
		g.GetComponent<TouchInput>().minSwipeDistance = 20;

		probability = new int[treeWeightList.Length+1];
		probability[0] = 0;
		int k = 0;
		for(int i =0; i < treeWeightList.Length; i ++ ) {
			k += treeWeightList[i].weight;
			probability[i+1] = k;
		}

		p2Scene.Instance.gameObject.SetActive(false);
	}

	public void Run () {
		gameObject.SetActive(true);
		TouchInput.Instance.gameObject.SetActive(true);
		foreach ( var p in players ) {
			p.gameObject.SetActive(true);
			if ( p.photonView.isMine ) {
				TouchInput.Instance.AddListener(p);
				p2Gui.Instance.AddListener(p);
				p2Gui.Instance.myName.text = p.photonView.owner.name;
			} else {
				p2Gui.Instance.opponentName.text = p.photonView.owner.name;
			}
		}
	}

	public void Initialize () {

		localMap = p2Map.Instance;
		localPool = p2TreePool.Instance;

		if ( !PhotonNetwork.isMasterClient) photonView.RPC("SceneLoaded",PhotonTargets.MasterClient);
	}
	
	[PunRPC] void SceneLoaded () {
		photonView.RPC("CreatePlayers", PhotonTargets.All);
	}
	
	[PunRPC] void CreatePlayers () {
		PhotonNetwork.Instantiate(NameMap.get["playerPrefabName"],new Vector3(0,0,0),Quaternion.identity,0);
	}

	public List<p2Player> players { get; private set; }

	bool masterReady, nonMasterReady;

	int player_verf_count = 0;
	public void OnPlayerReady (p2Player p) {
		if ( !players.Contains(p) ) players.Add(p);
		p.gameObject.SetActive(false);

		if ( !PhotonNetwork.isMasterClient &  player_verf_count >= PhotonNetwork.room.maxPlayers ) {
			photonView.RPC("NonMasterClientReady", PhotonTargets.MasterClient);
		}

		if ( PhotonNetwork.isMasterClient &  player_verf_count >= PhotonNetwork.room.maxPlayers ) {
			masterReady = true;
			if ( nonMasterReady & !_run ) {
				photonView.RPC("OnGameStart",PhotonTargets.All,Random.Range(0,2));
			}
		}
	}

	public void ActivateTrees (TreeActivateTime time ) {
		foreach ( var t in treesInScene ) {
			if ( t.isActiveAndEnabled & t.activateTime == time ) {
				t.Activate();
			}
		}
	}

	[PunRPC] void NonMasterClientReady () {
		nonMasterReady = true;
		if ( masterReady & !_run ) {
			photonView.RPC("OnGameStart",PhotonTargets.All,Random.Range(0,2));
		}
	}
	

	[PunRPC] void OnGameStart (int start_turn) {
		p2Gui.Instance.PanelPreToIn();
		Debug.Log("OnGameStart");
		nonMasterReady = false;
		masterReady = false;
		_run = true;
		currentTurnNb = start_turn ;

		foreach ( var p in players ) {
			p.OnGameStart (start_turn);
		}
	}

	[PunRPC] void OnGameEnd (int winner) {
		_run = false;
		foreach ( var p in players ) {
			p.OnGameEnd (winner);
		}
	}
	
	public void OnVictoryConditionsReached (int winner_id) {
		photonView.RPC("OnGameEnd",PhotonTargets.All,winner_id);
	}

	public void SceneGenTree(int number) {
		for(int k=0;  k < number ; k ++ ) {
			var t = RandomTree();
			var l = localMap.FreeCells();
			if ( l.Count > 0 ) { 
				var c = l[Random.Range(0,l.Count)];
				photonView.RPC("_BackGroundAddTree", PhotonTargets.All, (byte) t, c.x, c.z);
			} else {
				photonView.RPC("_BackGroundAddTree", PhotonTargets.All,  (byte) t, -1, -1);
			}
		}
	}
	public void OnBackgroundStart (p2Player invoker,int treenb) {
		SceneGenTree(treenb);
		photonView.RPC("_OnBackgroundStart", PhotonTargets.All);
	}

	[HideInInspector] public List<AbsTree> treesInScene = new List<AbsTree>();

	p2Player GetPlayer ( int owner_id ) {
		foreach( var p in players ) {
			if ( p.photonView.owner.ID == owner_id ) return p;
		}
		throw new UnityException("Invalid owner id: "+ owner_id);
	}

	[PunRPC] void _BackGroundAddTree (byte t , int x, int z) {
		if( localMap[x,z] != null ) {
			var tree = localPool.Get((TreeType)t);
			localMap[x,z].OnPlayerPlantTree(tree,null, 1);
		}
	}

	[PunRPC] void _OnBackgroundStart () {
		Debug.Log("_OnBackgroundStart");
		foreach ( var tree in treesInScene ) {
			tree.OnBackgroundUpdate (players);
		}
		foreach ( var p in players ) {
			p.OnBackgroundStart();
		}
	}

	public void OnBackgroundEnd () {
		photonView.RPC("_OnBackgroundEnd", PhotonTargets.MasterClient);
	}

	int background_end_counter = 0;
	[PunRPC] void _OnBackgroundEnd () {
		background_end_counter ++;
		if ( background_end_counter == 2 ) {
			currentTurnNb ++;
			photonView.RPC("_OnTurnStart", PhotonTargets.All,currentTurnNb);
		}
	}

	[PunRPC] void _OnTurnStart ( int turn_nb ) {
		background_end_counter = 0;
		currentTurnNb = turn_nb;
		foreach ( var t in treesInScene ) {
			t.OnTurnStart(turn_nb);
		}
		foreach ( var p in players ) {
			p.OnTurnStart (turn_nb);
		}
	}

	[SerializeField] TreeWeight[] treeWeightList;

	int[] probability;

	int turnToGen=0;

	TreeType RandomTree () {
		int max = 0;
		foreach ( var _v in treeWeightList ) {
			max += _v.weight;
		}

		int r = Random.Range(0,max);
		for(int i=0; i < probability.Length - 1; i++ ) {
			if ( r >= probability[i] & r < probability[i+1] ) return treeWeightList[i].type;
		}
		throw new UnityException("code should not reach here: r="+ r);
	}

	public void OnRematch () {
		Debug.Log("OnRematch");

		localMap.OnRematch();
		player_verf_count = 0;
		turnToGen=0;
		background_end_counter = 0;
		treesInScene.Clear();

		foreach ( var p in players ) {
			p.OnRematch();
		}

		p2Gui.Instance.PanelInToPre();
	}

	#region hide
	private static p2Scene _instance;
	public static p2Scene Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(p2Scene)) as p2Scene;
			}
			return _instance;
		}
	}
	#endregion
}

