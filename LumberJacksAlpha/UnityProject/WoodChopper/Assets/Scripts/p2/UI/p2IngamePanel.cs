using UnityEngine;
using System.Collections;

public class p2IngamePanel : Photon.PunBehaviour {

	public void Initialize () {
		p2Gui.Instance.cam1.gameObject.SetActive(false);
		p2Gui.Instance.cam2.gameObject.SetActive(true);
		p2Gui.Instance.GetComponent<Canvas>().worldCamera = p2Gui.Instance.cam2;
		p2Scene.Instance.Initialize();
		p2Scene.Instance.Run();
	}
}
