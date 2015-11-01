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

    void Update ()
    {
        if ( _run & PhotonNetwork.isMasterClient)
        {
            timer += Time.deltaTime;
            if ( timer > turnTime )
            {
                timer = 0;
                photonView.RPC("ChangeTurn", PhotonTargets.AllViaServer, current == PLAY.ER1 ? (int)PLAY.ER2 : (int)PLAY.ER1);
            }
        }
    }

    [PunRPC] 
    void ChangeTurn (int thisTurnPlayer )
    {
        current = (PLAY)thisTurnPlayer;
        uiDisplay.UpdateUi(character[thisTurnPlayer]);
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
                OnActionStart();
                p.VisualChop(cx, cy);
                p.parameters.ac-= chopCost;
                c.tree.VisualBeingChoped(_player);
            }
            if (c.character != null)
            {
                if (c.character.player != _player)
                {
                    OnActionStart();
                    p.VisualChop(cx, cy);
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
                OnActionStart();
                p.VisualPlant(cx, cy);
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
                OnActionStart();
                p.VisualMoveTo(cx, cy);
                p.parameters.ac -= moveCost;
            }
        }
    }

    private void OnActionStart()
    {
        throw new NotImplementedException();
    }

    public void OnActionDone()
    {
        throw new NotImplementedException();
    }
}
