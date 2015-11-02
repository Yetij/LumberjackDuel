using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class PlayScreenInit : Photon.MonoBehaviour {
    public Server serverPrefab;

    void OnLevelWasLoaded(int level)
    {
        Debug.Log("OnLevelWasLoaded");
        PhotonNetwork.isMessageQueueRunning = true;

        if ( PhotonNetwork.isMasterClient ) PhotonNetwork.InstantiateSceneObject(serverPrefab.name, Vector3.zero, Quaternion.identity, 0, null);
    }

}
