using UnityEngine;
using UnityEngine.UI;

public class p2UpdateOnAwakeText : MonoBehaviour {

	void Awake () {
		GetComponent<Text>().text = NameMap.get["gameVersion"];
	}
}
