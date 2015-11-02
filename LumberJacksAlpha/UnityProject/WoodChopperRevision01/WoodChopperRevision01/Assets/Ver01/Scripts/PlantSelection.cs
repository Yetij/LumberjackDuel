using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(PhotonView))]
public class PlantSelection : Photon.MonoBehaviour {
    void OnLevelWasLoaded(int level)
    {
        PhotonNetwork.isMessageQueueRunning = true;
    }

    Toggle[] buttons;
    private bool nonMasterReady;
    private bool masterReady;

    void Start ()
    {
        buttons = GameObject.FindObjectsOfType<Toggle>();
    }


    public void OnStartButtonClicked()
    {

        GameObject.FindObjectOfType<Button>().GetComponentInChildren<Text>().text = "wait...";

        if (PhotonNetwork.isMasterClient)
        {
            photonView.RPC("MasterReady", PhotonTargets.MasterClient);
        }
        else
        {
            photonView.RPC("NonMasterReady", PhotonTargets.MasterClient);
        }

    }

    [PunRPC]
    void NonMasterReady()
    {
        nonMasterReady = true;
        TryStartGame();
    }

    private void TryStartGame()
    {
        if ( nonMasterReady & masterReady )
        {
            photonView.RPC("StartGame", PhotonTargets.All);
        }
    }

    int plantIndex;

    [PunRPC]
    void StartGame()
    {
        
        plantIndex = 0;
        foreach ( var c in buttons )
        {
            if ( c.isOn )
            {
                PlayerPrefs.SetString("fukingtreehash_tree" + plantIndex, c.name);
                plantIndex++;
                if (plantIndex == 3) break;
            }
        }

        PhotonNetwork.isMessageQueueRunning = false;
        Application.LoadLevel(Application.loadedLevel + 1);
    }

    [PunRPC]
    void MasterReady()
    {
        masterReady = true;
        TryStartGame();
    }

}
