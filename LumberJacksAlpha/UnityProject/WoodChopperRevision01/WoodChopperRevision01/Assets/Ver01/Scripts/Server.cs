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

    public PLAY iAm;
    public PLAY current;

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
        l1.Init(PLAY.ER1, playground.GetCellAtIndex(0,0));
        l1.parameters.ac = acPerTurn;
        character[0] = l1;

        var l2 = Instantiate<Lumberjack>(lumberjackPrefab);
        l2.Init(PLAY.ER2, playground.GetCellAtIndex(gridX-1, 0));
        l2.parameters.ac = acPerTurn;
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

    float turn_timer = 0;
    public float timePerTurn = 10f;
	int last_sync_time;
    public int  acPerTurn = 5;

    void Update ()
    {
        if ( _run & PhotonNetwork.isMasterClient)
        {
            turn_timer -= Time.deltaTime;
            if ( turn_timer <= 0 )
            {
				_run = false;
				int next = current == PLAY.ER1 ? (int)PLAY.ER2 : (int)PLAY.ER1;
				var p = character[next].parameters;
                p.ac = acPerTurn;
                photonView.RPC("ChangeTurn", PhotonTargets.AllViaServer, next, p.ac);
			} else {
				if (turn_timer <= last_sync_time ) {
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
			turn_timer = timePerTurn;
			last_sync_time = (int)Mathf.Ceil(timePerTurn);
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
       
        if ( PhotonNetwork.isMasterClient )
        {
            photonView.RPC("ChangeTurn", PhotonTargets.AllViaServer, (int)current, acPerTurn);
        }
    }

    void OnTap ( Vector2 pos )
    {

        Cell c = playground.GetCellAtPos(pos);
       
        if (c != null)
        {
            if (iAm == current)
            {
                if ( uiTreeSelected != null )
                {
                    photonView.RPC("MO_PlayerWantPlant", PhotonTargets.MasterClient, (int)iAm, c.x, c.y, (int) uiTreeSelected.type); 
                }
                else photonView.RPC("MO_PlayerWantMoveOrChop", PhotonTargets.MasterClient, (int)iAm, c.x, c.y);
            }
        }

    }

    void OnDrag (Vector2 pos, Vector2 delta)
    {

    }

    [PunRPC]
    void MO_PlayerWantMoveOrChop(int player, int cx, int cy)
    {
        var p = character[player];
        PLAY _player = (PLAY)player;

        var c = playground.GetCellAtIndex(cx, cy);
        if (c != null)
        {
            if ( c.tree == null & c.character == null )
            {
                int moveCost = playground.GetMoveCost(p);
                if (p.parameters.ac - moveCost >= 0)
                {
                    p.parameters.ac -= moveCost;
                    photonView.RPC("VisualMove", PhotonTargets.AllViaServer, player, cx, cy);
                }
            }
            else
            {
                int chopCost = playground.GetChopCost(p, cx, cy);

                if (c.tree != null)
                {
                    p.VisualChop(c);
                    p.parameters.ac -= chopCost;
                    c.tree.VisualBeingChoped(_player);
                    photonView.RPC("VisualChop", PhotonTargets.AllViaServer, player, cx, cy);
                }
                if (c.character != null)
                {
                    if (c.character.player != _player)
                    {
                        p.parameters.ac -= chopCost;
                        c.character.parameters.hp--;
                        photonView.RPC("VisualChop", PhotonTargets.AllViaServer, player, cx, cy);
                        if (c.character.parameters.hp < 0) 
                        {
                            _run = false;
                            Debug.Log("GAME END!!");
                        }
                    }
                }
            }
            
        }
        
    }

    [PunRPC]
    void MO_PlayerWantPlant(int player, int cx, int cy,int treeType)
    {
        var p = character[player];
        PLAY _player = (PLAY)player;
        TreeType type = (TreeType)treeType;

        int plantCost = playground.GetPLantCost(p,type);

        if (p.parameters.ac - plantCost >= 0)
        {
            var c = playground.GetCellAtIndex(cx, cy);
            if (c.tree == null & c.character == null )
            {
                p.parameters.ac -= plantCost;
                photonView.RPC("VisualPlant", PhotonTargets.AllViaServer, player, cx, cy, treeType);
            }
        }
    }

   
    [PunRPC]
    void VisualMove(int player, int cx, int cy) {
        var c = playground.GetCellAtIndex(cx, cy);
        if (c.tree == null & c.character == null)
        {
            character[player].VisualMoveTo(c);
        }
    }

    [PunRPC]
    void VisualChop(int player, int cx, int cy)
    {
        var c = playground.GetCellAtIndex(cx, cy);
        PLAY _player = (PLAY)player;

        if (c.tree != null)
        {
            character[player].VisualChop(c);
            c.tree.VisualBeingChoped(_player);
        }
        if (c.character != null)
        {
            if (c.character.player != _player)
            {
                character[player].VisualChop(c);
                c.character.VisualBeingChoped(_player);
            }
        }
    }
    [PunRPC]
    void VisualPlant(int player, int cx, int cy, int treetype)
    {
        var c = playground.GetCellAtIndex(cx, cy);
        TreeType type = (TreeType)treetype;

        if (c.tree == null & c.character == null)
        {
            character[player].VisualPlant(c);
            c.VisualAddTree(playground.GetTree(type));
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
			photonView.RPC("UpdateUiAc", PhotonTargets.All, character[(int)current].parameters.ac);
			photonView.RPC("UpdateUiPoints", PhotonTargets.All, (int)current,character[(int)current].parameters.points);
		}
    }

	[PunRPC]
	void UpdateUiAc(int ac_remains ) 
	{
		uiDisplay.UpdateAc (ac_remains);
	}

	[PunRPC]
	void UpdateUiPoints(int player, int points ) 
	{
		uiDisplay.UpdatePoints ((PLAY)player, points);
	}
}
