using UnityEngine;
using System.Collections;

public delegate void OnHandle ();

public class v4TestObject : MonoBehaviour {
	event OnHandle onHandle;

	void OnGUI () {
		if ( GUILayout.Button("add event ") ) {
			onHandle += f;
		}
		if ( GUILayout.Button("remove event ") ) {
			onHandle -= f;
		}
	}

	void f () {
		Debug.Log("OnHandle not null");
	}
	void Update () {
		if ( onHandle == null ) return;
		onHandle();
	}
}
