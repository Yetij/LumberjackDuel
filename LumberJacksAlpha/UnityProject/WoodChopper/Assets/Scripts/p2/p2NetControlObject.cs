using UnityEngine;
using System.Collections;

public class p2NetControlObject : Photon.PunBehaviour  {
	void Start () {
		Debug.Log("Start p2NetControlObject");
		if ( !PhotonNetwork.isMasterClient ) {
			p2Gui.Instance.connectorUI.photonView.RPC("OnNonMasterNetControlObjectInitialized",PhotonTargets.MasterClient);
		}
		p2Gui.Instance.connectorUI.Log("Initialized room");
	}
}
