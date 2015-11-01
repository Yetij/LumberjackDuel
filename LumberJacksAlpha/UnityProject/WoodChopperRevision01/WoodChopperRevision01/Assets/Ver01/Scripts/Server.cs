using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Server : Photon.PunBehaviour {
    public Playground playgroundPrefab;
    public Lumberjack lumberjackPrefab;
    public InputAdapter inputAdapterPrefab;


    public int gridX = 10, gridY = 10;
    float offsetX = 1f, offsetY = 1f;

    PLAY iAm;
    PLAY current;

    Playground playground;
    Lumberjack[] character;
    InputAdapter inputAdapter;

    UiDisplay uiDisplay;
    PhotonPlayer[] players;

	public static Server self { get; private set; }

	void Awake () {
		if (self == null) {
			self = this;
		} else {
			Debug.LogError("Server class is supposed to be used as a singleton !");
		}
	}

    public override void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        playground = Instantiate<Playground>(playgroundPrefab);
        playground.Init(gridX, gridY, offsetX, offsetY);

        character = new Lumberjack[2];
        var l1 = Instantiate<Lumberjack>(lumberjackPrefab);
        l1.player = PLAY.ER1;
        character[0] = l1;
        var l2 = Instantiate<Lumberjack>(lumberjackPrefab);
        l2.player = PLAY.ER2;
        character[1] = l2;

        iAm = PhotonNetwork.isMasterClient ? PLAY.ER1 : PLAY.ER2;
        current = PLAY.ER1;

        inputAdapter = Instantiate<InputAdapter>(inputAdapterPrefab);

        uiDisplay = GameObject.FindObjectOfType<UiDisplay>();
            

        photonView.RPC("OneSideReady", PhotonTargets.MasterClient);
    }

    bool onePlayerReady = false;
    private UiTree uiTreeSelected;
    private bool _run;

    float timer = 0;
    public float turnTime = 10f;
	int last_sync_time;

    void Update ()
    {
        if ( _run & PhotonNetwork.isMasterClient)
        {
            timer -= Time.deltaTime;
            if ( timer <= 0 )
            {
				_run = false;
				var next = current == PLAY.ER1 ? (int)PLAY.ER2 : (int)PLAY.ER1;
				var p = character[next].parameters;
				photonView.RPC("ChangeTurn", PhotonTargets.AllViaServer, next,p.ac, p.points);
			} else {
				if (timer <= last_sync_time ) {
					photonView.RPC("SyncUiTime", PhotonTargets.AllViaServer,last_sync_time);
					last_sync_time --;
				}
			}
        }
    }

	[PunRPC]
	void SyncUiTime (int i ) {
		uiDisplay.SetTime (i);
	}

    [PunRPC] 
    void ChangeTurn (int thisTurnPlayer,int ac )
	{
		current = (PLAY)thisTurnPlayer;
		uiDisplay.UpdateAc (ac);
		if (PhotonNetwork.isMasterClient) {
			timer = turnTime;
			last_sync_time = (int)Mathf.Ceil(turnTime);
			_run = true;
		}
	}
	
    [PunRPC]
    void OneSideReady()
    {
        if ( onePlayerReady )
        {
            photonView.RPC("AllReady", PhotonTargets.AllViaServer);
        } else
        {
            onePlayerReady = true;
        }
    }


    [PunRPC]
    void AllReady ()
    {
        inputAdapter.onTap += OnTap;
        inputAdapter.onDrag += OnDrag;
        players = PhotonNetwork.playerList;
        _run = true;
    }

    void OnTap ( Vector2 pos )
    {

        Cell c = playground.GetCellAt(pos);
        if (c != null)
        {
            if (iAm == current)
            {
                if (c.tree != null | c.character != null)
                {
                    photonView.RPC("OnPlayerChop", PhotonTargets.MasterClient, (int)iAm, c.x, c.y);
                } else
                {
                    if ( uiTreeSelected != null )
                    {
                        photonView.RPC("OnPlayerPlant", PhotonTargets.MasterClient, (int)iAm, c.x, c.y, (int) uiTreeSelected.type); 
                    }
                    else photonView.RPC("OnPlayerMove", PhotonTargets.MasterClient, (int)iAm, c.x, c.y);

                }
            }
        }

    }

    void OnDrag (Vector2 pos, Vector2 delta)
    {

    }

    [PunRPC]
    void OnPlayerChop(int player, int cx, int cy)
    {
        var p = character[player];
        PLAY _player = (PLAY)player;

        int chopCost = playground.GetChopCost(p,cx,cy);

        if ( p.parameters.ac - chopCost >= 0  )
        {
            var c = playground.GetCellAt(cx, cy);
            if ( c.tree != null )
            {
                p.VisualChop(c);
                p.parameters.ac-= chopCost;
                c.tree.VisualBeingChoped(_player);
            }
            if (c.character != null)
            {
                if (c.character.player != _player)
                {
                    p.VisualChop(c);
                    p.parameters.ac-= chopCost;
                    c.character.VisualBeingChoped(_player);
                }
            }
        }
    }

    [PunRPC]
    void OnPlayerPlant(int player, int cx, int cy,int treeType)
    {
        var p = character[player];
        PLAY _player = (PLAY)player;
        TreeType type = (TreeType)treeType;

        int plantCost = playground.GetPLantCost(p,type);

        if (p.parameters.ac - plantCost >= 0)
        {
            var c = playground.GetCellAt(cx, cy);
            if (c.tree == null & c.character == null )
            {
                p.VisualPlant(c);
                p.parameters.ac -= plantCost;
                c.VisualAddTree(playground.GetTree(type));
            }
        }
    }

    [PunRPC] 
    void OnPlayerMove (int player, int cx, int cy)
    {
        var p = character[player];
        PLAY _player = (PLAY)player;

        int moveCost = playground.GetMoveCost(p);
        if (p.parameters.ac - moveCost >= 0)
        {
            var c = playground.GetCellAt(cx, cy);
            if (c.tree == null & c.character == null)
            {
                p.VisualMoveTo(c);
                p.parameters.ac -= moveCost;
            }
        }
    }

    public void ServerPause()
    {
        if (PhotonNetwork.isMasterClient) {
			_run = false;
		}
    }

    public void ServerUnPause()
    {
        if (PhotonNetwork.isMasterClient) 
		{
			_run = true;
			photonView.RPC("UpdateAcUi", PhotonTargets.All, character[(int)current].parameters.ac);
			photonView.RPC("UpdateAcUi", PhotonTargets.All, (int)current,character[(int)current].parameters.points);

		}
    }

	[PunRPC]
	void UpdateAcUi (int ac_remains ) 
	{
		uiDisplay.UpdateAc (ac_remains);
	}

	[PunRPC]
	void UpdatePointsUi (int player, int points ) 
	{
		uiDisplay.UpdatePoints ((PLAY)player, points);
	}
}
