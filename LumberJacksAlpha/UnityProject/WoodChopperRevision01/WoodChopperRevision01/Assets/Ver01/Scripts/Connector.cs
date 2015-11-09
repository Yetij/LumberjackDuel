using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
public class Connector: PunBehaviour
{
    public string room_name;
    public string my_name = "player";
    public string gameVersion = "p2";
    public string serverPrefabName = "ServerP2";
    public string lobbyName = "p2lobby";
    public LobbyType lobbyType;

    Text message;

    public void Start()
    {
        message = GameObject.FindObjectOfType<Text>();
        PhotonNetwork.autoJoinLobby = false;
        PhotonNetwork.playerName = my_name + Random.Range(0, 1000);
        PhotonNetwork.ConnectUsingSettings(gameVersion);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom(
                room_name,
               new RoomOptions() { maxPlayers = 2 },
            new TypedLobby(lobbyName, lobbyType)
        );
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.isMasterClient)
        {
            message.text = "joined room as master-client";
        }
        else message.text = "joined room as normal-client";
    }

    public override void OnConnectedToPhoton()
    {
        message.text = "connected to server";
    }

    public override void OnConnectionFail(DisconnectCause cause)
    {
        message.text = "failed to connect";
    }

    public override void OnCreatedRoom()
    {
        message.text = "created room";
    }

    public override void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        message.text = "failed to connect to server";
    }

    public override void OnJoinedLobby()
    {
        message.text = "joined lobby";
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        message.text = "failed to join room";
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        message.text = "new player has joined room";
        photonView.RPC("rpcStartGame", PhotonTargets.All);
    }

    [PunRPC]
    IEnumerator rpcStartGame ()
    {
        yield return new WaitForSeconds(1f);
        message.text = "game is starting ";
        yield return new WaitForSeconds(0.4f);
        message.text = "game is starting .";
        yield return new WaitForSeconds(0.4f);
        message.text = "game is starting ..";
        yield return new WaitForSeconds(0.4f);

        PhotonNetwork.isMessageQueueRunning = false;
        Application.LoadLevel(Application.loadedLevel + 1);
    }
}


