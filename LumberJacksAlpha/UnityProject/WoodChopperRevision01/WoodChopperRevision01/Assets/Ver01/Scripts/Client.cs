using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Client : Photon.PunBehaviour {
    [SerializeField] VisualJack jack_prefab;
    [SerializeField] VisualPlayground playground_prefab;

    private VisualPlayground playground;
    private VisualJack[] character;
    private int acPerTurn;

    private PLAY iAm;
    private PLAY playing;

    private InputAdapter input;
    private UIAdapter ui;
    private bool lockInput = false;
    private bool localLock;

    // ################## events / callbacks #############################
    void OnTap(Vector2 pos)
    {
        if ( iAm != playing | lockInput) return;
        int x, y;
        if ( playground.ValidPos(pos,out x, out y) )
        {
            if (iAm == playing) {
                photonView.RPC("S_PlayerInput", PhotonTargets.MasterClient, (int)iAm, x, y, (int)ui.selectedPlant);
                if(ui.currentSelected != null ) ui.currentSelected.isOn = false;
            }
        } else
        {
            if (ui.currentSelected != null)  ui.currentSelected.isOn = false;
        }

    }
   
    void OnDrag(Vector2 pos, Vector2 delta)
    {
        if (iAm != playing | lockInput) return;

    }

    // ################## init functions #############################
    [PunRPC]
    void InitClient (int gridX, int gridY, float offsetX , float offsetY )
    {
        Debug.Log("InitClient");
        playground = Instantiate<VisualPlayground>(playground_prefab);
        playground.name = "Playground";
        playground.Init(gridX, gridY, offsetX, offsetY);

        character = new VisualJack[2];

        var l1 = Instantiate<VisualJack>(jack_prefab);
        l1.Init(PLAY.ER1,  0, 0, playground.Pos(0,0));
        Debug.Log("jack 1 pos = " + playground.Pos(0, 0));
        l1.name = "Jack-1";
        character[0] = l1;

        var l2 = Instantiate<VisualJack>(jack_prefab);
        l2.Init(PLAY.ER2, gridX-1, 0, playground.Pos(gridX - 1, 0));
        l2.name = "Jack-2";
        character[1] = l2;

        iAm = PhotonNetwork.isMasterClient ? PLAY.ER1 : PLAY.ER2;

        input = FindObjectOfType<InputAdapter>();
        input.onTap += OnTap;
        input.onDrag += OnDrag;

        ui = FindObjectOfType<UIAdapter>();

        photonView.RPC("S_ClientInited", PhotonTargets.MasterClient, (int) iAm);
    }

    // ################### Logical functions #################################
    [PunRPC]
    void C_Lock()
    {
        lockInput = true;
    }

    [PunRPC]
    void C_Unlock()
    {
        lockInput = false;

    }

    [PunRPC]
    IEnumerator C_3sPrepare ()
    {
        Debug.Log(Definitions.DelayAtBeginningTime + " sec for preparation");
        yield return new WaitForSeconds(Definitions.DelayAtBeginningTime);
        photonView.RPC("S_ClientPrepared", PhotonTargets.MasterClient, (int)iAm);
    }

    [PunRPC]
    void C_TurnChanged(int player, int ac) 
    {
        ui.AC(ac);
        playing = (PLAY)player;
    }

    // ######################### UI #################################
    [PunRPC]
    void C_Points(int player, int points)
    {
        ui.Points(player,points);
    }

    [PunRPC]
    void C_Timer(int time)
    {
        ui.Timer(time);
    }

    [PunRPC]
    void C_AC(int ac)
    {
        ui.AC(ac);
    }
    
    [PunRPC]
    void C_MatchEnd (int winner, int player1_matchScore, int player2_matchScore )
    {
        // ui show dialog
    }

    // ################### Jack Actions #################################
    [PunRPC]
    void C_ChopPlayer(int chopper, int another )
    {
        // play animations
        character[chopper].Chop(character[another]);
        character[another].BeingChop(character[chopper]);
    }

    [PunRPC]
    void C_ChopTree (int _player, int x, int y )
    {
        character[_player].Chop(x, y);
        playground.ChopTree(character[_player], x, y);
    }

    [PunRPC]
    void C_Move(int _player, int x, int y)
    {
        character[_player].Move(x, y);
    }

    [PunRPC]
    void C_PlantTree(int x, int y, int type, int growth)
    {
        Debug.Log("Client place tree at " + new Int2(x, y));
        playground.PlaceTree((TreeType ) type, x, y,(Growth) growth);
    }

}
