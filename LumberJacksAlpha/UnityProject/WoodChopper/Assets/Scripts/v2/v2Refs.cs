using UnityEngine;
using System.Collections;

public class v2Refs : MonoBehaviour
{
	#region hiden
	private static v2Refs _instance;
	public static v2Refs Instance {
		get {
			if ( _instance == null ) {
				_instance = GameObject.FindObjectOfType(typeof(v2Refs)) as v2Refs;
				if ( _instance == null ) throw new UnityException("Object of type v2Refs not found");
			}
			return _instance;
		}
	}
	#endregion

}

