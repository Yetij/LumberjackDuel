using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Server : Photon.PunBehaviour
{
    public int gridX = 10, gridY = 10;
    float offsetX = 1f, offsetY = 1f;

    public LogicPlayground playground { get; private set; }
    
    public PLAY playing;

	void Awake ()
    {
        if (!PhotonNetwork.isMasterClient) Destroy(this);
    }

    void Start ()
    {
        Debug.Log("Server Start");
        playground = new LogicPlayground(gridX, gridY, offsetX, offsetY);

        character = new LogicJack[2];

        var l1 = new LogicJack(PLAY.ER1, 0, 0);
        l1.ac = acPerTurn;
        character[0] = l1;

        var l2 = new LogicJack(PLAY.ER2, gridX - 1, 0);
        l2.ac = acPerTurn;
        character[1] = l2;

        l1.Opponent = l2;
        l2.Opponent = l1;

        ready = new bool[2] { false, false };

        photonView.RPC("InitClient", PhotonTargets.AllViaServer,gridX, gridY, offsetX, offsetY);
    }

  
    public int  acPerTurn = 5;
    private LogicJack[] character;

    bool[] ready;
    private bool _run;
    [SerializeField] int startTreeNumber;

   
    float turn_timer = 0;
    public float timePerTurn = 10f;
    int last_sync_time;
    private bool blockInput;
    [SerializeField] float dominoDelay;

    [PunRPC] 
    void S_ClientInited (int player)
    {
        ready[player] = true;
        if ( ready[0] & ready[1] )
        {
            playground.RandomTree(startTreeNumber);

            foreach ( var t in playground.cellControl )
            {
                if ( t != null ) photonView.RPC("C_PlantTree", PhotonTargets.AllViaServer, t.x, t.y,(int) t.type, (int)t.growth );
            }

            photonView.RPC("C_3sPrepare", PhotonTargets.AllViaServer);
            ready[0] = false;
            ready[1] = false;
        }
    }

    [PunRPC]
    void S_ClientPrepared(int player)
    {
        Debug.Log("S_ClientPrepared");
        ready[player] = true;
        if (ready[0] & ready[1])
        {
            turn_timer = timePerTurn;
            last_sync_time = (int)Mathf.Ceil(timePerTurn);

            _run = true;

            playing = PLAY.ER1;

            photonView.RPC("C_TurnChanged", PhotonTargets.AllViaServer, (int)playing, character[(int)playing].ac);

            ready[0] = false;
            ready[1] = false;
        }
    }

    void Update ()
    {
        if ( _run )
        {
            turn_timer -= Time.deltaTime;
            if ( turn_timer < 0 & last_sync_time * turn_timer > 0 )
            {
                playing = playing == PLAY.ER1 ? PLAY.ER2 : PLAY.ER1;
                    
                foreach (var c in character) c.ac = acPerTurn;

                playground.TurnChange(character);   

                turn_timer = timePerTurn;
                last_sync_time = (int)Mathf.Ceil(timePerTurn);

                photonView.RPC("C_TurnChanged", PhotonTargets.AllViaServer, (int)playing, character[(int)playing].ac);
			}
            else
            {
				if (turn_timer <= last_sync_time )
                {
                    photonView.RPC("C_Timer", PhotonTargets.AllViaServer,last_sync_time);
                    last_sync_time--;
                }
			}
        }
    }

    [PunRPC]
    void S_PlayerInput (int _player, int x, int y, int _treeSelected )
    {
        if (blockInput | !_run ) return;
        PLAY player = (PLAY)_player;
        if ( player != playing )
        {
            return;
        }
        TreeType treeSelected = (TreeType)_treeSelected;
        if ( treeSelected != TreeType.None )
        {
            // plant a tree 
            if ( playground.PlantTree(character,_player, treeSelected, x, y) )
            {
                Debug.Log("SERVER Player plant tree OK x,y=" + new Int2(x, y));
                StartCoroutine(corPlantTree(_player, x, y, _treeSelected, character[_player].ac, (int) Growth.Small));
            }
            else
                Debug.Log("SERVER Player plant tree FAILED");
        }
        else
        {
            // move or chop
            var op = character[_player].Opponent;
            if ( op.x == x & op.y == y )
            {
                // chop player
                if ( playground.ChopPlayer(character[_player], character[(int)op.player]) )
                {
                    Debug.Log("SERVER Player chop player OK");
                    if (op.hp > 0)
                    {
                        StartCoroutine(corChopPlayer(_player, (int)op.player, character[_player].ac));
                    }
                    else
                    {
                        StartCoroutine(corMatchEnd(_player));
                    }
                }
                else
                    Debug.Log("SERVER  Player chop player FAILED");
            }
            else
            {
                LogicTree t = playground.TreeAt(x, y);
                
                if ( t == null )
                {
                    //move
                    if (playground.PlayerMove(character[_player],x,y))
                    {
                        Debug.Log("SERVER Player move OK");
                        StartCoroutine(corPlayerMove(_player, x, y));
                    }
                    else
                        Debug.Log("SERVER Player move FAILED");
                }
                else
                {
                    // chop tree
                    List<LogicTree> domino;
                    if ( playground.ChopTree(character[_player],x,y, out domino) )
                    {
                        Debug.Log("SERVER Player chop tree OK ");
                        StartCoroutine(corChopTree(_player, x, y, domino));
                    }
                    else
                        Debug.Log("SERVER Player chop tree FAILED");
                }
            }
        }
    }

    // ###################### coroutines ##########################################

    private IEnumerator corChopTree(int _player, int x, int y, List<LogicTree> domino)
    {
        //photonView.RPC("C_Lock", PhotonTargets.AllViaServer);
        blockInput = true;
        _run = false;

        photonView.RPC("C_AC", PhotonTargets.AllViaServer, character[_player].ac);

        photonView.RPC("C_ChopTree", PhotonTargets.AllViaServer, _player, x, y);
        yield return new WaitForSeconds(Definitions.visual_tree_fall_time);
        for (int i =0; i < domino.Count; i ++ )
        {
            photonView.RPC("C_ChopTree", PhotonTargets.AllViaServer, _player, domino[i].x, domino[i].y);
            yield return new WaitForSeconds(Definitions.visual_tree_fall_time);
        }
        photonView.RPC("C_Points", PhotonTargets.AllViaServer, _player, character[_player].points);

        //photonView.RPC("C_Unlock", PhotonTargets.AllViaServer);
        blockInput = false;
        _run = true;
    }

    private IEnumerator corPlayerMove(int _player, int x, int y)
    {
       // photonView.RPC("C_Lock", PhotonTargets.AllViaServer);
        blockInput = true;
        _run = false;

        photonView.RPC("C_Move", PhotonTargets.AllViaServer, _player, x, y);
        photonView.RPC("C_AC", PhotonTargets.AllViaServer, character[_player].ac);

        //expect the animation time = 1s for both players in this case
        yield return new WaitForSeconds(Definitions.visual_move_time);

        //photonView.RPC("C_Unlock", PhotonTargets.AllViaServer);
        blockInput = false;
        _run = true;
    }

    private IEnumerator corMatchEnd(int _player)
    {
        //photonView.RPC("C_Lock", PhotonTargets.AllViaServer);
        blockInput = true;
        _run = false;

        character[_player].matchScore++;
        photonView.RPC("C_MatchEnd", PhotonTargets.AllViaServer, _player, character[0].matchScore, character[1].matchScore);
       
        yield return new WaitForSeconds(Definitions.visual_match_end);

        // !! SHOULD BE SOME KIND OF RESET MATCH HERE !!
        //photonView.RPC("C_Unlock", PhotonTargets.AllViaServer);
        blockInput = false;
        _run = true;
    }

    private IEnumerator corPlantTree(int _player, int x, int y, int _treeSelected, int ac, int growth)
    {
        //photonView.RPC("C_Lock", PhotonTargets.AllViaServer);
        blockInput = true;
        _run = false;

        photonView.RPC("C_PlantTree", PhotonTargets.AllViaServer, x, y, _treeSelected, growth);
        photonView.RPC("C_AC", PhotonTargets.AllViaServer, ac);

        //expect the animation time = 0.5s for both players in this case
        yield return new WaitForSeconds(Definitions.visual_chop_player_time);

        //photonView.RPC("C_Unlock", PhotonTargets.AllViaServer);
        blockInput = false;
        _run = true;
    }

    private IEnumerator corChopPlayer(int player, int another,  int ac)
    {
        //photonView.RPC("C_Lock", PhotonTargets.AllViaServer);
        blockInput = true;
        _run = false;

        photonView.RPC("C_ChopPlayer", PhotonTargets.AllViaServer, player, another);
        photonView.RPC("C_AC", PhotonTargets.AllViaServer, ac);

        //expect the animation time = 0.5s for both players in this case
        yield return new WaitForSeconds(Definitions.visual_chop_player_time);

        //photonView.RPC("C_Unlock", PhotonTargets.AllViaServer);
        blockInput = false;
        _run = true;
    }

}
