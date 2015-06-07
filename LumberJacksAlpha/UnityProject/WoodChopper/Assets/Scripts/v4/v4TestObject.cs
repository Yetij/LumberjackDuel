using UnityEngine;
using System.Collections;

public class v4TestObject : MonoBehaviour {

	PhotonView netview;
	float speed = 5;
	
	void Start () {
		netview = GetComponent<PhotonView>();
		Debug.Log("local player id = " + PhotonNetwork.player.ID);
		Debug.Log("netview owner id = " + netview.ownerId);
		Debug.Log("netview netview id = " + netview.viewID);
	}
	// Update is called once per frame
	void Update () {
		//if ( netview.isMine ) {
			int current_x = (int)Input.GetAxisRaw("Horizontal");
			int current_z = current_x != 0 ? 0 : (int)Input.GetAxisRaw("Vertical");
			transform.position += new Vector3(current_x,0,current_z).normalized*speed*Time.deltaTime;
		//}
	}
	void OnPhotonInstantiate  () {
		Debug.Log("v4TestObject OnPhotonInstantiate" );
	}
	
}
