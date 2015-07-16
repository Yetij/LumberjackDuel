using UnityEngine;
using System.Collections;

public class TEST_PREFAB : MonoBehaviour {
	public GameObject pref;
	void OnGUI () {
		if ( GUILayout.Button("Change ") ) {
			pref.GetComponent<TEST_SCRIPT>().i ++;
		}
		if ( GUILayout.Button("Inst ") ) {
			Instantiate(pref);
		}
	}
}
